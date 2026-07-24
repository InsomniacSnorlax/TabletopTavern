using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
partial struct SquadTotalHealthSystem : ISystem
{
    private ComponentLookup<Health> healthComponentLookup;
    private ComponentLookup<UnitStatsSetUpTag> awaitingStatsSetUpLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        healthComponentLookup = state.GetComponentLookup<Health>(true);
        awaitingStatsSetUpLookup = state.GetComponentLookup<UnitStatsSetUpTag>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        healthComponentLookup.Update(ref state);
        awaitingStatsSetUpLookup.Update(ref state);
        SquadTotalHealthJob squadTotalHealthJob = new SquadTotalHealthJob
        {
            HealthLookup = healthComponentLookup,
            AwaitingStatsSetUpLookup = awaitingStatsSetUpLookup
        };
        state.Dependency = squadTotalHealthJob.ScheduleParallel(state.Dependency);

        // EntityManager entityManager = state.EntityManager;

        // foreach (var ( 
        //     squadTotalHealth,
        //     entityBuffer
        // ) in SystemAPI.Query<
        //     RefRW<SquadTotalHealth>,
        //     DynamicBuffer<EntityReferenceBufferElement>
        // >()) {
            
        //     int totalHealth = 0;
        //     for (int i = 0; i < entityBuffer.Length; i++)
        //     {
        //         Entity entity = entityBuffer[i].Entity;
        //         totalHealth += entityManager.GetComponentData<Health>(entity).Value;
        //     }
        //     squadTotalHealth.ValueRW.CurrentValue = totalHealth;
        // }
    }
}

[BurstCompile]
public partial struct SquadTotalHealthJob : IJobEntity {
    [ReadOnly] public ComponentLookup<Health> HealthLookup;
    [ReadOnly] public ComponentLookup<UnitStatsSetUpTag> AwaitingStatsSetUpLookup;
    public void Execute (in DynamicBuffer<EntityReferenceBufferElement> entityBuffer, ref SquadStateComponent squadTotalHealth) {
        int totalHealth = 0;
        for (int i = 0; i < entityBuffer.Length; i++) {
            Entity entity = entityBuffer[i].Entity;

            // A squad's units sit in this buffer before UnitSetUpSystem has assigned their Health,
            // so summing now would yield 0 and overwrite the correct value RegisterSquad set. For a
            // squad spawned mid-battle that reads as a full-health loss in HealthLossTrackingSystem,
            // which craters its morale for the 5 seconds that event stays in the buffer. Leave the
            // squad's health untouched until every unit has been set up.
            if(AwaitingStatsSetUpLookup.HasComponent(entity)) return;

            if(!HealthLookup.HasComponent(entity)) continue; // Check if the entity has a Health component
            totalHealth += HealthLookup[entity].Value;
        }
        squadTotalHealth.CurrentHealthValue = totalHealth;
    }
}