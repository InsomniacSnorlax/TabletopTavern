using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

partial struct SquadFlankingSystem : ISystem
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

        //Get squads that are flanking
        foreach (var (squadEntity, isFlanking, entityBuffer, Entity) in SystemAPI.Query<
            RefRW<SquadEntity>, RefRW<IsFlanking>, 
            DynamicBuffer<EntityReferenceBufferElement>>()
        .WithEntityAccess())
        {
            for (int i = 0; i < entityBuffer.Length; i++) {
                Entity referencedEntity = entityBuffer[i].Entity;
                if (entityManager.HasComponent<DealFlankingDamageTag>(referencedEntity)) {
                    if (!entityManager.IsComponentEnabled<DealFlankingDamageTag>(referencedEntity))
                    {
                        entityManager.SetComponentEnabled<DealFlankingDamageTag>(referencedEntity, true);
                    }
                }
            }
            
            // Debug.Log($"Squad {squadEntity.ValueRO.SquadId} is flanking its target!");
            Entity targetSquadEntity = squadEntity.ValueRO.TargetSquadEntity;
            if (targetSquadEntity == Entity.Null || !entityManager.Exists(targetSquadEntity) || isFlanking.ValueRO.TargetFlankedSquadEntity != targetSquadEntity)
            {
                isFlanking.ValueRW.TargetFlankedSquadEntity = Entity.Null;
                entityManager.SetComponentEnabled<IsFlanking>(squadEntity.ValueRO.SelfEntity, false);
                // Debug.Log($"Squad {squadEntity.ValueRO.SquadId} is no longer flanking its target!");

                for (int i = 0; i < entityBuffer.Length; i++) {
                    Entity referencedEntity = entityBuffer[i].Entity;
                    if (entityManager.HasComponent<DealFlankingDamageTag>(referencedEntity)) {
                        entityManager.SetComponentEnabled<DealFlankingDamageTag>(referencedEntity, false);
                    }
                }
            }
        }
    }
}