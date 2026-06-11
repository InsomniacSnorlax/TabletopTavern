using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

partial struct RangedSquadChargeSystem : ISystem
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

        // Ranged squad moving into range
        foreach (var (squad, SquadTargettingComponent, SquadMovementComponent, RangedSquad, SquadOverridesComponent, entityBuffer, ChargeSquad) in SystemAPI.Query<
            RefRW<SquadEntity>,
            RefRW<SquadTargettingComponent>,
            RefRW<SquadMovementComponent>,
            RefRW<RangedSquad>,
            SquadOverridesComponent,
            DynamicBuffer<EntityReferenceBufferElement>,
            ChargeSquad>
        ().WithAbsent<InCombat, BrokenSquadTag>()
        .WithNone<CeaseFireTag>()
        )
        {
            // Debug.Log($"RangedSquadChargeSystem: ");

            if (!SquadOverridesComponent.AutoTarget && !entityManager.Exists(squad.ValueRO.TargetSquadEntity)) continue;
            
            entityCommandBuffer.SetComponentEnabled<SquadTargettingComponent>(squad.ValueRO.SelfEntity, false);

            if(entityManager.HasComponent<SquadMoveOverrideTag>(squad.ValueRO.SelfEntity)) continue;

            if(!entityManager.Exists(squad.ValueRO.TargetSquadEntity))
            {
                // Debug.Log($"bug?");
                Debug.Log($"RangedSquadChargeSystem: Squad {squad.ValueRO.SquadId} has no target but is trying to charge");
                entityCommandBuffer.AddComponent<HaltCommandTag>(squad.ValueRO.SelfEntity);
                continue;
            } 
            
            if(entityManager.HasComponent<BrokenSquadTag>(squad.ValueRO.TargetSquadEntity)) 
            {
                // Debug.Log($"RangedSquadChargeSystem: Squad {squad.ValueRO.SquadId} is targeting a broken squad {squad.ValueRO.TargetSquadEntity}");
                entityCommandBuffer.AddComponent<HaltCommandTag>(squad.ValueRO.SelfEntity);
                // entityCommandBuffer.AddComponent<HaltCommandTag>(squad.ValueRO.SelfEntity);
                continue;
            }

            SquadMovementComponent targetSquad = entityManager.GetComponentData<SquadMovementComponent>(squad.ValueRO.TargetSquadEntity);
            float distance = math.distance(targetSquad.SquadCenter, SquadMovementComponent.ValueRO.SquadCenter);
            quaternion directionToTarget = quaternion.LookRotationSafe(targetSquad.SquadCenter - SquadMovementComponent.ValueRO.SquadCenter, math.up());

            SquadMovementComponent.ValueRW.SetRotation(directionToTarget);
            //get forward vector of navmesh agent
            // float3 forward = math.forward(SquadMovementComponent.ValueRO.SquadRotation);

            if (distance < RangedSquad.ValueRO.AttackRange)
            {
                // Debug.Log($"unit in range to shoot");

                if (!entityManager.HasComponent<HaltCommandTag>(squad.ValueRO.SelfEntity))
                {
                    entityCommandBuffer.AddComponent(squad.ValueRO.SelfEntity, new HaltCommandTag() { FreezePosition = true });
                }

                entityCommandBuffer.AddComponent<FormationEngagedInRangedCombat>(squad.ValueRO.SelfEntity);

                for (int i = 0; i < entityBuffer.Length; i++)
                {
                    Entity entity = entityBuffer[i].Entity;
                    if (!entityManager.Exists(entity)) continue;

                    Unit unit = entityManager.GetComponentData<Unit>(entity);
                    unit.unitState = UnitState.OnEngageRanged;
                    entityManager.SetComponentData(entity, unit);
                }
            }
            else
            {
                SquadMovementComponent.ValueRW.GoalPosition = targetSquad.SquadCenter;
                entityCommandBuffer.AddComponent<RecalculatePositionsForUnitsCharging>(squad.ValueRO.SelfEntity);
            } 
        }
    }
}