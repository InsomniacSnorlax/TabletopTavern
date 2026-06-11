using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct BraceSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (RefRO<SquadEntity> squad in SystemAPI.Query<RefRO<SquadEntity>>().WithPresent<BracedTag, AntiLargeTag>()
        .WithNone<ArtillerySquad>())
        {
            if(entityManager.IsComponentEnabled<BracedTag>(squad.ValueRO.SelfEntity))
            {
                if(entityManager.HasComponent<ChargeSquad>(squad.ValueRO.SelfEntity))
                {
                    // Debug.Log($"BraceSystem: squad {squad.ValueRO.SquadId} is charging and no longer bracing");
                    entityCommandBuffer.SetComponentEnabled<BracedTag>(squad.ValueRO.SelfEntity, false);
                }
                else if(entityManager.HasComponent<SquadMoveOverrideTag>(squad.ValueRO.SelfEntity))
                {
                    // Debug.Log($"BraceSystem: squad {squad.ValueRO.SquadId} is moving and no longer bracing");
                    entityCommandBuffer.SetComponentEnabled<BracedTag>(squad.ValueRO.SelfEntity, false);
                }
            }
            else
            {
                if(!entityManager.HasComponent<ChargeSquad>(squad.ValueRO.SelfEntity) && !entityManager.HasComponent<SquadMoveOverrideTag>(squad.ValueRO.SelfEntity))
                {
                    // Debug.Log($"BraceSystem: squad {squad.ValueRO.SquadId} is now bracing");
                    entityCommandBuffer.SetComponentEnabled<BracedTag>(squad.ValueRO.SelfEntity, true);
                }
            }
        }      
    }
}

