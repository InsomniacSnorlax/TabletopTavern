using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using TJ;
using TJ.Morale;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(KillUnitSystem))]
partial struct SquadRemoveUnitSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // state.RequireForUpdate<BattlePhase>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        // Remove dead and dying unit entities from ALL squad buffers before EndSimulationECB destroys them.
        // KillUnitSystem queues DestroyEntity via EndSimulationECB (not yet played back), so Exists() still
        // returns true for to-be-killed units. We check KillUnitTag directly to catch them this frame,
        // preventing SquadDisengageJob from accessing stale destroyed entities next frame.
        foreach (var (_, entityBuffer) in SystemAPI.Query<RefRO<SquadEntity>, DynamicBuffer<EntityReferenceBufferElement>>())
        {
            for (int i = entityBuffer.Length - 1; i >= 0; i--)
            {
                Entity unitEntity = entityBuffer[i].Entity;
                if (!entityManager.Exists(unitEntity) || entityManager.HasComponent<KillUnitTag>(unitEntity))
                    entityBuffer.RemoveAt(i);
            }
        }

        // Squads updating on entity destroyed
        foreach (var (squad, squadMovementComponent, entityBuffer, FormationNeedsToBeProcessed) in SystemAPI.Query<
            RefRW<SquadEntity>, 
            RefRO<SquadMovementComponent>,
            DynamicBuffer<EntityReferenceBufferElement>, 
            FormationNeedsToBeProcessed>())
        {
            
            int2 squadUnitCount = new (entityBuffer.Length, squad.ValueRO.initialSquadSize);
            Entity squadDestroyedEntity = entityCommandBuffer.CreateEntity();

            entityCommandBuffer.AddComponent(squadDestroyedEntity,
                new UpdatedSquadUnitCount
                {
                    SquadId = squad.ValueRO.SquadId,
                    UnitCount = squadUnitCount
                });
            entityCommandBuffer.AddComponent(squadDestroyedEntity, new DestroyEntityTag { });
            entityCommandBuffer.SetComponentEnabled<DestroyEntityTag>(squadDestroyedEntity, false);

            //get unit depth and width
            int formationWidth = squadMovementComponent.ValueRO.SquadWidthAndDepth.x;
            
            //skip this the first time the squad is processed
            // Iterate in reverse so we can safely remove stale entries without index shifting
            for (int i = entityBuffer.Length - 1; i >= 0; i--)
            {
                Entity referencedEntity = entityBuffer[i].Entity;
                if (!entityManager.Exists(referencedEntity))
                {
                    Debug.LogError($"SquadRemoveUnitSystem: Stale entity reference found in squad {squad.ValueRO.SquadId} buffer at index {i}, removing.");
                    entityBuffer.RemoveAt(i);
                    continue;
                }
                UnitPosition unitPosition = entityManager.GetComponentData<UnitPosition>(referencedEntity);
                SetDestination setDestination = entityManager.GetComponentData<SetDestination>(referencedEntity);

                if(unitPosition.unitIndex >= formationWidth) {
                    if(!entityManager.Exists(unitPosition.supportingEntity)) {
                        // Debug.Log($"SquadCombatPositioningSystem: Unit {unitPosition.unitIndex} is missing its supporting entity, setting its new index to {unitPosition.unitIndex - formationWidth}");
                        // Debug.Log($"Unit {unitPosition.unitIndex}, setting new index {unitPosition.unitIndex - formationWidth} from {FormationNeedsToBeProcessed.indexRemoved}");
                        unitPosition.unitIndex = FormationNeedsToBeProcessed.indexRemoved;
                        setDestination.squadPosition = FormationNeedsToBeProcessed.squadPosition;
                        entityManager.SetComponentData(referencedEntity, unitPosition);
                        entityManager.SetComponentData(referencedEntity, setDestination);
                        //update squad position to be
                    }
                }
            }
            //reorder the indexes to remove any gaps
            for (int i = 0; i < entityBuffer.Length; i++)
            {
                Entity referencedEntity = entityBuffer[i].Entity;
                if (!entityManager.Exists(referencedEntity)) continue;
                UnitPosition unitPosition = entityManager.GetComponentData<UnitPosition>(referencedEntity);
                unitPosition.unitIndex = i;
                entityManager.SetComponentData(referencedEntity, unitPosition);
            }

            entityCommandBuffer.RemoveComponent<FormationNeedsToBeProcessed>(squad.ValueRO.SelfEntity);
            entityCommandBuffer.AddComponent<FormationShapeChanged>(squad.ValueRO.SelfEntity);
        }
}
}

