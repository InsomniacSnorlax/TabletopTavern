using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
partial struct SquadTotalHealthSystem : ISystem
{
    private ComponentLookup<Health> healthComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        healthComponentLookup = state.GetComponentLookup<Health>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        healthComponentLookup.Update(ref state);
        SquadTotalHealthJob squadTotalHealthJob = new SquadTotalHealthJob
        {
            HealthLookup = healthComponentLookup
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
    public void Execute (in DynamicBuffer<EntityReferenceBufferElement> entityBuffer, ref SquadStateComponent squadTotalHealth) {
        int totalHealth = 0;
        for (int i = 0; i < entityBuffer.Length; i++) {
            Entity entity = entityBuffer[i].Entity;
            if(!HealthLookup.HasComponent(entity)) continue; // Check if the entity has a Health component
            totalHealth += HealthLookup[entity].Value;
        }
        squadTotalHealth.CurrentHealthValue = totalHealth;
    }
}