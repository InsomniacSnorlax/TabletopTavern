using Unity.Burst;
using Unity.Entities;

partial struct CeaseFireSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (squadEntity, entityBuffer, ceaseFireRequested) in SystemAPI.Query<
            RefRW<SquadEntity>,
            DynamicBuffer<EntityReferenceBufferElement>,
            CeaseFireRequestedTag
        >()) 
        {
            entityCommandBuffer.SetComponentEnabled<CeaseFireRequestedTag>(squadEntity.ValueRO.SelfEntity, false);
            entityCommandBuffer.SetComponentEnabled<CeaseFireTag>(squadEntity.ValueRO.SelfEntity, true);
            // Debug.Log($"CeaseFireSystem: squad {squadEntity.ValueRO.SquadId} is now in cease fire mode");
            squadEntity.ValueRW.TargetSquadEntity = Entity.Null;
        
            for (int i = 0; i < entityBuffer.Length; i++)
            {
                Entity referencedEntity = entityBuffer[i].Entity;

                Target target = entityManager.GetComponentData<Target>(referencedEntity);
                if (entityManager.Exists(target.targetEntity))
                {
                    entityManager.SetComponentData(referencedEntity, new Target { targetEntity = Entity.Null });
                    // Debug.Log($"CeaseFireSystem: unit {referencedEntity} is dropping target {target.targetEntity} due to cease fire");
                }
            }
        }
    }
}