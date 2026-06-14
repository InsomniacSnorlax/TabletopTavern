using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using TJ;

public enum UnitType { Melee, Ranged, Hybrid, Artillery, Structure }
public enum UnitCondition { None, InForest, InCombat, IsCharging, IsTerrified, InSwamp, IsExhausted, IsOutOfAmmo }
public enum UnitStat { MeleeAttack, MeleeDefense, WeaponStrength, 
    Accuracy, Range, MissileStrength, HitPoints, None, Speed, 
    Armor, ChargeBonus, Leadership, Ammunition, ChargeImpactDamage }
public enum UnitSize { Infantry, Cavalry, Monstrous, SingleUnit, Artillery }
public enum UnitRarity { Common, Uncommon, Rare, Legendary }
public enum Team { Player, Enemy, Neutral }
public enum FormationType { Triangle, Radial, Box}
public enum GamePhase { SetUp, Deployment, Battle, PostGame }
public enum BattleLayoutType { Normal, PlayerEncircled, EnemyEncircled, EnemyDeferred }
public enum Race { IronLegion, Gruntkin, RavenHost, TaelindorForest, SanguineCourt, SakuraDynasty, DeepstoneHold, DrakosaurBrood, Special } //Olympian Phalanx
//Imperial Edict: First recruitment pack is free upon entering a town
//Endless Hordes: Heal all units by 20% of their max health after winning a battle
//Starlit Guidance: Grants the ability to spend 5<sprite name=GoldSprite> to reveal the outcome of the next dice roll during events
//Elite Ravagers: Sacking towns prestiges two random units
//Raise Dead: After every battle, gain 2 free [Tier 1] units
//Bushido Discipline: If army contains only Sakura Dynasty units, all units gain +20 [Leadership] and +4 [Melee Attack]
//Anvil's Second Strike: Gain a free reroll when collecting gear from chests
//A Feast for Beasts: After sacking a town, heal all units by 50% of their max health
public enum UnitName
{
    PeasantBowmen, LevySwordsmen, FieldPikemen,
    LandsknechtGreatswords, Arbalesters, MilanesePolearms,
    ImperialTemplars, DeepwoodRangers, BlackEagleHalberdiers,
    BorderlandRiders, HearthboundKnights, RoyalCavaliers,

    GoblinRabble, GoblinScrapShooters, DireWolves,
    OrcRavagers, OrcImpalers, OrcStalkers,
    BogmawTroll, ArmoredDirewolves, StonehewerGiant,
    StonegulletEnforcers, FleshshredderFanatics, Direriders,

    ThrallLevy, DriftwoodSkirmishers, SeawindSpears,
    Huskarls, Shieldmaidens, SkogarmadrArchers,
    Berserkers, Valkyries, VarangianGuard, SonsOfFenrir,
    Jomsvikings,

    SylvanArchers, ForestSpirits, AshwoodMilitia,
    AerindelGuard, Duskspears, MistweaverScouts, StarStriders,
    Veilpiercers, Treants, ElarionSentinels, SunspireBladelords,
    VeilkinDrakes,

    UndeadLevies, BoneclatterSpears, FeralHounds, GravestoneImps,
    DeathhavenFiends, Nightriders, BoneshardArchers, CorpseClaws,
    MistWraiths, BlackWardens, Bloodsworn, BloodswornKnights,

    AshigaruSpearmen, RoninWanderers, DaikyuCommoners,
    FootSamurai, NaginataOnna, KunoichiInfiltrators,
    DragonTachi, EmperorsArquebusiers, Hokoshu, KitsuneBlademasters,
    Oni,

    EisenmannRegiment, Ashguard, AngelsOfDeath,

    RiftpickLaborers, CrackshotCrossbows,
    Cragflayers, HelmwallDefenders, DrakefireRiflers, GrimfireGuns,
    ThunderhoofChargers, GrimazulAncients, GlyphstonePhalanx, StormForgedBattery, TheBulwark,
    AbyssalDelveknights, ForgewrathHammers,

    KaiserCannon, BanryuBombardiers, JoseonHwacha, DraugrBoltThrowers, Gobbopult, LorandelStarhurler,
    ArchersOfApollo,

    Meatgrinders, Siegeclaws, Shadelords, NecroticChimerae, GoldenSaru, EmeraldAncient,

    KoboldBrawlers, ScalebowKobolds, DireRaptors,
    VenomtailArchers, Brutes, Redhorns, RaptorRiders, TriceraPlatform,
    BlackDragon, Leviathans, StegoplateGuard, BloodCarnos,
    Kaiju, ObsidianScales,

