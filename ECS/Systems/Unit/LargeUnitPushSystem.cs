using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectDawn.Navigation;

/// <summary>
/// Directly displaces stopped small units that a moving large unit physically overlaps.
///
/// Why this is necessary:
///   AgentLocomotionSystem returns early for IsStopped agents, so separation forces written to
///   body.Force are never applied. AgentColliderSystem skips the stopped-vs-moving pair entirely.
///   Neither built-in system can move stopped infantry out of a large unit's path.
///
/// Runs single-threaded (Schedule, not ScheduleParallel) so writes to arbitrary entities'
/// LocalTransform are safe without further synchronisation.
/// </summary>
[BurstCompile]
[UpdateInGroup(typeof(AgentDisplacementSystemGroup), OrderLast = true)]
partial struct LargeUnitPushSystem : ISystem
{
    // Max displacement (Unity units) applied per frame when a large unit fully overlaps a small unit.
    // Scales down to 0 at the edge of contact. No upper limit — higher = more aggressive scatter.
    const float PushStrength = 30f;

    ComponentLookup<LocalTransform>    m_TransformLookup;
    ComponentLookup<ResistKnockbackTag> m_ResistLookup;
    ComponentLookup<EntityTeam>         m_TeamLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattleHasStarted>();
        m_TransformLookup = state.GetComponentLookup<LocalTransform>(isReadOnly: false);
        m_ResistLookup    = state.GetComponentLookup<ResistKnockbackTag>(isReadOnly: true);
        m_TeamLookup      = state.GetComponentLookup<EntityTeam>(isReadOnly: true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        m_TransformLookup.Update(ref state);
        m_ResistLookup.Update(ref state);
        m_TeamLookup.Update(ref state);

        new LargeUnitPushJob
        {
            Spatial         = SystemAPI.GetSingleton<AgentSpatialPartitioningSystem.Singleton>(),
            TransformLookup = m_TransformLookup,
            ResistLookup    = m_ResistLookup,
            TeamLookup      = m_TeamLookup,
            DeltaTime       = SystemAPI.Time.DeltaTime,
        }.Schedule();
    }

    // Only large, moving units act as pushers.
    [BurstCompile]
    [WithAll(typeof(LargeTag), typeof(Agent))]
    partial struct LargeUnitPushJob : IJobEntity
    {
        [ReadOnly]
        public AgentSpatialPartitioningSystem.Singleton Spatial;

        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<LocalTransform> TransformLookup;

        [ReadOnly]
        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<ResistKnockbackTag> ResistLookup;

        [ReadOnly]
        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<EntityTeam> TeamLookup;

        public float DeltaTime;

        public void Execute(
            Entity entity,
            in Unit unit,
            in AgentBody body,
            in AgentShape shape,
            in LocalTransform transform)
        {
            if (body.IsStopped) return;
            if (unit.unitState != UnitState.Moving && unit.unitState != UnitState.Charge) return;

            // Query only infantry/artillery (NavigationLayers.Default) — large units sit on
            // Layer1/Layer2 after Fix 1 so they are excluded from this query automatically.
            float queryRadius = shape.Radius + 1.6f;

            var action = new PushAction
            {
                LargeTransform  = transform,
                LargeShape      = shape,
                TransformLookup = TransformLookup,
                ResistLookup    = ResistLookup,
                TeamLookup      = TeamLookup,
                LargeTeam       = unit.Team,
                PushStrength    = PushStrength,
                DeltaTime       = DeltaTime,
            };

            Spatial.QueryCylinder(
                transform.Position, queryRadius, shape.Height,
                Spatial.QueryCapacity, ref action, NavigationLayers.Default);

            // Apply accumulated counter-force from overlapping enemy infantry.
            if (math.lengthsq(action.EnemyResistance) > 1e-8f)
            {
                var t = TransformLookup[entity];
                t.Position += new float3(action.EnemyResistance.x, 0f, action.EnemyResistance.y);
                TransformLookup[entity] = t;
            }
        }
    }

    struct PushAction : ISpatialQueryEntity
    {
        public LocalTransform LargeTransform;
        public AgentShape LargeShape;
        public float PushStrength;
        public float2 EnemyResistance;
        public float DeltaTime;

        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<LocalTransform> TransformLookup;

        [ReadOnly]
        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<ResistKnockbackTag> ResistLookup;

        [ReadOnly]
        [NativeDisableContainerSafetyRestriction]
        public ComponentLookup<EntityTeam> TeamLookup;

        public Team LargeTeam;

        public void Execute(
            Entity otherEntity,
            AgentBody otherBody,
            AgentShape otherShape,
            LocalTransform otherTransform)
        {
            float2 towards  = otherTransform.Position.xz - LargeTransform.Position.xz;
            float  distance = math.length(towards);
            float  contact  = LargeShape.Radius + otherShape.Radius;

            if (distance >= contact || distance < 1e-4f) return;

            float  penetration = (contact - distance) / contact;
            float2 dir         = math.normalizesafe(towards);

            // Scale by deltaTime and clamp to actual overlap so we never overshoot the contact
            // surface — overshooting causes the flicker loop (push → nav corrects → push again).
            float push = math.min(penetration * PushStrength * DeltaTime, contact - distance);

            bool isEnemy = TeamLookup.TryGetComponent(otherEntity, out EntityTeam otherTeam) && otherTeam.Value != LargeTeam;

            if (isEnemy)
            {
                // Accumulate pushback on the large unit — applied after the query loop.
                EnemyResistance -= dir * push;
                return;
            }

            // Friendly infantry — push them aside whether moving or stopped.
            if (ResistLookup.HasComponent(otherEntity)) return;

            var t = TransformLookup[otherEntity];
            t.Position += new float3(dir.x, 0f, dir.y) * push;
            TransformLookup[otherEntity] = t;
        }
    }
}
