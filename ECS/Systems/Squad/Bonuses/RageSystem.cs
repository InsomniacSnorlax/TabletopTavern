using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using TJ;

partial struct RageSystem : ISystem
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
            >().WithPresent<RageApplicatorTag>().WithEntityAccess())
        {
            if (squadstate.ValueRO.CurrentHealthValue * 2 < squadstate.ValueRO.MaxHealthValue)
            {
                // Debug.Log($"RageSystem: Applying rage to squad {squad.ValueRO.SquadId}");
                //apply rage
                entityCommandBuffer.RemoveComponent<RageApplicatorTag>(entity);
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
            }
        }
    }
}

