using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

partial struct EnemyArmyTargetOverrideSystem : ISystem
{
    private double lastExecutionTime;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
        lastExecutionTime = 0;
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //only fire twice per second
        double currentTime = SystemAPI.Time.ElapsedTime;
        if (currentTime - lastExecutionTime < 0.5f) return;

        lastExecutionTime = currentTime;

        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        
        foreach (var (squad, squadMovementComponent, enemySquad) in SystemAPI.Query<
            RefRW<SquadEntity>, 
            RefRO<SquadMovementComponent>, 
            EnemySquad
        >().WithNone<
            InCombat, 
            WithdrawSquadTag>()
        .WithNone<
            CavalryFlankingTag>()
        ){

            bool dazedFromOpponentRunningAway = entityManager.HasComponent<OpponentRanAwayTag>(squad.ValueRO.SelfEntity);
            if(dazedFromOpponentRunningAway) {
                OpponentRanAwayTag opponentRanAwayTag = entityManager.GetComponentData<OpponentRanAwayTag>(squad.ValueRO.SelfEntity);
                opponentRanAwayTag.DazedTime -= Time.deltaTime;
                entityManager.SetComponentData(squad.ValueRO.SelfEntity, opponentRanAwayTag);

                if(opponentRanAwayTag.DazedTime <= 0) {
                    entityCommandBuffer.RemoveComponent<OpponentRanAwayTag>(squad.ValueRO.SelfEntity);
                }
                continue;
            }

            Entity closestEnemySquadEntity = squad.ValueRO.TargetSquadEntity;
            if(closestEnemySquadEntity == Entity.Null) continue; //if not yet targeting an enemy squad, ignore this system

            float currentDistanceToTarget = math.distance(
                squadMovementComponent.ValueRO.SquadCenter, 
                entityManager.GetComponentData<SquadMovementComponent>(closestEnemySquadEntity).SquadCenter);

            // Debug.Log($"SquadTargettingSystem: squad {squad.ValueRO.SquadId} is looking for targeting override, current distance to target {currentDistanceToTarget}");

            foreach (RefRO<SquadEntity> playerSquad in SystemAPI.Query<RefRO<SquadEntity>>().WithNone<BrokenSquadTag>())
            {
                //only target player squads
                if(playerSquad.ValueRO.SquadId < 0) continue;

                //ignore current target
                if(playerSquad.ValueRO.SelfEntity == squad.ValueRO.TargetSquadEntity) continue;

                float3 centerOfSquad = squadMovementComponent.ValueRO.SquadCenter;
                float3 centerOfTargetSquad = entityManager.GetComponentData<SquadMovementComponent>(playerSquad.ValueRO.SelfEntity).SquadCenter;
                float distance = math.distance(centerOfSquad, centerOfTargetSquad);

                // Debug.Log($"SquadTargettingSystem: squad {squad.ValueRO.SquadId} is checking targeting override, distance to squad {playerSquad.ValueRO.SquadId} is {distance}");

                if(distance < TabletopTavernConstants.OVERIDE_TARGET_SQUADENTITY_DISTANCE && distance < currentDistanceToTarget) {
                    currentDistanceToTarget = distance;
                    closestEnemySquadEntity = playerSquad.ValueRO.SelfEntity;
                    // Debug.Log($"SquadTargettingSystem: squad {squad.ValueRO.SquadId} is overriding targeting {closestEnemySquadEntity.Index}");
                }
            }
            
            //if no closest enemy squad found, ignore this system
            if(closestEnemySquadEntity == squad.ValueRO.TargetSquadEntity) continue;

            squad.ValueRW.TargetSquadEntity = closestEnemySquadEntity;
            // float3 centerOfTarget = entityManager.GetComponentData<SquadMovementComponent>(closestEnemySquadEntity).SquadCenter;
            // float3 squadcenter = squadMovementComponent.ValueRO.SquadCenter;

            // float3 directionToTarget = math.normalize(centerOfTarget - squadcenter);
            // quaternion newRotation = quaternion.LookRotationSafe(directionToTarget, math.up());
            // entityCommandBuffer.AddComponent(squad.ValueRO.SelfEntity, new DirectionToRotateTo { Value = newRotation });
            entityCommandBuffer.AddComponent<StartChargeTag>(squad.ValueRO.SelfEntity);

            // Debug.Log($"SquadTargettingSystem: squad {squad.ValueRO.SquadId} is targeting {closestEnemySquadEntity.Index}");
        }
    }
}

