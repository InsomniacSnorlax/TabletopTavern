using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using TJ;

partial struct EmblazingSystem : ISystem
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

        // Squads updating on entity destroyed
        foreach (var (squad, entity) in SystemAPI.Query<
            RefRO<SquadStateComponent>
            >().WithPresent<ArmorSunderedTag>().WithAbsent<InCombat>().WithEntityAccess())
        {
            // Debug.Log($"EmblazingSystem: Removing emblazing from squad ");
            //remove emblazing
            entityCommandBuffer.AddComponent<RemoveEmblazingTag>(entity);
            entityCommandBuffer.RemoveComponent<ArmorSunderedTag>(entity);
        }
    }
}
partial struct EmblazingApplicationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        var statsData = SystemAPI.GetSingleton<SquadStatsData>();
        ref var statsBlob = ref statsData.StatsBlob.Value; 
        
        // Squads updating on entity destroyed
        foreach (var (squad, battlefieldBonusBufferElement, entity) in SystemAPI.Query<
            RefRO<SquadEntity>,
            DynamicBuffer<BattlefieldBonusBufferElement>
            >().WithPresent<EmblazingApplicatorTag>().WithEntityAccess())
        {
            if(SystemAPI.HasComponent<ArmorSunderedTag>(entity))
            {
                entityCommandBuffer.RemoveComponent<EmblazingApplicatorTag>(entity);
                continue;
            }
            // Debug.Log($"EmblazingApplicationSystem: Applying emblazing to squad {squad.ValueRO.SquadId}");
            //apply emblazing
            entityCommandBuffer.RemoveComponent<EmblazingApplicatorTag>(entity);
            entityCommandBuffer.AddComponent<ArmorSunderedTag>(entity);

            SquadStats squadStats = statsBlob.GetStats(squad.ValueRO.UnitName);
            float bonus = squadStats.Armor;
            bonus /= bonus + 100f;

            battlefieldBonusBufferElement.Add(new BattlefieldBonusBufferElement
            {
                Value = new BattlefieldBonus
                {
                    UnitStat = UnitStat.Armor,
                    BattlefieldBonusEnum = BattlefieldBonusEnum.Emblazing,
                    Team = Team.Neutral,
                    Value = -bonus,
                    Range = Mathf.Infinity,
                }
            });
        }
    }
}