    Gate,
}
[System.Serializable] public struct SquadSpawnData {
    public int squadId;
    public UnitType unitType;
    public UnitName unitName;
    public Team Team;
    public List<float3> entityPositions;
    public List<int> unitIndiciesList;
    public int2 widthAndDepth;
    public quaternion squadRotation;
}
[System.Serializable] public struct SquadSaveData {
    public List<SquadSpawnData> squads;
    public List<SquadSpawnData> enemies;
}
[System.Serializable] public struct UnitFormationNoise {
    public UnitName unitName;
    [Range(0, 1)] public float noise;
}
[System.Serializable] public struct SpawnBox {
    public float3 min;
    public float3 max;
}
public struct GarrisonConcaveZone
{
    public float wallZ;          // Z of the flank wall segments
    public float middleZ;        // Z of the recessed middle section (= wallZ when flat)
    public float leftConnectorX; // X boundary between left flank and middle
    public float rightConnectorX;// X boundary between middle and right flank
    public float battleMinX;     // left edge of the battle zone
    public float battleMaxX;     // right edge of the battle zone
    public float battleMaxZ;     // back edge of the battle zone (enemy far side)
    public bool isFlat;          // true when inwardDepth <= 0 or wall too narrow for sections

    public bool IsInsideEnemyZone(float x, float z)
    {
        if (isFlat) return z >= wallZ;
        bool inMiddle = x > leftConnectorX && x < rightConnectorX;
        return z >= (inMiddle ? middleZ : wallZ);
    }

    public float GetSectionZ(float x)
    {
        if (isFlat) return wallZ;
        return (x > leftConnectorX && x < rightConnectorX) ? middleZ : wallZ;
    }

    public (float minX, float maxX) GetSectionXRange(float x)
    {
        if (isFlat) return (battleMinX, battleMaxX);
        if (x <= leftConnectorX) return (battleMinX, leftConnectorX);
        if (x >= rightConnectorX) return (rightConnectorX, battleMaxX);
        return (leftConnectorX, rightConnectorX);
    }
}
[System.Serializable] public struct SquadBounds
{
    public float3 bottomLeft;
    public float3 bottomRight;
    public float3 topLeft;
    public float3 topRight;
}
public struct UnitStatBonus
{
    public UnitStat UnitStat;
    public string BonusName;
    public float Value;
    public UnitStatBonus(UnitStat _unitStat, string _bonusName, float _value)
    {
        UnitStat = _unitStat;
        BonusName = _bonusName;
        Value = _value;
    }
}
public enum UnlockCondition { None, NotAvailableInDemo, DiscordExclusive, NewsletterExclusive, HeroCompletion }
public struct UnitAttributeBonus
{
    public UnitAttribute UnitAttribute;
    public string BonusName;
    public float Value;
    public UnitAttributeBonus(UnitAttribute _unitStat, string _bonusName, float _value)
    {
        UnitAttribute = _unitStat;
        BonusName = _bonusName;
        Value = _value;
    }
}
public struct UnitStatValue 
{ 
    public UnitStat unitStat; 
    public float Value;
    public UnitStatValue(UnitStat _unitStat, float _value)
    {
        unitStat = _unitStat;
        Value = _value;
    }
}
[System.Serializable]
public class UnitTier
{
    public UnitName unitName;
    public int tier; // 1 for low tier, 2 for mid-tier, 3 for high tier
}
public enum EngagementType { Skirmish, Horde };
public static class DataTypes
{
    public static float[] GetTierProbabilities(int reputation)
    {
        if (reputation <= 33) return new float[] { 0.7f, 0.25f, 0.05f, 0f };
        else if (reputation <= 66) return new float[] { 0.45f, 0.45f, 0.1f, 0f };
        else if (reputation <= 90) return new float[] { 0.1f, 0.7f, 0.2f, 0f };
        else if (reputation <= 100) return new float[] { 0.05f, 0.25f, 0.5f, 0.2f };
        else
        {
            Debug.LogError($"GetTierProbabilities: Invalid reputation value: {reputation}");
            return new float[] { };
        }
    }
    public static int GetFormationWidthFromUnitCount(int unitCount)
    {
        return unitCount switch
        {
            > 48 => 14,
            > 24 => 12,
            > 8 => 8,
            > 3 => 4,
            3 => 3,
            1 => 1,
            _ => 69,
        };
    }
}
