using UnityEngine;
using TJ;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;

public static class TabletopTavernConstants
{
    // Layers
    public const int UNITS_LAYER = 6;
    public const int TILE_LAYER = 7;
    public const int BATTLEFIELD_BONUS_LAYER = 17;
    public const int SWAMP_LAYER = 10;
    public const int FOREST_LAYER = 19;
    public const int SQUAD_FLAG_LAYER = 18;

    // Animations
    public const float IDLE_ANIMATION_FREQUENCY = 0.001f;
    public const float COMBAT_ANIMATION_FREQUENCY = 0.01f;
    public const float MELEE_ATTACK_DISTANCE = 8f;
    public const float MELEE_DETECTION_RANGE = 8f;
    public const float DAZED_ON_DISENGAGE_TIME = 3f;
    public const float RANGED_ATTACK_COOLDOWN = 5f;
    public const float ARTILLERY_ATTACK_COOLDOWN = 10f;
    public const int MELEE_ATTACK_IDLE_ID = 3;
    public const int MELEE_ATTACK_ID = 4;
    public const int RANGED_ATTACK_IDLE_ID = 13;
    public const int RANGED_ATTACK_ID = 14;
    public const int CAVALRY_DEATH_ANIMATION_ID = 2;

    // Combat
    public const int TIME_REQUIRED_FOR_CHARGE_BONUS = 2;
    public const int TIME_TO_REMOVE_CHARGE_BONUS = 6;
    public const float TERROR_RADIUS = 20f;
    public const int OVERIDE_TARGET_SQUADENTITY_DISTANCE = 20;
    // Melee pursuit watchdog: abandon an uncatchable (kiting) target that isn't being closed on.
    public const float MELEE_PURSUIT_GIVEUP_TIME = 4f;          // no-closing time before abandon
    public const float MELEE_PURSUIT_CLOSE_EPSILON = 0.75f;     // min improvement counted as progress
    public const float MELEE_TARGET_BLACKLIST_COOLDOWN = 8f;    // how long an abandoned kiter is ignored
    public const float RANGED_REPRIORITIZE_CLOSER_FRACTION = 0.66f; // ranged: switch aim only if >=34% closer
    public const int ARCHER_FLEE_DISTANCE = 20;
    public const int WITHDRAW_DISTANCE = 155;
    public const int MINIMUM_ARCHER_RANGE = 15;
    public const int FIRE_AT_WILL_ACCURACY_PENALTY = 20;
    public const float NEARBY_ALLIES_RETREATING_PENALTY_TIME = 10f;
    public const int NEARBY_ALLIES_RETREATING_DISTANCE = 40;

    // Speed Modifiers
    public const float SWAMP_SPEED_MODIFIER = 0.5f;
    public const float FOG_MODIFIER = 0.5f;
    public const float RAIN_SPEED_MODIFIER = 0.5f;
    public const float SNOW_MORALE_PENALTY = -10f;
    public const float FORTIFIED_MORALE_BONUS = 10f;

    // Spreads
    public const float InfantrySpread = 2f;
    public const float CavalrySpread = 3f;
    public const float MonsterSpread = 5f;
    public const float ArtillerySpread = 4f;
    public const float SingleUnitSpread = 0.01f;

    // Colors
    public static readonly Color PLAYER_TRIANGLE_COLOR = new(1f, 0.75f, 0f, 1f);
    public static readonly Color ENEMY_TRIANGLE_COLOR = new(1f, 0f, 0f, 1f);
    public static readonly Color DISABLED_TRIANGLE_COLOR = new(0, 0, 0, 0);
    public const int TRIANGLE_HOVER_BLOOM = 5;
    public const int TRIANGLE_SELECTED_BLOOM = 10;

    // Damage Modifiers
    public const float MELEE_TOTAL_DAMAGE_MODIFIER = 0.25f;
    public const float RANGED_TOTAL_DAMAGE_MODIFIER = 1.0f;
    public const float MORALE_LOSS_MODIFIER = 0.75f;
    public const float ARTILLERY_VS_ARTILLERY_DAMAGE_MODIFIER = 0.5f;

    // Campaign
    public const float RESERVES_HEAL_AMOUNT = 0.5f;
    public const float ENDLESS_HORDES_HEAL_AMOUNT = 0.30f;
    public const float CONSUME_CAPTIVES_HEAL_AMOUNT = 0.33f;

    //Prestige: melee gains MeleeAttack/MeleeDefense/Leadership, ranged gains Range/Accuracy/Ammunition, all scaled by prestige level
    public const int PRESTIGE_BONUS = 5;
    public const int PRESTIGE_AMMO_BONUS_RANGED = 100;
    public const int PRESTIGE_AMMO_BONUS_ARTILLERY = 6;

