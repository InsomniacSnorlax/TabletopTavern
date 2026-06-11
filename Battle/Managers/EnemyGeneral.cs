using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;
using TJ.Morale;

namespace TJ
{
    /// <summary>
    /// Orchestrates the enemy AI during battle via a simple state machine.
    ///
    /// States:
    ///   Aggressive        – all squads committed; cavalry flanks once the player closes in.
    ///   Passive           – army holds until the player approaches or deals damage.
    ///   DelayedPassive    – Passive for up to 10 seconds, then forced Aggressive.
    ///   DelayedAggressive – idles for 1 second to let units finish teleporting, then goes Aggressive.
    ///   Garrison          – holds position until a gate is breached.
    ///
    /// Ranged unit behaviour is fully delegated to EnemyRangedBehavior.
    /// Cavalry flanking behaviour is fully delegated to EnemyCavalryBehavior.
    /// Both components must be attached to the same GameObject.
    /// </summary>
    [RequireComponent(typeof(EnemyRangedBehavior))]
    [RequireComponent(typeof(EnemyCavalryBehavior))]
    public class EnemyGeneral : MonoBehaviour
    {
        private enum EnemyGeneralState
        {
            Aggressive,
            Passive,
            DelayedPassive,
            DelayedAggressive,
            Garrison
        }

        [Header("State Machine")]
        [SerializeField] private EnemyGeneralState _currentState;
        [SerializeField] private float stateEvaluationInterval   = 1f;
        [SerializeField] private float aggressionTriggerDistance  = 20f;

        private EnemyRangedBehavior  _rangedBehavior;
        private EnemyCavalryBehavior _cavalryBehavior;

        private EntityManager _entityManager;
        private EntityQuery   _battleHasStartedQuery;
        private EntityQuery   _enemySquadQuery;
        private EntityQuery   _playerSquadQuery;
        private EntityQuery   _balanceOfPowerQuery;

        private float _stateEvaluationTimer;
        private int   _delayedPassiveTimer;
        private int   _delayedAggressiveTimer;
        private bool  _battleHasStarted;
        private bool  _battleHasEnded;
        private bool  _setup;
        private bool  _isRiverCrossing;
        private bool  _isGarrisonBattle;

        // Shielded squads monitored for combat deterioration in the Aggressive state.
        private readonly List<Entity> _shieldedEnemySquads = new();

        #region Lifecycle

        private void Awake()
        {
            // Resolve sub-component references immediately so external callers such as
            // BattleCleanUpManager can safely delegate to them before SetUp() is called.
            _rangedBehavior  = GetComponent<EnemyRangedBehavior>();
            _cavalryBehavior = GetComponent<EnemyCavalryBehavior>();
        }

        public void SetUp()
        {
            _entityManager         = World.DefaultGameObjectInjectionWorld.EntityManager;
            _battleHasStartedQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BattleHasStarted>());
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
            _balanceOfPowerQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BalanceOfPower>());

            _isGarrisonBattle = BattleManager.Instance.BattleSaveManager.IsGarrisonBattle;
            _setup = true;

            _rangedBehavior.SetUp();
            _cavalryBehavior.SetUp();

