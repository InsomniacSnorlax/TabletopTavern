using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

partial struct RangedUnitTargetingSystem : ISystem {

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {

        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var ( target, unit, rangedUnit, needsToBeProcessed, entity) 
            in SystemAPI.Query< RefRW<Target>, RefRO<Unit>, RangedUnitTag, RefRW<NeedsToBeProcessed>>().WithEntityAccess()) {

            if(needsToBeProcessed.ValueRO.Delay > 0f) {
                needsToBeProcessed.ValueRW.Delay -= SystemAPI.Time.DeltaTime;
                continue;
            }
            
            entityCommandBuffer.RemoveComponent<NeedsToBeProcessed>(entity);
            // Debug.Log($"RangedTargetingSystem: Processing unit {unit.ValueRO.squadEntity}");
            //if target is already set, skip
            if(target.ValueRO.targetEntity != Entity.Null) continue; 

            //get the targeted enemy squad
            SquadEntity squadEntity = SystemAPI.GetComponent<SquadEntity>(unit.ValueRO.squadEntity);
            if(!entityManager.Exists(squadEntity.TargetSquadEntity)) continue;

            SquadEntity targetSquadEntity = SystemAPI.GetComponent<SquadEntity>(squadEntity.TargetSquadEntity);
            foreach (var (squad, entityBuffer) in SystemAPI.Query<RefRW<SquadEntity>, DynamicBuffer<EntityReferenceBufferElement>>())
            {
                if(squad.ValueRO.SquadId != targetSquadEntity.SquadId) continue;

                if(entityBuffer.Length != 0) 
                {
                    float randomInt = UnityEngine.Random.Range(0, entityBuffer.Length);
                    Entity targetEntity = entityBuffer[(int)randomInt].Entity;
                    target.ValueRW.targetEntity = targetEntity;

                    // quaternion directionToEntity = quaternion.LookRotation(math.normalize(entityManager.GetComponentData<LocalToWorld>(targetEntity).Position - entityManager.GetComponentData<LocalToWorld>(entity).Position), math.up());
                    // entityManager.SetComponentData(entity, new RotateUnit { targetRotation = directionToEntity });
                    // entityManager.SetComponentEnabled<RotateUnit>(entity, true);
                    continue;
                }
            }
        }
    }
}