    // Random trait pool granted once when a unit reaches max prestige (level 2 / "Prestige 3")
    public static readonly UnitAttribute[] PRESTIGE_TRAIT_POOL = {
        UnitAttribute.ArmorPiercing, UnitAttribute.AntiInfantry, UnitAttribute.AntiLarge,
        UnitAttribute.Terrifying, UnitAttribute.Stalwart, UnitAttribute.Rage,
        UnitAttribute.Emblazing, UnitAttribute.FlamingAmmo,
        UnitAttribute.BackStabbers, UnitAttribute.MonsterSlayer, UnitAttribute.BloodFrenzy
    };


    public const int VILLAGE_RECRUIT_COST = 10;
    public const int CASTLE_RECRUIT_COST = 20;
    public const int CITY_RECRUIT_COST = 40;
    public const int FORGEFURY_TEMPERING_KILLS_REQUIRED = 50;
    // public const int MAX_DEMO_DEPOSITED_GOLD = 200000; //max gold that can be deposited in demo version of the game
    public const int RANSOM_CAPTIVES_REWARD = 3;
    public const int SKIRMISH_REWARD = 3;
    public const int HORDE_REWARD = 6;
    public const float CONSCRIPT_SURVIVORS_HEALTH_PERCENTAGE = 0.5f;
    public static readonly int2 BATTLEFIELD_BONUSES_RANGE = new (0, 2);
    public const string GOLD_SPRITE_STRING = "<sprite name=GoldSprite>";

    // Wishlists
    public const string WISHLIST_COUNT = "120,780";

    #region Economy Mod Overrides
    // Consts above are compiler-inlined at every call site, so a runtime override can't intercept
    // them directly - these dictionaries/nullable fields back the wrapper functions below instead.
    // Note: the town-recruit-cost override (keyed by TownSize) is NOT here - TownSize lives in
    // TownPanel.cs, part of the root TabletopTavern.Core assembly, which this Components assembly
    // cannot reference (root already references Components; the reverse would be circular). See
    // TownSaveData.GetTownRecruitCost in TownPanel.cs instead, which reads VILLAGE/CASTLE/
    // CITY_RECRUIT_COST below as its fallback defaults (root -> Components is a valid direction).
    private static readonly Dictionary<int, int> UnitCostOverrides = new();
    private static int? RansomCaptivesRewardOverride, SkirmishRewardOverride, HordeRewardOverride;

    public static void ClearEconomyOverrides()
    {
        UnitCostOverrides.Clear();
        RansomCaptivesRewardOverride = SkirmishRewardOverride = HordeRewardOverride = null;
    }
    public static void SetUnitTierCostOverride(int tier, int cost) => UnitCostOverrides[tier] = cost;
    public static void SetRansomCaptivesRewardOverride(int value) => RansomCaptivesRewardOverride = value;
    public static void SetSkirmishRewardOverride(int value) => SkirmishRewardOverride = value;
    public static void SetHordeRewardOverride(int value) => HordeRewardOverride = value;

    public static int GetRansomCaptivesReward() => RansomCaptivesRewardOverride ?? RANSOM_CAPTIVES_REWARD;
    public static int GetSkirmishReward() => SkirmishRewardOverride ?? SKIRMISH_REWARD;
    public static int GetHordeReward() => HordeRewardOverride ?? HORDE_REWARD;
    #endregion

    public static float GetSpread(UnitSize unitSize)
    {
        return unitSize switch
        {
            UnitSize.Infantry => InfantrySpread,
            UnitSize.Cavalry => CavalrySpread,
            UnitSize.Monstrous => MonsterSpread,
            UnitSize.Artillery => ArtillerySpread,
            UnitSize.SingleUnit => SingleUnitSpread,
            _ => 0.01f,
        };
    }
    public static int GetUnitCost(int _unitTier)
    {
        if (UnitCostOverrides.TryGetValue(_unitTier, out int overrideCost)) return overrideCost;
        return _unitTier switch
        {
            1 => 2,
            2 => 6,
            3 => 12,
            4 => 99,
            _ => 69,
        };
    }//<a href="https://www.flaticon.com/free-icons/jurassic" title="jurassic icons">Jurassic icons created by Marz Gallery - Flaticon</a>
    // Hybrid units that are typed as Ranged but should receive melee prestige bonuses
    public static bool UsesMeleePrestige(UnitName unitName) =>
        unitName == UnitName.Cragflayers || unitName == UnitName.Berserkers || unitName == UnitName.KunoichiInfiltrators;

    public static bool IsAGoblinUnit(UnitName unitName) =>
        unitName == UnitName.GoblinRabble || unitName == UnitName.GoblinScrapShooters || unitName == UnitName.StonegulletEnforcers;