            BattleManager.Instance.UIManager.BalanceOfPowerDisplay.ArmyLossesTriggered += OnArmyLossesHandler;
        }

        public void TearDown()
        {
            _rangedBehavior.TearDown();
            _cavalryBehavior.TearDown();

            World world = World.DefaultGameObjectInjectionWorld;
            if (world != null && world.IsCreated)
            {
                _battleHasStartedQuery.Dispose();
                _enemySquadQuery.Dispose();
                _playerSquadQuery.Dispose();
                _balanceOfPowerQuery.Dispose();
            }
            _setup = false;
        }

        private void Update()
        {
            if (!_setup || _battleHasEnded) return;

            if (!_battleHasStarted)
            {
                if (_battleHasStartedQuery.CalculateEntityCount() == 0) return;
                _battleHasStarted = true;
                DetermineStartingState();
            }

            _stateEvaluationTimer -= Time.deltaTime;
            if (_stateEvaluationTimer > 0) return;
            _stateEvaluationTimer = stateEvaluationInterval;

            HandleStateMachine();
        }

        private void OnDestroy()
        {
            if (_setup) TearDown();
            if (BattleManager.Instance == null || BattleManager.Instance.UIManager == null) return;
            BattleManager.Instance.UIManager.BalanceOfPowerDisplay.ArmyLossesTriggered -= OnArmyLossesHandler;
        }

        #endregion

        #region Public API

        /// <summary>Called by EntityWatcher when a squad with CavalryFlankingTag is registered.</summary>
        public void MarkSquadForFlanking(int squadId) =>
            _cavalryBehavior.MarkSquadForFlanking(squadId);

        public void SetRiverCrossing(bool isRiverCrossing)
        {
            _isRiverCrossing = isRiverCrossing;
            _cavalryBehavior.SetRiverCrossing(isRiverCrossing);
        }

        /// <summary>Forwarded from BalanceOfPowerDisplay.ArmyLossesTriggered.</summary>
        public void OnArmyLossesHandler(Team team) =>
            _rangedBehavior.OnArmyLossesTriggered(team);

        #endregion

        #region Battle Start

        public void DetermineStartingState()
        {
            _rangedBehavior.SetToVolleyFire();
            _rangedBehavior.InitialiseSquadLists();
            _cavalryBehavior.DetermineInfantryMarchSpeed();

            if (_isGarrisonBattle)
            {
                Debug.Log("[EnemyGeneral] Starting in Garrison state");
                SetGarrison();
                return;
            }

            if (_isRiverCrossing)
            {
                Debug.Log("[EnemyGeneral] Starting Aggressive (river crossing)");
                SetDelayedAggressive();
                return;
            }

            if (_rangedBehavior.ArmyHasArtillery())
            {
                Debug.Log("[EnemyGeneral] Starting Passive — army contains artillery");
                SetPassive();
                return;
            }

            if (ArmyHasOutriders())
            {
                Debug.Log("[EnemyGeneral] Starting Aggressive — army contains outriders");
                SetDelayedAggressive();
                return;
            }

            if (UnityEngine.Random.value < 0.25f)
            {
                Debug.Log("[EnemyGeneral] Starting DelayedPassive (random chance)");
                SetDelayedPassive();
            }
            else
            {
                Debug.Log("[EnemyGeneral] Starting Aggressive (default)");
                SetDelayedAggressive();
            }
        }

        private bool ArmyHasOutriders()
        {
            NativeArray<Entity> enemyEntities = _enemySquadQuery.ToEntityArray(Allocator.Temp);
            bool hasOutrider = false;
            foreach (Entity entity in enemyEntities)
            {
                SquadEntity squadEntity = _entityManager.GetComponentData<SquadEntity>(entity);
                if (TabletopTavernData.Instance.GetSquadStats(squadEntity.UnitName).SquadAttributes.Outrider)
                {
                    hasOutrider = true;
                    break;
                }
            }
            enemyEntities.Dispose();
            return hasOutrider;
        }

        #endregion

        #region State Machine

        private void HandleStateMachine()
        {
            EnemyGeneralState newState = EvaluateState();
            if (newState != _currentState)
            {
                ExitState();
                _currentState = newState;
                EnterState(_currentState);
            }
            UpdateState(_currentState);
        }

        private EnemyGeneralState EvaluateState()
        {
            BalanceOfPower bop   = _balanceOfPowerQuery.GetSingleton<BalanceOfPower>();
            float          ratio = bop.EnemyMaxHealth > 0 ? bop.EnemyCurrentHealth / bop.EnemyMaxHealth : 1f;

            // Force-commit to Aggressive if the army is near-dead while sitting passive.
            if (_currentState == EnemyGeneralState.Passive && ratio < 0.35f)
                return EnemyGeneralState.Aggressive;

            return _currentState;
        }

        private void EnterState(EnemyGeneralState state)
        {
            Debug.Log($"[EnemyGeneral] → {state}");
        }

        private void ExitState() { }

        private void UpdateState(EnemyGeneralState state)
        {
            switch (state)
            {
                case EnemyGeneralState.Aggressive:        UpdateAggressive();        break;
                case EnemyGeneralState.Passive:           UpdatePassive();           break;
                case EnemyGeneralState.DelayedPassive:    UpdateDelayedPassive();    break;
                case EnemyGeneralState.DelayedAggressive: UpdateDelayedAggressive(); break;
                case EnemyGeneralState.Garrison:          UpdateGarrison();          break;
            }
        }

        private void UpdateAggressive()
        {
            CheckShieldedSquadsForDefensiveSwitch();
            _rangedBehavior.Tick();
            _cavalryBehavior.Tick();
            _rangedBehavior.ReprioritizeTargets();
        }

        private void UpdatePassive()
        {
            NativeArray<Entity> enemyEntities  = _enemySquadQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Entity> playerEntities = _playerSquadQuery.ToEntityArray(Allocator.Temp);

            bool playerIsClose = false;
            foreach (Entity enemyEntity in enemyEntities)
            {
                SquadEntity enemySquadEntity = _entityManager.GetComponentData<SquadEntity>(enemyEntity);
                if (TabletopTavernData.Instance.GetSquadStats(enemySquadEntity.UnitName).SquadAttributes.Outrider) continue;

                float3 enemyCenter = _entityManager.GetComponentData<SquadMovementComponent>(enemyEntity).SquadCenter;
                foreach (Entity playerEntity in playerEntities)
                {
                    float3 playerCenter = _entityManager.GetComponentData<SquadMovementComponent>(playerEntity).SquadCenter;
                    if (math.distance(enemyCenter, playerCenter) <= aggressionTriggerDistance)
                    {
                        playerIsClose = true;
                        break;
                    }
                }
                if (playerIsClose) break;
            }

            enemyEntities.Dispose();
            playerEntities.Dispose();

            if (playerIsClose)
            {
                Debug.Log($"[EnemyGeneral] Player within {aggressionTriggerDistance} units — going Aggressive");
                SetAggressive();
            }
            else if (BattleManager.Instance.UIManager.BalanceOfPowerDisplay.EnemyHasTakenDamage)
            {
                Debug.Log("[EnemyGeneral] Enemy took damage — going Aggressive");
                SetAggressive();
            }

            _rangedBehavior.ReprioritizeTargets();
        }

        private void UpdateDelayedPassive()
        {
            UpdatePassive();
            _delayedPassiveTimer += 1;

            // Prevent the player from avoiding combat indefinitely by forcing Aggressive after 10 ticks.
            if (_delayedPassiveTimer >= 10)
            {
                Debug.Log("[EnemyGeneral] DelayedPassive expired — going Aggressive");
                SetAggressive();
            }
        }

        private void UpdateDelayedAggressive()
        {
            _delayedAggressiveTimer += 1;
            if (_delayedAggressiveTimer >= 1)
            {
                Debug.Log("[EnemyGeneral] DelayedAggressive expired — going Aggressive");
                SetAggressive();
            }
        }

        private void UpdateGarrison()
        {
            if (BattleManager.Instance.AnyGateBreached)
                SetAggressive();
        }

        #endregion

        #region State Entry

        private void SetAggressive()
        {
            _currentState = EnemyGeneralState.Aggressive;
            _shieldedEnemySquads.Clear();
            _cavalryBehavior.OnAggressiveStarted();

            NativeArray<Entity> enemyEntities = _enemySquadQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in enemyEntities)
            {
                SquadEntity squadEntity = _entityManager.GetComponentData<SquadEntity>(entity);

                // Cavalry behavior claims flanking squads and handles their own setup.
                if (_cavalryBehavior.TryRegisterFlankingSquad(entity, squadEntity)) continue;

                _entityManager.SetComponentEnabled<WaitingForCommand>(entity, false);

                SquadAttributes attrs = TabletopTavernData.Instance.GetSquadStats(squadEntity.UnitName).SquadAttributes;
                if (attrs.StandardShields || attrs.HeavyShields || attrs.TowerShields)
                    _shieldedEnemySquads.Add(entity);
            }
            enemyEntities.Dispose();
        }

        private void SetPassive()           => _currentState = EnemyGeneralState.Passive;
        private void SetDelayedPassive()    => _currentState = EnemyGeneralState.DelayedPassive;
        private void SetGarrison()          => _currentState = EnemyGeneralState.Garrison;
        private void SetDelayedAggressive() { _delayedAggressiveTimer = 0; _currentState = EnemyGeneralState.DelayedAggressive; }

        #endregion

        #region Shielded Squad Monitoring

        /// <summary>
        /// When a shielded squad starts losing its melee fight, orders it to switch to
        /// Defensive stance to reduce incoming damage.
        /// </summary>
        private void CheckShieldedSquadsForDefensiveSwitch()
        {
            for (int i = _shieldedEnemySquads.Count - 1; i >= 0; i--)
            {
                Entity entity = _shieldedEnemySquads[i];
                if (!_entityManager.Exists(entity))                          { _shieldedEnemySquads.RemoveAt(i); continue; }
                if (!_entityManager.HasComponent<InCombat>(entity))          continue;
                if (!_entityManager.HasComponent<HealthLossPercent>(entity)) continue;

                HealthLossPercent healthLoss = _entityManager.GetComponentData<HealthLossPercent>(entity);
                if (healthLoss.CombatStatus != CombatStatus.Losing) continue;

                SquadEntity squadEntity = _entityManager.GetComponentData<SquadEntity>(entity);
                BattleManager.Instance.SquadManager.SetDefensiveStancForEntity(squadEntity);
                _shieldedEnemySquads.RemoveAt(i);
            }
        }

        #endregion
    }
}
