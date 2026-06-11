using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using TJ;

partial struct SlayerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
        state.RequireForUpdate<SquadStatsData>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var statsData = SystemAPI.GetSingleton<SquadStatsData>();
        ref var statsBlob = ref statsData.StatsBlob.Value; 
        
        // Squads updating on entity destroyed
        foreach (var (squad, squadstate, battlefieldBonusBufferElement, entity) in SystemAPI.Query<
            RefRO<SquadEntity>,
            RefRO<SquadStateComponent>,
            DynamicBuffer<BattlefieldBonusBufferElement>
            >().WithPresent<SlayerApplicatorTag>().WithEntityAccess())
        {

            //get id of squad
            int squadId = squad.ValueRO.SquadId;
            bool isPlayerSquad = squadId >= 0;
            entityCommandBuffer.RemoveComponent<SlayerApplicatorTag>(entity);

            foreach (RefRO<SquadEntity> enemySquad in SystemAPI.Query<RefRO<SquadEntity>>().WithNone<BrokenSquadTag>())
            {
                //only target playersquads that are not the same squad
                if(isPlayerSquad && enemySquad.ValueRO.SquadId > 0) continue;
                if(!isPlayerSquad && enemySquad.ValueRO.SquadId < 0) continue;
                
                if(!entityManager.HasComponent<MonsterousSquadTag>(enemySquad.ValueRO.SelfEntity)) continue;

                entityCommandBuffer.AddComponent<RageActiveTag>(entity);
                SquadStats squadStats = statsBlob.GetStats(squad.ValueRO.UnitName);
                int bonus = squadStats.WeaponStrength;

                battlefieldBonusBufferElement.Add(new BattlefieldBonusBufferElement
                {
                    Value = new BattlefieldBonus
                    {
                        UnitStat = UnitStat.WeaponStrength,
                        BattlefieldBonusEnum = BattlefieldBonusEnum.Rage,
                        Team = Team.Neutral,
                        Value = bonus,
                        Range = Mathf.Infinity,
                    }
                });
                break;
            }
        }
    }
}

