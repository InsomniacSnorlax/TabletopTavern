using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using ProjectDawn.Navigation;

namespace TJ
{
    /// <summary>
    /// Manages pre-battle cavalry flanking during battle.
    ///
    /// Cavalry squads registered via MarkSquadForFlanking() are held in a pending state
    /// when the army enters Aggressive. Rather than rushing to the flank immediately, they
    /// march forward alongside the infantry at a capped speed. Once the player army closes
    /// within CavalryReleaseDistance of the infantry front line, their speed is restored and
    /// they break into a full flank charge at either side of the enemy formation.
    ///
    /// Outrider squads are explicitly excluded from this system and advance normally.
    /// </summary>
    public class EnemyCavalryBehavior : MonoBehaviour
    {
        [Header("Flank Release")]
        [Tooltip("Distance (world units) between the player and the enemy infantry centroid that triggers the flank.")]
        [SerializeField] private float _cavalryReleaseDistance = 55f;

        [Header("March Speed")]
        [Tooltip("Fallback march speed cap used when the enemy army contains no infantry.")]
        [SerializeField] private float _cavalryMarchSpeedCapFallback = 3.5f;

        [Header("Flank Position")]
        [Tooltip("Padding (world units) added beyond the outermost infantry squad when computing flank waypoints.")]
        [SerializeField] private float _flankPadding = 20f;

        private EntityManager _entityManager;
        private EntityQuery   _enemySquadQuery;
        private EntityQuery   _playerSquadQuery;

        // Squad IDs registered for flanking at spawn time, populated by EntityWatcher.
        private readonly List<int> _squadsMarkedForFlanking = new();

        // Squads currently held in the march phase before the flank release.
        private readonly List<Entity> _pendingFlankSquads = new();

        // Original AgentLocomotion.Speed per squad entity, cached before the march cap is applied.
        private readonly Dictionary<Entity, float> _cachedCavalrySpeeds = new();

        // Speed cap applied during the march phase; set from the slowest infantry at battle start.
        private float _cavalryMarchSpeedCap;

        private bool _isRiverCrossing;
        private bool _flanksReleased;

        #region Lifecycle

