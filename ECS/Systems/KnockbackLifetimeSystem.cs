using Unity.Burst;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// This is runs when knockback units in applied on a charge or shot but not turned on until it is in combat
/// </summary>
[BurstCompile]
public partial struct KnockbackLifetimeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ApplyKnockbackOnContact>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (applyKnockbackOnContact, entity) in SystemAPI.Query<
            RefRW<ApplyKnockbackOnContact>>()
                     .WithEntityAccess())
        {
            applyKnockbackOnContact.ValueRW.LifeTime -= deltaTime;

            if (applyKnockbackOnContact.ValueRO.LifeTime <= 0f)
            {
                // Debug.Log($"KnockbackLifetimeSystem: {entity} knockback expired");
                entityCommandBuffer.SetComponentEnabled<ApplyKnockbackOnContact>(entity, false);
            }
        }
    }
}