using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct SquadTakingFireDamageSystem : ISystem
{
    const float TAKING_FIRE_DAMAGE_DURATION = 5f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (TakingFireDamage, entity) in SystemAPI.Query<RefRW<TakingFireDamage>>()
            .WithPresent<TakingFireDamage>()
            .WithAbsent<BrokenSquadTag>()
            .WithEntityAccess())
        {
            if (TakingFireDamage.ValueRO.RecentlyTookDamage)
            {
                TakingFireDamage.ValueRW.RecentlyTookDamage = false;
                TakingFireDamage.ValueRW.LifeTime = TAKING_FIRE_DAMAGE_DURATION;
                if (!SystemAPI.IsComponentEnabled<TakingFireDamage>(entity))
                {
                    // Debug.Log($"SquadTakingFireDamageSystem: Entity {entity.Index} is now taking fire damage.");
                    entityCommandBuffer.SetComponentEnabled<TakingFireDamage>(entity, true);
                }
            }
            
            // if disabled continue
            if (!SystemAPI.IsComponentEnabled<TakingFireDamage>(entity)) continue;
            
            TakingFireDamage.ValueRW.LifeTime -= deltaTime;

            if (TakingFireDamage.ValueRO.LifeTime <= 0f)
            {
                // Debug.Log($"SquadTakingFireDamageSystem: Entity {entity.Index} has stopped taking fire damage.");
                entityCommandBuffer.SetComponentEnabled<TakingFireDamage>(entity, false);
            }
        }
    }
}