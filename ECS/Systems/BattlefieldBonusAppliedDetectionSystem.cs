using Unity.Burst;
using Unity.Entities;

// Cross-cutting detector: fires a BattlefieldBonusAppliedBufferElement event the first frame
// any BattlefieldBonusEnum value appears in a squad's BattlefieldBonusBufferElement buffer.
// Covers every producer (race passives, hero abilities, zone/biome fixtures) generically by
// diffing buffer contents against a per-squad bitmask, so no producer system needs to change.
// Must run after every system that can write BattlefieldBonusBufferElement this frame; only
// OathcarvedSystem and DeathcrySystem run in LateSimulationSystemGroup, everything else runs
// in the (earlier) default SimulationSystemGroup.
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(OathcarvedSystem))]
[UpdateAfter(typeof(DeathcrySystem))]
partial struct BattlefieldBonusAppliedDetectionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonBuffer<BattlefieldBonusAppliedBufferElement>(out var appliedBuffer))
            return;

        foreach (var (bonusBuffer, seenMask, squad) in SystemAPI.Query<
            DynamicBuffer<BattlefieldBonusBufferElement>,
            RefRW<BattlefieldBonusSeenMask>,
            RefRO<SquadEntity>>())
        {
            ulong currentMask = 0;
            for (int i = 0; i < bonusBuffer.Length; i++)
            {
                BattlefieldBonusEnum bonusEnum = bonusBuffer[i].Value.BattlefieldBonusEnum;
                if (bonusEnum == BattlefieldBonusEnum.None) continue;
                currentMask |= 1ul << (int)bonusEnum;
            }

            ulong newBits = currentMask & ~seenMask.ValueRO.SeenMask;
            if (newBits != 0)
            {
                for (int bit = 0; bit < 64; bit++)
                {
                    if ((newBits & (1ul << bit)) == 0) continue;

                    for (int i = 0; i < bonusBuffer.Length; i++)
                    {
                        if ((int)bonusBuffer[i].Value.BattlefieldBonusEnum != bit) continue;

                        appliedBuffer.Add(new BattlefieldBonusAppliedBufferElement
                        {
                            SquadId = squad.ValueRO.SquadId,
                            BonusEnum = (BattlefieldBonusEnum)bit,
                            UnitStat = bonusBuffer[i].Value.UnitStat,
                            Value = bonusBuffer[i].Value.Value
                        });
                        break;
                    }
                }
            }

            // Reassign (never OR) - lets a bonus that clears and later re-activates fire again.
            seenMask.ValueRW.SeenMask = currentMask;
        }
    }
}
