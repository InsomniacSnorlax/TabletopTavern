using Unity.Entities;
using Unity.Mathematics;

// Race identity tags — added to squad entities during RegisterSquad
public struct IronLegionRaceTag : IComponentData {}
public struct GruntkinRaceTag : IComponentData {}
public struct RavenHostRaceTag : IComponentData {}
public struct TaelindorForestRaceTag : IComponentData {}
public struct SanguineCourtRaceTag : IComponentData {}
public struct SakuraDynastyRaceTag : IComponentData {}
public struct DeepstoneHoldRaceTag : IComponentData {}
public struct DrakosaurBroodRaceTag : IComponentData {}

// Iron Legion — "Iron Resolve"
// Clamps morale at Wavering for 10 seconds when squad would break
public struct IronResolveComponent : IComponentData
{
    public float ClampTimer;
    public bool IsClamped;
}

// Gruntkin — "Warband Rush"
// Tracks how many stacks of +5 WeaponStrength are currently applied to this squad
public struct CrashingHordeComponent : IComponentData
{
    public int AppliedStacks;
}

// RavenHost — "Deathcry"
// Tracks remaining duration and total bonus applied to this squad
public struct DeathcryComponent : IComponentData
{
    public float TimeRemaining;
    public int AppliedBonus;
}
// Added to a destroyed RavenHost squad so the trigger only fires once
public struct DeathcryTriggeredTag : IComponentData {}

// TaelindorForest — "Hunter's Patience"
// Ranged squads stack Accuracy, melee squads stack WeaponStrength while stationary
public struct HuntersPatienceComponent : IComponentData
{
    public float StationaryTime;
    public int CurrentBonus;
    public float3 LastPosition;
    public bool IsRanged;
}

// SakuraDynasty — "Kensei's Eye"
// Stacks WeaponStrength bonus at 10s / 20s / 30s of continuous melee combat
public struct KenseiEyeComponent : IComponentData
{
    public float CombatTime;
    public int CurrentStage; // 0-3; bonus = CurrentStage * 10
}

// DeepstoneHold — "Oathcarved"
// Counts unit deaths for bookkeeping (actual bonuses applied directly to units)
public struct OathcarvedComponent : IComponentData
{
    public int DeathCount;
}

// DrakosaurBrood — "Pack Instinct"
// Tracks stacks currently applied so delta changes are possible each update
public struct ApexHuntersComponent : IComponentData
{
    public int AppliedStacks;
    public float UpdateTimer;
}

