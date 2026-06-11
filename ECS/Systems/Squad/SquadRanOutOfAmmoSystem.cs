using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using TJ;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(RangedSquadRemoveAmmunitionSystem))]
partial struct SquadRanOutOfAmmoSystem : ISystem
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

        // Squads updating on entity destroyed
        foreach (var (squad, rangedSquad, queuedOrders, entityBuffer) in SystemAPI.Query<
            RefRO<SquadEntity>,
            RefRW<RangedSquad>,
            DynamicBuffer<QueuedOrder>,
            DynamicBuffer<EntityReferenceBufferElement>>().WithPresent<RanOutOfAmmoTag>().WithAbsent<GarrisonGateSquadTag>())
        {
            entityCommandBuffer.RemoveComponent<RanOutOfAmmoTag>(squad.ValueRO.SelfEntity);
            entityCommandBuffer.RemoveComponent<RangedSquad>(squad.ValueRO.SelfEntity);
            entityCommandBuffer.AddComponent<MeleeSquad>(squad.ValueRO.SelfEntity);

            if(entityManager.HasComponent<RangedSquadSkirmishTag>(squad.ValueRO.SelfEntity)) {
                entityCommandBuffer.RemoveComponent<RangedSquadSkirmishTag>(squad.ValueRO.SelfEntity);
            }

            if(entityManager.HasComponent<FormationEngagedInRangedCombat>(squad.ValueRO.SelfEntity)) {
                entityCommandBuffer.RemoveComponent<FormationEngagedInRangedCombat>(squad.ValueRO.SelfEntity);
            }

            queuedOrders.Clear();

            // Debug.Log($"SquadRanOutOfAmmoSystem: Squad {squad.ValueRO.SquadId} has ran out of ammo!");

            for (int i = 0; i < entityBuffer.Length; i++)
            {
                Entity entity = entityBuffer[i].Entity;

                if (entityManager.HasComponent<RangedMeleeConverter>(entity))
                {
                    RangedMeleeConverter rangedMeleeConverter = entityManager.GetComponentData<RangedMeleeConverter>(entity);
                    rangedMeleeConverter.SwitchToMelee = true;
                    entityManager.SetComponentData(entity, rangedMeleeConverter);
                    entityCommandBuffer.SetComponentEnabled<RangedMeleeConverter>(entity, true);
                }
                if(entityManager.HasComponent<ShootAttack>(entity)) {
                    entityCommandBuffer.RemoveComponent<ShootAttack>(entity);
                }
            }
        }
    }
}

