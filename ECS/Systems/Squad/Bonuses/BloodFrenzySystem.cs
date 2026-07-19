using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using TJ;

partial struct BloodFrenzySystem : ISystem
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
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var statsData = SystemAPI.GetSingleton<SquadStatsData>();
        ref var statsBlob = ref statsData.StatsBlob.Value; 
        
        foreach (var (squad, squadstate, battlefieldBonusBufferElement, entity) in SystemAPI.Query<
            RefRO<SquadEntity>,
            RefRO<SquadStateComponent>,
            DynamicBuffer<BattlefieldBonusBufferElement>
            >().WithPresent<BloodFrenzyApplicatorTag>().WithEntityAccess())
        {
            if (squadstate.ValueRO.CurrentHealthValue > squadstate.ValueRO.MaxHealthValue * 0.75f)
            {
                // Debug.Log($"BloodFrenzySystem: Applying blood frenzy to squad {squad.ValueRO.SquadId}");
                //apply blood frenzy
                entityCommandBuffer.RemoveComponent<BloodFrenzyApplicatorTag>(entity);
                entityCommandBuffer.AddComponent<BloodFrenzyActiveTag>(entity);

                SquadStats squadStats = statsBlob.GetStats(squad.ValueRO.UnitName);
                int bonus = (int)(squadStats.WeaponStrength * 1.5f);
                float speedBonus = squadStats.Speed * 0.2f / 10f;

                battlefieldBonusBufferElement.Add(new BattlefieldBonusBufferElement
                {
                    Value = new BattlefieldBonus
                    {
                        UnitStat = UnitStat.WeaponStrength,
                        BattlefieldBonusEnum = BattlefieldBonusEnum.BloodFrenzy,
                        Team = Team.Neutral,
                        Value = bonus,
                        Range = Mathf.Infinity,
                    }
                });

                battlefieldBonusBufferElement.Add(new BattlefieldBonusBufferElement
                {
                    Value = new BattlefieldBonus
                    {
                        UnitStat = UnitStat.Speed,
                        BattlefieldBonusEnum = BattlefieldBonusEnum.BloodFrenzy,
                        Team = Team.Neutral,
                        Value = speedBonus,
                        Range = Mathf.Infinity,
                    }
                });
            }
        }

        //remove blood frenzy when health goes to or below 75% of max health
        foreach (var (squad, squadstate, battlefieldBonusBufferElement, entity) in SystemAPI.Query<
            RefRO<SquadEntity>,
            RefRO<SquadStateComponent>,
            DynamicBuffer<BattlefieldBonusBufferElement>
            >().WithPresent<BloodFrenzyActiveTag>().WithEntityAccess())
        {
            if (squadstate.ValueRO.CurrentHealthValue <= squadstate.ValueRO.MaxHealthValue * 0.75f)
            {
                // Debug.Log($"BloodFrenzySystem: Removing blood frenzy from squad {squad.ValueRO.SquadId}");
                //remove blood frenzy
                entityCommandBuffer.AddComponent<RemoveBloodFrenzyTag>(entity);
                entityCommandBuffer.RemoveComponent<BloodFrenzyActiveTag>(entity);
            }
        }
    }
}

