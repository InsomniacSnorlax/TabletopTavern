using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct StartChargeSystem : ISystem
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

        foreach (var (squad, entityBuffer) in SystemAPI.Query<
            RefRW<SquadEntity>, 
            DynamicBuffer<EntityReferenceBufferElement>
            >().WithPresent<StartChargeTag>())
        {
            entityCommandBuffer.RemoveComponent<StartChargeTag>(squad.ValueRO.SelfEntity);
            entityCommandBuffer.AddComponent<ChargeSquad>(squad.ValueRO.SelfEntity);
            // Debug.Log($"StartChargeSystem: squad {squad.ValueRO.SquadId} is starting charge");

            for (int i = 0; i < entityBuffer.Length; i++)
            {
                Entity unitEntity = entityBuffer[i].Entity;
                Unit unit = entityManager.GetComponentData<Unit>(unitEntity);
                // Debug.Log($"StartChargeSystem: Setting unit {unit.unitState} state to OnCharge");
                if(unit.unitState == UnitState.OnEngage)
                {
                    // Debug.LogWarning($"StartChargeSystem: Unit {unitEntity} is already OnEngage, cannot set to OnCharge");
                    continue;
                }
                unit.unitState = UnitState.OnCharge;
                entityManager.SetComponentData(unitEntity, unit);
            }
        }      
    }
}

