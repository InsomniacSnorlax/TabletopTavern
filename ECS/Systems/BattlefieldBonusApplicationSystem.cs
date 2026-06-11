using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

partial struct BattlefieldBonusApplicationSystem : ISystem
{
    public Unity.Mathematics.Random random;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // state.RequireForUpdate<BattlePhase>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var battlefieldBonusApplicator in SystemAPI.Query<RefRW<BattlefieldBonusApplicator>>())
        {
            //only fire if timer is up
            battlefieldBonusApplicator.ValueRW.Timer -= SystemAPI.Time.DeltaTime;
            if (battlefieldBonusApplicator.ValueRO.Timer > 0f)
            {
                continue;
            }
            battlefieldBonusApplicator.ValueRW.Timer = battlefieldBonusApplicator.ValueRO.TimerMax;

            foreach (var (squadMovement2, squadEntity) in SystemAPI.Query<RefRO<SquadMovementComponent>, SquadEntity>())
            {
                //check if target team
                if (battlefieldBonusApplicator.ValueRO.BattlefieldBonus.Team != Team.Neutral && squadEntity.Team != battlefieldBonusApplicator.ValueRO.BattlefieldBonus.Team) continue;

                float distance = math.distance(battlefieldBonusApplicator.ValueRO.BattlefieldBonus.OriginationPoint, squadMovement2.ValueRO.SquadCenter);
                // Debug.Log($"BattlefieldBonusApplicationSystem: Distance between is {distance}");
                if (distance > battlefieldBonusApplicator.ValueRO.BattlefieldBonus.Range)
                {
                    continue;
                }

                //check if has buffer element
                if (!SystemAPI.HasBuffer<BattlefieldBonusBufferElement>(squadEntity.SelfEntity))
                {
                    continue;
                }

                //get bonus buffer
                DynamicBuffer<BattlefieldBonusBufferElement> bonusBuffer = SystemAPI.GetBuffer<BattlefieldBonusBufferElement>(squadEntity.SelfEntity);

                bool hasBonus = false;
                //check if already has bonus 
                foreach (BattlefieldBonusBufferElement bonusBufferElement in bonusBuffer)
                {
                    if (bonusBufferElement.Value.Guid == battlefieldBonusApplicator.ValueRO.BattlefieldBonus.Guid)
                    {
                        hasBonus = true;
                        break;
                    }
                }

                if (hasBonus) continue;

                //apply bonus
                bonusBuffer.Add(new BattlefieldBonusBufferElement { Value = battlefieldBonusApplicator.ValueRO.BattlefieldBonus });
                // Debug.Log($"BattlefieldBonusApplicationSystem: Applied bonus {battlefieldBonusApplicator.ValueRO.BattlefieldBonus.UnitStat} to {squadEntity.SelfEntity}");
            }
        }
    }
}