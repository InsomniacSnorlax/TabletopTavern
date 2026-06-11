using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

// [UpdateInGroup(typeof(LateSimulationSystemGroup))]
// [UpdateAfter(typeof(DestroySquadSystem))]
partial struct DestroyEntitySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        //destroy squads with no entities
        foreach (var (DestroyEntityTag, entity) in SystemAPI.Query<
            DestroyEntityTag
        >().WithEntityAccess()) {
            entityCommandBuffer.DestroyEntity(entity);
        }
    }
}