    // Covers all 24 real SquadAttributes bool fields (every UnitAttribute value except None).
    // Armored/Large/Infantry/IsOnFire/TowerShields have no backing field (derived elsewhere or
    // vestigial) and fall through to false here - see SetAttribute for the same set rejected loudly.
    public static bool GetAttribute(SquadAttributes attributes, UnitAttribute trait) => trait switch
    {
        UnitAttribute.StandardShields => attributes.StandardShields,
        UnitAttribute.ArmorPiercing => attributes.ArmorPiercing,
        UnitAttribute.AntiInfantry => attributes.AntiInfantry,
        UnitAttribute.AntiLarge => attributes.AntiLarge,
        UnitAttribute.Terrifying => attributes.Terrifying,
        UnitAttribute.Stalwart => attributes.Stalwart,
        UnitAttribute.Outrider => attributes.Outrider,
        UnitAttribute.SwampCreature => attributes.SwampCreature,
        UnitAttribute.ForestDweller => attributes.ForestDweller,
        UnitAttribute.ChickenFlight => attributes.ChickenFlight,
        UnitAttribute.Ethereal => attributes.Ethereal,
        UnitAttribute.BloodFrenzy => attributes.BloodFrenzy,
        UnitAttribute.Rage => attributes.Rage,
        UnitAttribute.Emblazing => attributes.Emblazing,
        UnitAttribute.Unstoppable => attributes.Unstoppable,
        UnitAttribute.HeavyShields => attributes.HeavyShields,
        UnitAttribute.ThrowingAxes => attributes.ThrowingAxes,
        UnitAttribute.ArmorSundering => attributes.ArmorSundering,
        UnitAttribute.MonsterSlayer => attributes.MonsterSlayer,
        UnitAttribute.ForgefuryTempering => attributes.ForgefuryTempering,
        UnitAttribute.FlamingAmmo => attributes.FlamingAmmo,
        UnitAttribute.DragonsHoard => attributes.DragonsHoard,
        UnitAttribute.BackStabbers => attributes.BackStabbers,
        UnitAttribute.ThickScales => attributes.ThickScales,
        _ => false,
    };

    // UnitAttribute values with no SquadAttributes backing field - derived elsewhere (Armored,
    // Large, Infantry, IsOnFire) or vestigial (TowerShields). Not valid targets for data-driven
    // attribute grants.
    public static bool HasBackingField(UnitAttribute trait) => trait switch
    {
        UnitAttribute.None or UnitAttribute.Armored or UnitAttribute.Large or
        UnitAttribute.Infantry or UnitAttribute.IsOnFire or UnitAttribute.TowerShields => false,
        _ => true,
    };

    public static List<UnitAttribute> GetEligiblePrestigeTraits(SquadStats squadStats) =>
        PRESTIGE_TRAIT_POOL
            .Where(trait => !GetAttribute(squadStats.SquadAttributes, trait))
            .Where(trait => trait != UnitAttribute.FlamingAmmo || squadStats.unitType == UnitType.Ranged)
            .Where(trait => trait != UnitAttribute.Emblazing || squadStats.unitType == UnitType.Melee)
            .ToList();

    public static void SetAttribute(ref SquadAttributes attributes, UnitAttribute trait)
    {
        switch (trait)
        {
            case UnitAttribute.StandardShields: attributes.StandardShields = true; break;
            case UnitAttribute.ArmorPiercing: attributes.ArmorPiercing = true; break;
            case UnitAttribute.AntiInfantry: attributes.AntiInfantry = true; break;
            case UnitAttribute.AntiLarge: attributes.AntiLarge = true; break;
            case UnitAttribute.Terrifying: attributes.Terrifying = true; break;
            case UnitAttribute.Stalwart: attributes.Stalwart = true; break;
            case UnitAttribute.Outrider: attributes.Outrider = true; break;
            case UnitAttribute.SwampCreature: attributes.SwampCreature = true; break;
            case UnitAttribute.ForestDweller: attributes.ForestDweller = true; break;
            case UnitAttribute.ChickenFlight: attributes.ChickenFlight = true; break;
            case UnitAttribute.Ethereal: attributes.Ethereal = true; break;
            case UnitAttribute.BloodFrenzy: attributes.BloodFrenzy = true; break;
            case UnitAttribute.Rage: attributes.Rage = true; break;
            case UnitAttribute.Emblazing: attributes.Emblazing = true; break;
            case UnitAttribute.Unstoppable: attributes.Unstoppable = true; break;
            case UnitAttribute.HeavyShields: attributes.HeavyShields = true; break;
            case UnitAttribute.ThrowingAxes: attributes.ThrowingAxes = true; break;
            case UnitAttribute.ArmorSundering: attributes.ArmorSundering = true; break;
            case UnitAttribute.MonsterSlayer: attributes.MonsterSlayer = true; break;
            case UnitAttribute.ForgefuryTempering: attributes.ForgefuryTempering = true; break;
            case UnitAttribute.FlamingAmmo: attributes.FlamingAmmo = true; break;
            case UnitAttribute.DragonsHoard: attributes.DragonsHoard = true; break;
            case UnitAttribute.BackStabbers: attributes.BackStabbers = true; break;
            case UnitAttribute.ThickScales: attributes.ThickScales = true; break;
            default:
                Debug.LogWarning($"[TabletopTavernConstants] SetAttribute: '{trait}' has no SquadAttributes backing field, ignoring.");
                break;
        }
    }
}