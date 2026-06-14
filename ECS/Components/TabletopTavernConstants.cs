using UnityEngine;
using TJ;
using Unity.Mathematics;

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
    public const float RANGED_TOTAL_DAMAGE_MODIFIER = 1.10f;
    public const float MORALE_LOSS_MODIFIER = 0.75f;

    // Campaign
    public const float RESERVES_HEAL_AMOUNT = 0.5f;
    public const float ENDLESS_HORDES_HEAL_AMOUNT = 0.30f;
    public const float CONSUME_CAPTIVES_HEAL_AMOUNT = 0.33f;
    public const int PRESTIGE_BONUS = 10;
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
}