using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using TJ;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct SquadSwitchToMeleeModeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // state.RequireForUpdate<BattlePhase>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        // Squads updating on entity destroyed
        foreach (var (squad, SwitchToMeleeTag, entityBuffer) in SystemAPI.Query<
            RefRO<SquadEntity>,
            SwitchToMeleeTag,
            DynamicBuffer<EntityReferenceBufferElement>>().WithPresent<RangedSquad>())
        {
            if(SwitchToMeleeTag.SwitchType == RangedToMeleeSwitchType.Melee)
            {
                Debug.Log($"SquadSwitchToMeleeModeSystem: Squad {squad.ValueRO.SquadId} is switching to Melee mode!");
                if (!entityManager.HasComponent<MeleeSquad>(squad.ValueRO.SelfEntity))
                {
                    entityCommandBuffer.AddComponent<MeleeSquad>(squad.ValueRO.SelfEntity);
                }
                entityCommandBuffer.SetComponentEnabled<RangedSquad>(squad.ValueRO.SelfEntity, false);
            }
            else if(SwitchToMeleeTag.SwitchType == RangedToMeleeSwitchType.Ranged)
            {
                // Debug.Log($"SquadSwitchToMeleeModeSystem: switching squad {squad.ValueRO.SquadId} to RANGED mode");
                if (entityManager.HasComponent<MeleeSquad>(squad.ValueRO.SelfEntity))
                {
                    entityCommandBuffer.RemoveComponent<MeleeSquad>(squad.ValueRO.SelfEntity);
                }
                entityCommandBuffer.SetComponentEnabled<RangedSquad>(squad.ValueRO.SelfEntity, true);
            }

            for (int i = 0; i < entityBuffer.Length; i++)
            {
                Entity entity = entityBuffer[i].Entity;

                if (entityManager.HasComponent<RangedMeleeConverter>(entity))
                {
                    RangedMeleeConverter rangedMeleeConverter = entityManager.GetComponentData<RangedMeleeConverter>(entity);
                    rangedMeleeConverter.SwitchToMelee = SwitchToMeleeTag.SwitchType == RangedToMeleeSwitchType.Ranged ? false : true;
                    entityManager.SetComponentData(entity, rangedMeleeConverter);
                    entityCommandBuffer.SetComponentEnabled<RangedMeleeConverter>(entity, true);
                }
            }
            
            entityCommandBuffer.RemoveComponent<SwitchToMeleeTag>(squad.ValueRO.SelfEntity);
        }
    }
}

