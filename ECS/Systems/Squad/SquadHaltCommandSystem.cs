using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using ProjectDawn.Navigation;
using System.Collections.Generic;

partial struct SquadHaltCommandSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (squadEntity, squadMovementComponent, haltCommandTag, entityBuffer) in SystemAPI.Query<
            RefRW<SquadEntity>,
            RefRW<SquadMovementComponent>,
            RefRO<HaltCommandTag>, 
            DynamicBuffer<EntityReferenceBufferElement>
        >()) {
            entityCommandBuffer.RemoveComponent<HaltCommandTag>(squadEntity.ValueRO.SelfEntity);
            entityCommandBuffer.SetComponentEnabled<JustFollowingOrders>(squadEntity.ValueRO.SelfEntity, false);
            // Debug.Log($"SquadHaltCommandSystem: squad is no longer following orders, halting");

            if (SystemAPI.GetComponentLookup<ChargeSquad>(true).HasComponent(squadEntity.ValueRW.SelfEntity)) {
                entityCommandBuffer.RemoveComponent<ChargeSquad>(squadEntity.ValueRW.SelfEntity);
                // Debug.Log($"SquadHaltCommandSystem: squad {squadEntity.ValueRO.SquadId} is halting charge");
            }

            if(haltCommandTag.ValueRO.DropTarget) 
            {
                squadEntity.ValueRW.TargetSquadEntity = Entity.Null;
                entityCommandBuffer.SetComponentEnabled<DisengageFromCombat>(squadEntity.ValueRO.SelfEntity, true);
            }

            squadMovementComponent.ValueRW.GoalPosition = squadMovementComponent.ValueRW.SquadCenter;
            // Debug.Log($"SquadHaltCommandSystem: squad {squadEntity.ValueRO.SquadId} is halting movement");

            for (int i = 0; i < entityBuffer.Length; i++)
            {
                Entity referencedEntity = entityBuffer[i].Entity;

                if(haltCommandTag.ValueRO.FreezePosition)
                {
                    LocalTransform localTransform = entityManager.GetComponentData<LocalTransform>(referencedEntity);
                    SetDestination setDestination = entityManager.GetComponentData<SetDestination>(referencedEntity);

                    //check for nans
                    if (localTransform.Position.x is float.NaN || localTransform.Position.y is float.NaN || localTransform.Position.z is float.NaN)
                    {
                        localTransform.Position = float3.zero;
                        Debug.LogError($"LocalTransform Position contained NaN values for entity {referencedEntity}!!!!");
                    }
                    setDestination.squadPosition = localTransform.Position;
                    entityManager.SetComponentData(referencedEntity, setDestination);

                    // AgentSonarAvoid agentSonarAvoid = entityManager.GetComponentData<AgentSonarAvoid>(referencedEntity);
                    // agentSonarAvoid.BlockedStop = false;
                    // agentSonarAvoid.MaxAngle = math.radians(360);
                    // entityManager.SetComponentData(referencedEntity, agentSonarAvoid);
                    // entityCommandBuffer.SetComponentEnabled<AgentSonarAvoid>(referencedEntity, false);
                }
            }
            
            entityCommandBuffer.AddComponent<FormationShapeChanged>(squadEntity.ValueRO.SelfEntity);
        }
    }
}