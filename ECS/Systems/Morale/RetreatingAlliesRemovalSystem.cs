using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

namespace TJ.Morale
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(MoraleSystem))]
    public partial struct RetreatingAlliesRemovalSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BattlePhase>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityManager entityManager = state.EntityManager;
            EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            //Apply charge bonus to squad
            foreach (var (squad, squadWithRetreatingAllies) 
                in SystemAPI.Query<
                    RefRO<SquadEntity>,
                    RefRW<RetreatingNearbyAllies>
                >().WithNone<BrokenSquadTag>())
            {
                squadWithRetreatingAllies.ValueRW.AlertTimer -= SystemAPI.Time.DeltaTime;

                if(squadWithRetreatingAllies.ValueRO.AlertTimer <= 0)
                {
                    squadWithRetreatingAllies.ValueRW.AlertTimer = 0;
                    entityManager.SetComponentEnabled<RetreatingNearbyAllies>(squad.ValueRO.SelfEntity, false);
                    // Debug.Log($"Squad {squad.ValueRO.SquadId} is no longer affected by nearby retreating allies.");
                }
            }
        }
    }
}