        public void SetUp()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _enemySquadQuery = _entityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All  = new[] { ComponentType.ReadOnly<EnemySquad>(), ComponentType.ReadOnly<SquadEntity>() },
                None = new[] { ComponentType.ReadOnly<BrokenSquadTag>() }
            });
            _playerSquadQuery = _entityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All  = new[] { ComponentType.ReadOnly<PlayerSquad>(), ComponentType.ReadOnly<SquadEntity>() },
                None = new[] { ComponentType.ReadOnly<BrokenSquadTag>() }
            });
            _cavalryMarchSpeedCap = _cavalryMarchSpeedCapFallback;
        }

        public void TearDown()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated)
            {
                _enemySquadQuery.Dispose();
                _playerSquadQuery.Dispose();
            }
            _pendingFlankSquads.Clear();
            _cachedCavalrySpeeds.Clear();
        }

        #endregion

        #region Public API

        /// <summary>Called by EntityWatcher when a squad with CavalryFlankingTag is created.</summary>
        public void MarkSquadForFlanking(int squadId) =>
            _squadsMarkedForFlanking.Add(squadId);

        public void SetRiverCrossing(bool isRiverCrossing) =>
            _isRiverCrossing = isRiverCrossing;

        /// <summary>
        /// Reads the slowest infantry speed in the enemy army and uses it as the march speed cap.
        /// Call once during DetermineStartingState so cavalry cannot outpace the infantry line.
        /// </summary>
        public void DetermineInfantryMarchSpeed()
        {
            float slowest = float.MaxValue;
            NativeArray<Entity> enemyEntities = _enemySquadQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in enemyEntities)
            {
                SquadEntity squadEntity = _entityManager.GetComponentData<SquadEntity>(entity);
                if (TabletopTavernData.Instance.GetUnitSizeFromUnitName(squadEntity.UnitName) != UnitSize.Infantry) continue;
                float speed = TabletopTavernData.Instance.GetSquadStats(squadEntity.UnitName).Speed / 10f;
                if (speed < slowest) slowest = speed;
            }
            enemyEntities.Dispose();

            bool foundInfantry    = slowest < float.MaxValue;
            _cavalryMarchSpeedCap = foundInfantry ? slowest : _cavalryMarchSpeedCapFallback;
            // Debug.Log($"[EnemyCavalryBehavior] March speed cap: {_cavalryMarchSpeedCap:F2} " +
                    //   $"({(foundInfantry ? "slowest infantry" : "fallback default")})");
        }

        /// <summary>
        /// Called by EnemyGeneral when the Aggressive state is entered.
        /// Resets the pending flank list so TryRegisterFlankingSquad can repopulate it.
        /// </summary>
        public void OnAggressiveStarted()
        {
            _pendingFlankSquads.Clear();
            _flanksReleased = false;
        }

        /// <summary>
        /// Attempts to register an enemy squad as a pending flank squad.
        /// Returns true if the squad was claimed; the caller should skip normal Aggressive setup for it.
        /// Returns false for outriders (they advance with the infantry) and unmarked squads.
        /// </summary>
        public bool TryRegisterFlankingSquad(Entity entity, SquadEntity squadEntity)
        {
            if (!_squadsMarkedForFlanking.Contains(squadEntity.SquadId)) return false;

            SquadAttributes attrs = TabletopTavernData.Instance.GetSquadStats(squadEntity.UnitName).SquadAttributes;
            if (attrs.Outrider) return false;

            _pendingFlankSquads.Add(entity);
            _entityManager.SetComponentEnabled<WaitingForCommand>(entity, false);
            ApplyCavalryMarchSpeed(entity);
            return true;
        }

        /// <summary>
        /// Per-evaluation-tick update. Advances cavalry alongside the infantry and releases
        /// them to flank when the player closes in. Call from UpdateAggressive.
        /// </summary>
        public void Tick()
        {
            if (_flanksReleased || _pendingFlankSquads.Count == 0) return;
            MarchCavalryWithInfantry();
            CheckForFlankRelease();
        }

        /// <summary>
        /// Issues the two-step flank move orders (wide staging position → deep behind player).
        /// Public so it can be called for immediate flanks in non-standard battle configurations.
        /// </summary>
        public void OrderSquadToFlank(SquadEntity squadEntity)
        {
            EntityManager entityManager       = World.DefaultGameObjectInjectionWorld.EntityManager;
            SquadMovementComponent movement   = entityManager.GetComponentData<SquadMovementComponent>(squadEntity.SelfEntity);
            DynamicBuffer<QueuedOrder> orders = entityManager.GetBuffer<QueuedOrder>(squadEntity.SelfEntity);

            float sideX = ComputeFlankX(movement.SquadCenter.x);

            float3 widePosition = new(sideX, 0f, movement.SquadCenter.z - 10f);
            float3 farPosition  = new(sideX, 0f, -75f);

            orders.Clear();

            if (!_isRiverCrossing)
            {
                orders.Add(new QueuedOrder
                {
                    Type          = QueuedOrderType.Move,
                    Status        = QueuedOrderStatus.Pending,
                    Goal          = widePosition,
                    Rotation      = quaternion.LookRotationSafe(widePosition - movement.SquadCenter, math.up()),
                    WidthAndDepth = new int2(8, 4)
                });
            }

            orders.Add(new QueuedOrder
            {
                Type          = QueuedOrderType.Move,
                Status        = QueuedOrderStatus.Pending,
                Goal          = farPosition,
                Rotation      = quaternion.LookRotationSafe(farPosition - movement.SquadCenter, math.up()),
                WidthAndDepth = new int2(8, 4)
            });
        }

        #endregion

        #region March Phase

        /// <summary>
        /// Issues a fresh move order each evaluation tick so cavalry keeps pace with the
        /// infantry advance at their own X position, without drifting into the melee line.
        /// </summary>
        private void MarchCavalryWithInfantry()
        {
            float3 meleeCentroid = ComputeInfantryMeleeCentroid();
            if (meleeCentroid.Equals(float3.zero)) return;

            for (int i = _pendingFlankSquads.Count - 1; i >= 0; i--)
            {
                Entity entity = _pendingFlankSquads[i];
                if (!_entityManager.Exists(entity))
                {
                    _cachedCavalrySpeeds.Remove(entity);
                    _pendingFlankSquads.RemoveAt(i);
                    continue;
                }
                if (_entityManager.HasComponent<BrokenSquadTag>(entity))
                {
                    RestoreCavalrySpeed(entity);
                    _pendingFlankSquads.RemoveAt(i);
                    continue;
                }

                SquadMovementComponent movement = _entityManager.GetComponentData<SquadMovementComponent>(entity);
                float3 target    = new float3(movement.SquadCenter.x, 0f, meleeCentroid.z);
                float3 direction = target - movement.SquadCenter;
                if (math.lengthsq(direction) < 0.01f) continue;

                DynamicBuffer<QueuedOrder> orders = _entityManager.GetBuffer<QueuedOrder>(entity);
                orders.Clear();
                orders.Add(new QueuedOrder
                {
                    Type          = QueuedOrderType.Move,
                    Status        = QueuedOrderStatus.Pending,
                    Goal          = target,
                    Rotation      = quaternion.LookRotationSafe(direction, math.up()),
                    WidthAndDepth = movement.SquadWidthAndDepth
                });
            }
        }

        #endregion

        #region Flank Release

        /// <summary>
        /// Checks whether the player army has reached the infantry front line.
        /// When triggered, restores full cavalry speed and issues flank orders.
        /// </summary>
        private void CheckForFlankRelease()
        {
            float3 meleeCentroid = ComputeInfantryMeleeCentroid();

            bool release;
            if (meleeCentroid.Equals(float3.zero))
            {
                // No surviving infantry to pace against — release immediately so cavalry
                // does not freeze waiting for a centroid that will never exist.
                release = true;
            }
            else
            {
                NativeArray<Entity> playerEntities = _playerSquadQuery.ToEntityArray(Allocator.Temp);
                release = false;
                foreach (Entity playerEntity in playerEntities)
                {
                    float3 playerCenter = _entityManager.GetComponentData<SquadMovementComponent>(playerEntity).SquadCenter;
                    if (math.distance(playerCenter, meleeCentroid) <= _cavalryReleaseDistance)
                    {
                        release = true;
                        break;
                    }
                }
                playerEntities.Dispose();
            }

            if (!release) return;

            _flanksReleased = true;
            // Debug.Log($"[EnemyCavalryBehavior] Player within {_cavalryReleaseDistance} units — releasing {_pendingFlankSquads.Count} cavalry to flank");

            for (int i = _pendingFlankSquads.Count - 1; i >= 0; i--)
            {
                Entity entity = _pendingFlankSquads[i];
                if (!_entityManager.Exists(entity))                      { _pendingFlankSquads.RemoveAt(i); continue; }
                if (_entityManager.HasComponent<BrokenSquadTag>(entity)) { RestoreCavalrySpeed(entity); _pendingFlankSquads.RemoveAt(i); continue; }

                SquadEntity squadEntity = _entityManager.GetComponentData<SquadEntity>(entity);
                RestoreCavalrySpeed(entity);
                OrderSquadToFlank(squadEntity);
                _pendingFlankSquads.RemoveAt(i);
            }
        }

        #endregion

        #region Centroid and Flank X Calculation

        /// <summary>
        /// Returns the average world position of all non-outrider, non-pending-cavalry enemy
        /// squads. Used as the infantry front line reference for march pacing and release detection.
        /// Returns float3.zero if no qualifying squads exist.
        /// </summary>
        private float3 ComputeInfantryMeleeCentroid()
        {
            NativeArray<Entity> enemyEntities = _enemySquadQuery.ToEntityArray(Allocator.Temp);
            float3 centroid = float3.zero;
            int    count    = 0;

            foreach (Entity e in enemyEntities)
            {
                if (_pendingFlankSquads.Contains(e)) continue;

                SquadEntity se = _entityManager.GetComponentData<SquadEntity>(e);
                if (TabletopTavernData.Instance.GetSquadStats(se.UnitName).SquadAttributes.Outrider) continue;

                centroid += _entityManager.GetComponentData<SquadMovementComponent>(e).SquadCenter;
                count++;
            }
            enemyEntities.Dispose();

            return count > 0 ? centroid / count : float3.zero;
        }

        /// <summary>
        /// Computes the X coordinate for flank waypoints by measuring the current width of the
        /// enemy infantry formation and adding _flankPadding beyond each edge.
        /// Cavalry already on the right flanks right; cavalry near centre picks randomly.
        /// </summary>
        private float ComputeFlankX(float cavalryCurrentX)
        {
            float leftEdge  = -10f;
            float rightEdge =  10f;

            NativeArray<Entity> enemyEntities = _enemySquadQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity e in enemyEntities)
            {
                if (_pendingFlankSquads.Contains(e)) continue;
                SquadMovementComponent em = _entityManager.GetComponentData<SquadMovementComponent>(e);
                if (em.BoundsMin.x < leftEdge)  leftEdge  = em.BoundsMin.x;
                if (em.BoundsMax.x > rightEdge) rightEdge = em.BoundsMax.x;
            }
            enemyEntities.Dispose();

            float rightFlankX = rightEdge + _flankPadding;
            float leftFlankX  = leftEdge  - _flankPadding;

            if (math.abs(cavalryCurrentX) < 10f)
                return UnityEngine.Random.value >= 0.5f ? rightFlankX : leftFlankX;

            return cavalryCurrentX >= 0f ? rightFlankX : leftFlankX;
        }

        #endregion

        #region March Speed Management

        /// <summary>
        /// Caps each unit entity's AgentLocomotion.Speed to the infantry march speed and
        /// caches the original value so it can be restored when cavalry is released to flank.
        /// </summary>
        private void ApplyCavalryMarchSpeed(Entity squadEntity)
        {
            if (!_entityManager.HasBuffer<EntityReferenceBufferElement>(squadEntity)) return;

            DynamicBuffer<EntityReferenceBufferElement> unitBuffer =
                _entityManager.GetBuffer<EntityReferenceBufferElement>(squadEntity);

            bool speedCached = false;
            for (int i = 0; i < unitBuffer.Length; i++)
            {
                Entity unitEntity = unitBuffer[i].Entity;
                if (!_entityManager.Exists(unitEntity)) continue;
                if (!_entityManager.HasComponent<AgentLocomotion>(unitEntity)) continue;

                AgentLocomotion loc = _entityManager.GetComponentData<AgentLocomotion>(unitEntity);

                if (!speedCached)
                {
                    _cachedCavalrySpeeds[squadEntity] = loc.Speed;
                    Debug.Log($"[EnemyCavalryBehavior] March cap applied — original: {loc.Speed:F2}, cap: {_cavalryMarchSpeedCap:F2}");
                    speedCached = true;
                }

                if (loc.Speed > _cavalryMarchSpeedCap)
                {
                    loc.Speed = _cavalryMarchSpeedCap;
                    _entityManager.SetComponentData(unitEntity, loc);
                }
            }
        }

        /// <summary>
        /// Restores each unit entity's AgentLocomotion.Speed to the pre-march cached value,
        /// allowing cavalry to accelerate to full speed for the flank charge.
        /// </summary>
        private void RestoreCavalrySpeed(Entity squadEntity)
        {
            if (!_cachedCavalrySpeeds.TryGetValue(squadEntity, out float originalSpeed))
            {
                _cachedCavalrySpeeds.Remove(squadEntity);
                return;
            }
            _cachedCavalrySpeeds.Remove(squadEntity);

            if (!_entityManager.HasBuffer<EntityReferenceBufferElement>(squadEntity)) return;

            DynamicBuffer<EntityReferenceBufferElement> unitBuffer =
                _entityManager.GetBuffer<EntityReferenceBufferElement>(squadEntity);

            for (int i = 0; i < unitBuffer.Length; i++)
            {
                Entity unitEntity = unitBuffer[i].Entity;
                if (!_entityManager.Exists(unitEntity)) continue;
                if (!_entityManager.HasComponent<AgentLocomotion>(unitEntity)) continue;

                AgentLocomotion loc = _entityManager.GetComponentData<AgentLocomotion>(unitEntity);
                loc.Speed = originalSpeed;
                _entityManager.SetComponentData(unitEntity, loc);
            }
        }

        #endregion
    }
}
