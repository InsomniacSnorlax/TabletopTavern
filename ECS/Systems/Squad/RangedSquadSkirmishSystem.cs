using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
[WithAll(typeof(RangedSquadSkirmishTag))]
[WithAll(typeof(RangedSquad))]
[WithNone(typeof(PlayerSquad))]
[WithNone(typeof(GarrisonDefenderComponent))]
partial struct RangedSquadSkirmishJob : IJobEntity
{
    [ReadOnly] public NativeArray<Entity> AllEnemySquadEntities;
    [ReadOnly] public ComponentLookup<SquadEntity> SquadLookup;
    [ReadOnly] public ComponentLookup<SquadMovementComponent> MovementLookup;
    public EntityCommandBuffer.ParallelWriter ECB;
    public float FleeDistance;

    [BurstCompile]
    public void Execute(Entity entity, in SquadEntity squad, [ChunkIndexInQuery] int chunkIndex)
    {
        float3 centerOfSquad = MovementLookup[squad.SelfEntity].SquadCenter;

        float closestDistance = float.MaxValue;
        Entity closestEnemySquadEntity = Entity.Null;
        bool searchForEnemySquads = squad.SquadId > 0;

        for (int i = 0; i < AllEnemySquadEntities.Length; i++)
        {
            Entity enemyEntity = AllEnemySquadEntities[i];
            SquadEntity enemySquad = SquadLookup[enemyEntity];

            if (searchForEnemySquads)
            {
                if (enemySquad.SquadId > 0) continue;
            }
            else
            {
                if (enemySquad.SquadId < 0) continue;
            }

            // if (MovementLookup.HasComponent<SquadMoveOverrideTag>(enemySquad.SelfEntity)) continue; // Assuming lookup if needed, but commented

            float3 centerOfTarget = MovementLookup[enemySquad.SelfEntity].SquadCenter;

            float distance = math.distance(centerOfSquad, centerOfTarget);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemySquadEntity = enemySquad.SelfEntity;
            }
        }

        if (closestEnemySquadEntity == Entity.Null) return;

        if (closestDistance < FleeDistance)
        {
            ECB.AddComponent(chunkIndex, entity, new IssueSquadCommand
            {
                SquadCommand = SquadCommand.Retreat,
            });
            ECB.RemoveComponent<RangedSquadSkirmishTag>(chunkIndex, squad.SelfEntity);
        }
    }
}

[BurstCompile]
public partial struct RangedSquadSkirmishSystem : ISystem
{
    private EntityQuery _enemySquadsQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();

        _enemySquadsQuery = state.GetEntityQuery(ComponentType.ReadOnly<SquadEntity>(), ComponentType.Exclude<BrokenSquadTag>(), ComponentType.Exclude<DestroyEntityTag>());
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var squadLookup = SystemAPI.GetComponentLookup<SquadEntity>(true);
        var movementLookup = SystemAPI.GetComponentLookup<SquadMovementComponent>(true);

        var allEnemySquadEntities = _enemySquadsQuery.ToEntityArray(Allocator.TempJob);

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var job = new RangedSquadSkirmishJob
        {
            AllEnemySquadEntities = allEnemySquadEntities,
            SquadLookup = squadLookup,
            MovementLookup = movementLookup,
            ECB = ecb,
            FleeDistance = TabletopTavernConstants.ARCHER_FLEE_DISTANCE
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
        allEnemySquadEntities.Dispose(state.Dependency);
    }
}