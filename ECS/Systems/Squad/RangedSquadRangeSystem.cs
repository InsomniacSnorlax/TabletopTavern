using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(DestroySquadSystem))]
partial struct RangedSquadRangeSystem : ISystem
{
    private ComponentLookup<ShootAttack> _shootAttackLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _shootAttackLookup = state.GetComponentLookup<ShootAttack>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _shootAttackLookup.Update(ref state);
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged)
            .AsParallelWriter();

        state.Dependency = new RangedSquadRangeJob
        {
            ShootAttackLookup = _shootAttackLookup,
            Ecb = ecb
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
partial struct RangedSquadRangeJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<ShootAttack> ShootAttackLookup;
    public EntityCommandBuffer.ParallelWriter Ecb;

    public void Execute([ChunkIndexInQuery] int sortKey, Entity entity,
        DynamicBuffer<EntityReferenceBufferElement> entityBuffer, ref RangedSquad rangedSquad)
    {
        if (entityBuffer.Length == 0) return;
        Entity unitEntity = entityBuffer[0].Entity;
        if (!ShootAttackLookup.HasComponent(unitEntity)) return;

        var range = ShootAttackLookup[unitEntity].Range;
        var effectiveRange = range < TabletopTavernConstants.MINIMUM_ARCHER_RANGE
            ? TabletopTavernConstants.MINIMUM_ARCHER_RANGE
            : range;

        if (effectiveRange == rangedSquad.AttackRange) return;

        rangedSquad.AttackRange = effectiveRange;
        Ecb.AddComponent(sortKey, entity, new ArcherRangeUpdated());
    }
}
