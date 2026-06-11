using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using TJ;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct RangedSquadRemoveAmmunitionSystem : ISystem
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
        foreach (var (squad, rangedSquad, entityBuffer) in SystemAPI.Query<
            RefRO<SquadEntity>, 
            RefRW<RangedSquad>,
            DynamicBuffer<EntityReferenceBufferElement>>())
        {
            //skip this the first time the squad is processed
            for (int i = 0; i < entityBuffer.Length; i++)
            {
                Entity referencedEntity = entityBuffer[i].Entity;
                if(entityManager.HasComponent<AmmuntionSpent>(referencedEntity)) 
                {
                    entityCommandBuffer.RemoveComponent<AmmuntionSpent>(referencedEntity);
                    rangedSquad.ValueRW.Ammunition -= 1;
                }
            }
            //update healthbar ammo count
            if (rangedSquad.ValueRW.Ammunition < 0)
            {
                entityCommandBuffer.AddComponent<RanOutOfAmmoTag>(squad.ValueRO.SelfEntity);
            }
        }
    }
}

