using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(ProcessUnitDeathSystem))]
partial struct KillUnitSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (KillUnitTag, entity) in SystemAPI.Query<KillUnitTag>().WithEntityAccess())
        {
            entityCommandBuffer.DestroyEntity(entity);
        }
    }
}