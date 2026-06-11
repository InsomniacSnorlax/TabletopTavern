using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using ProjectDawn.Navigation;
using System.Collections.Generic;

partial struct UnitCatchUpSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        // foreach (var (Unit, AgentBody, AgentLocomotion, Entity) in SystemAPI.Query<RefRO<Unit>, RefRW<AgentBody>, RefRW<AgentLocomotion>>().WithEntityAccess())
        // {
        //     if (!entityManager.IsComponentEnabled<CatchUpTag>(Entity) && !entityManager.HasComponent<InCombat>(Entity)) {
        //         if(AgentBody.ValueRO.RemainingDistance > 9) {
        //             // Debug.Log($"Unit {Unit.ValueRO.unitName} needs to catch up");
        //             SquadStats squadStats = TabletopTavernData.Instance.GetSquadStats(Unit.ValueRO.unitName);
        //             AgentLocomotion.ValueRW.Speed = squadStats.Speed * 2;
        //             entityCommandBuffer.SetComponentEnabled<CatchUpTag>(Entity, true);
        //         }
        //     }
        // }

        // foreach (var (Unit, AgentBody, CatchUpTag, AgentLocomotion,  Entity) in SystemAPI.Query<RefRO<Unit>, RefRW<AgentBody>, RefRW<CatchUpTag>, RefRW<AgentLocomotion>>().WithEntityAccess())
        // {
        //     if(AgentBody.ValueRO.RemainingDistance < 5) {
        //         // Debug.Log($"Unit {Unit.ValueRO.unitName} is done catching up");
        //         SquadStats squadStats = TabletopTavernData.Instance.GetSquadStats(Unit.ValueRO.unitName);
        //         AgentLocomotion.ValueRW.Speed = squadStats.Speed;
        //         entityCommandBuffer.SetComponentEnabled<CatchUpTag>(Entity, false);
        //     }
        // }
        
    }
}