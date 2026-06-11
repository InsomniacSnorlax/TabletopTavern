using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct SquadTakingFlankingDamageSystem : ISystem
{
    const float TAKING_FLANKING_DAMAGE_DURATION = 3f;

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

        foreach (var (takingFlankingDamage, entity) in SystemAPI.Query<RefRW<TakingFlankingDamage>>()
            .WithPresent<TakingFlankingDamage>()
            .WithAbsent<BrokenSquadTag>()
            .WithEntityAccess())
        {
            if (takingFlankingDamage.ValueRO.RecentlyTookDamage)
            {
                takingFlankingDamage.ValueRW.RecentlyTookDamage = false;
                takingFlankingDamage.ValueRW.LifeTime = TAKING_FLANKING_DAMAGE_DURATION;
                if (!SystemAPI.IsComponentEnabled<TakingFlankingDamage>(entity))
                {
                    // Debug.Log($"SquadTakingFlankingDamageSystem: Entity {entity.Index} is now taking flanking damage.");
                    entityCommandBuffer.SetComponentEnabled<TakingFlankingDamage>(entity, true);
                }
            }
            
            // if disabled continue
            if (!SystemAPI.IsComponentEnabled<TakingFlankingDamage>(entity)) continue;
            
            takingFlankingDamage.ValueRW.LifeTime -= deltaTime;

            if (takingFlankingDamage.ValueRO.LifeTime <= 0f)
            {
                // Debug.Log($"SquadTakingFlankingDamageSystem: Entity {entity.Index} has stopped taking flanking damage.");
                entityCommandBuffer.SetComponentEnabled<TakingFlankingDamage>(entity, false);
            }
        }
    }
}