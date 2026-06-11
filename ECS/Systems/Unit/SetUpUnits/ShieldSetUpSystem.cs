using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace TJ
{
public partial struct ShieldSetUpSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ShieldSetUpEntity>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
        foreach (var (shield, ShieldEntity) in SystemAPI
            .Query<ShieldSetUpEntity>()
            .WithEntityAccess())
        {
            // Get the parent of the spawned entity
            if (SystemAPI.HasComponent<Parent>(ShieldEntity))
            {
                var parentComponent = SystemAPI.GetComponent<Parent>(ShieldEntity);
                Entity parentEntity = parentComponent.Value;

                if (SystemAPI.HasComponent<Parent>(parentEntity))
                {
                    var grandparentComponent = SystemAPI.GetComponent<Parent>(parentEntity);
                    Entity grandparentEntity = grandparentComponent.Value;

                    if (SystemAPI.HasComponent<Shield>(grandparentEntity))
                    {
                        Shield shieldComp = SystemAPI.GetComponent<Shield>(grandparentEntity);
                        shieldComp.shieldEntity = ShieldEntity;

                        ecb.SetComponent(grandparentEntity, shieldComp);
                        ecb.RemoveComponent<ShieldSetUpEntity>(ShieldEntity);
                    }
                }
            }
        }
        
        ecb.Playback(state.EntityManager);
    }
}
}
