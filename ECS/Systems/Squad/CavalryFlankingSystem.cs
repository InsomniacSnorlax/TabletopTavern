using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

partial struct CavalryFlankingSystem : ISystem
{
    private EntityQuery _playerSquadQuery;
    private ComponentLookup<SquadMovementComponent> _squadMovementLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattleHasStarted>();
        _playerSquadQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<SquadMovementComponent>()
            .WithNone<EnemySquad>()
            .WithNone<BrokenSquadTag>()
            .Build(ref state);
        _squadMovementLookup = state.GetComponentLookup<SquadMovementComponent>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _squadMovementLookup.Update(ref state);

        // ToEntityArray reads only entity IDs (chunk metadata), not component data,
        // so it does not force completion of SquadCenterJob. The lookup handles that dependency.
        var playerEntities = _playerSquadQuery.ToEntityArray(Allocator.TempJob);
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        state.Dependency = new CavalryFlankingJob
        {
            PlayerEntities      = playerEntities,
            SquadMovementLookup = _squadMovementLookup,
            Ecb                 = ecb
        }.ScheduleParallel(state.Dependency);

        playerEntities.Dispose(state.Dependency);
    }
}

[BurstCompile]
[WithPresent(typeof(CavalryFlankingTag))]
partial struct CavalryFlankingJob : IJobEntity
{
    [ReadOnly] public NativeArray<Entity> PlayerEntities;
    [ReadOnly] public ComponentLookup<SquadMovementComponent> SquadMovementLookup;
    public EntityCommandBuffer.ParallelWriter Ecb;

    public void Execute([ChunkIndexInQuery] int sortKey,
        ref SquadEntity squad, in SquadMovementComponent movement,
        DynamicBuffer<QueuedOrder> queuedOrders)
    {
        float closestDist = float.MaxValue;
        Entity closestEntity = Entity.Null;
        for (int i = 0; i < PlayerEntities.Length; i++)
        {
            if (!SquadMovementLookup.HasComponent(PlayerEntities[i])) continue;
            float3 playerCenter = SquadMovementLookup[PlayerEntities[i]].SquadCenter;
            float d = math.distance(playerCenter, movement.SquadCenter);
            if (d < 20f && d < closestDist)
            {
                closestDist   = d;
                closestEntity = PlayerEntities[i];
            }
        }

        bool isBehind = closestEntity != Entity.Null;

        // Pass 2: only check z-positions if no interceptor found
        if (!isBehind)
        {
            isBehind = true;
            for (int i = 0; i < PlayerEntities.Length; i++)
            {
                if (!SquadMovementLookup.HasComponent(PlayerEntities[i])) continue;
                if (movement.SquadCenter.z > SquadMovementLookup[PlayerEntities[i]].SquadCenter.z)
                {
                    isBehind = false;
                    break;
                }
            }
        }

        if (movement.SquadCenter.z < -70f)
            isBehind = true;

        if (!isBehind) return;

        // If intercepted, immediately re-target the closest intercepting squad and charge it.
        // Without this, cavalry would wait up to 0.5s for EnemyArmyTargetOverrideSystem
        // to re-target, during which it keeps moving in the wrong direction.
        if (closestEntity != Entity.Null)
        {
            squad.TargetSquadEntity = closestEntity;
            Ecb.AddComponent<StartChargeTag>(sortKey, squad.SelfEntity);
        }

        Ecb.RemoveComponent<CavalryFlankingTag>(sortKey, squad.SelfEntity);
        Ecb.AddComponent<CancelSquadMoveOverrideTag>(sortKey, squad.SelfEntity);
        queuedOrders.Clear();
    }
}
