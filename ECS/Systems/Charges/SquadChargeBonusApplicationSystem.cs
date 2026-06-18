using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using TJ;

partial struct SquadChargeBonusApplicationSystem : ISystem
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
        CampaignSaveDataHolder campaignSaveDataHolder = SystemAPI.GetSingleton<CampaignSaveDataHolder>();
        var statsData = SystemAPI.GetSingleton<SquadStatsData>();
        ref var statsBlob = ref statsData.StatsBlob.Value; 
        
        foreach (var (squad, BattlefieldBonusBufferElement, ApplyChargeBonusTag) 
            in SystemAPI.Query<
                SquadEntity,
                DynamicBuffer<BattlefieldBonusBufferElement>,
                ApplyChargeBonusTag
            >())
        {
            entityCommandBuffer.RemoveComponent<ApplyChargeBonusTag>(squad.SelfEntity);
            // Debug.Log($"SquadChargeBonusApplicationSystem: applying charge bonus to squad {squad.SquadId}");
            if (SystemAPI.HasComponent<InForestTag>(squad.SelfEntity) ||
                SystemAPI.HasComponent<InSwampTag>(squad.SelfEntity) ||
                SystemAPI.HasComponent<InRainTag>(squad.SelfEntity))
            {
                Debug.LogWarning($"SquadChargeBonusApplicationSystem: Squad {squad.SquadId} is in forest or swamp or rain, dont apply charge bonus.");
                continue;
            }

            if (squad.TargetSquadEntity != Entity.Null && SystemAPI.HasComponent<GarrisonGateSquadTag>(squad.TargetSquadEntity))
            {
                Debug.LogWarning($"SquadChargeBonusApplicationSystem: Squad {squad.SquadId} is charging a gate, dont apply charge bonus.");
                continue;
            }

            SquadStats squadStats = statsBlob.GetStats(squad.UnitName);
            int bonus = squadStats.ChargeBonus;
            if(campaignSaveDataHolder.ActiveHeroID == 1 && squad.Team== Team.Player) {
                bonus +=2;
            }

            //apply bonus
            BattlefieldBonusBufferElement.Add(new BattlefieldBonusBufferElement { 
                Value = new BattlefieldBonus { 
                    UnitStat = UnitStat.MeleeAttack, 
                    BattlefieldBonusEnum = BattlefieldBonusEnum.ChargeBonus,
                    Team = Team.Neutral,
                    Value = bonus,
                    Range = Mathf.Infinity,
            } });
            BattlefieldBonusBufferElement.Add(new BattlefieldBonusBufferElement { 
                Value = new BattlefieldBonus { 
                    UnitStat = UnitStat.WeaponStrength, 
                    BattlefieldBonusEnum = BattlefieldBonusEnum.ChargeBonus,
                    Team = Team.Neutral,
                    Value = bonus,
                    Range = Mathf.Infinity,
            } });
        }     
    }
}

