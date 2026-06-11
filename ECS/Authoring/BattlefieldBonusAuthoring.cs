using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using ProjectDawn.Navigation;
using System;

public class BattlefieldBonusAuthoring : MonoBehaviour
{
    public BattlefieldBonus BattlefieldBonus;
    public float TimerMax = 1f;

    public class Baker : Baker<BattlefieldBonusAuthoring> 
    {
        public override void Bake(BattlefieldBonusAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.WorldSpace);

            AddComponent(entity, new BattlefieldBonusApplicator {
                BattlefieldBonus = authoring.BattlefieldBonus,
                TimerMax = authoring.TimerMax
            });
        }
    }
}
public enum BattlefieldBonusEnum { None, StatueOfBoromid, Rain, Blacksmith, GoblinBarracks, GoblinCamp, ShrineToTheAllMother, Watchtower, ChargeBonus, Forest, Swamp, BloodFrenzy, Rage, Emblazing, Fog, Snow, CrashingHorde, Deathcry, HuntersPatience, KenseiEye, Oathcarved, ApexHunters }
public struct BattlefieldBonusApplicator : IComponentData
{
    public BattlefieldBonus BattlefieldBonus;
    public float Timer;
    public float TimerMax;
}
public struct InForestTag : IComponentData { }
public struct InSwampTag : IComponentData { }
public struct InRainTag : IComponentData { }
public struct InSnowTag : IComponentData { }

public struct RageApplicatorTag : IComponentData { }
public struct RageActiveTag : IComponentData { }
public struct RemoveRageTag : IComponentData { }

public struct SlayerApplicatorTag : IComponentData { }
public struct SlayerActiveTag : IComponentData { }

public struct BloodFrenzyApplicatorTag : IComponentData { }
public struct BloodFrenzyActiveTag : IComponentData { }
public struct RemoveBloodFrenzyTag : IComponentData { }

public struct EmblazerTag : IComponentData { }
public struct ArmorSunderedTag : IComponentData { }
public struct EmblazingApplicatorTag : IComponentData { }
public struct RemoveEmblazingTag : IComponentData { }

public struct RemoveBattlefieldBonusRain : IComponentData { }
public struct RemoveBattlefieldBonusSnow : IComponentData { }
public struct RemoveBattlefieldBonusFog : IComponentData { }

public struct RemoveSwampTag : IComponentData { }
public struct RemoveForestTag : IComponentData { }

public struct ApplyBiomeBonusTag : IComponentData { public BattlefieldBonusEnum BattlefieldBonusEnum; public Guid Guid; }

[System.Serializable] public struct BattlefieldBonus : IComponentData
{
    public UnitStat UnitStat;
    public BattlefieldBonusEnum BattlefieldBonusEnum;
    public Team Team;
    public float Value;
    public Guid Guid;
    public float3 OriginationPoint;
    public float Range;
    public bool Applied;
    public int TargetedUnit;
}
[InternalBufferCapacity(8)] // Optional: set the internal buffer capacity
[System.Serializable] public struct BattlefieldBonusBufferElement : IBufferElementData {
    public BattlefieldBonus Value;
}
