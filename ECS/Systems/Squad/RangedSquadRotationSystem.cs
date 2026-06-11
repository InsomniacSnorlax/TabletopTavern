using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

[BurstCompile]
public partial struct RangedSquadRotationSystem : ISystem
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

        foreach (var (squadEntity, squadMovement) in SystemAPI.Query<
            RefRO<SquadEntity>,
            RefRW<SquadMovementComponent>
            >()
                .WithPresent<FormationEngagedInRangedCombat>()
            )
        {
            if (!entityManager.Exists(squadEntity.ValueRO.TargetSquadEntity)) continue;
            
            SquadMovementComponent targetSquad = entityManager.GetComponentData<SquadMovementComponent>(squadEntity.ValueRO.TargetSquadEntity);
            Vector3 thisSquadPos = squadMovement.ValueRO.SquadCenter;
            Vector3 otherPos = targetSquad.SquadCenter;
            quaternion directionToTarget = quaternion.LookRotationSafe(otherPos - thisSquadPos, math.up());

            squadMovement.ValueRW.SetRotation(directionToTarget);
        }
    }
}