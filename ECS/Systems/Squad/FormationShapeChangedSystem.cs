using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

partial struct FormationShapeChangedSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        // When the Formation Is Changed
        foreach (var (squadEntity, squadMovementComponent, entityBuffer, FormationShapeChanged) in 
        SystemAPI.Query<
            RefRO<SquadEntity>, 
            RefRO<SquadMovementComponent>,
            DynamicBuffer<EntityReferenceBufferElement>, 
            FormationShapeChanged>())
        {
            // Debug.Log($"Processing FormationShapeChanged for squad {squadEntity.ValueRO.SquadId}");
            entityCommandBuffer.RemoveComponent<FormationShapeChanged>(squadEntity.ValueRO.SelfEntity);

            for (int i = 0; i < entityBuffer.Length; i++) {
                // Debug.Log($"SquadCombatPositioningSystem: Processing unit {i}");
                Entity referencedEntity = entityBuffer[i].Entity;
                UnitPosition unitPosition = entityManager.GetComponentData<UnitPosition>(referencedEntity);
                // Debug.Log($"SquadResetSystem: Processing unit {i} with index {unitPosition.unitIndex}");

                if(unitPosition.unitIndex >= squadMovementComponent.ValueRO.SquadWidthAndDepth.x) {
                    // Debug.Log($"SquadEntitySetUpSystem: Unit {unitPosition.unitIndex} is not in the front row, supporting {unitPosition.unitIndex - squadEntity.ValueRO.SquadWidthAndDepth.x}");
                    Entity frontEntity = entityBuffer[unitPosition.unitIndex - squadMovementComponent.ValueRO.SquadWidthAndDepth.x].Entity;
                    unitPosition.supportingEntity = frontEntity;
                    entityManager.SetComponentData(referencedEntity, unitPosition);
                } else {
                    unitPosition.supportingEntity = Entity.Null;
                    entityManager.SetComponentData(referencedEntity, unitPosition);
                }
            }
        }
    }
}