using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpriteData
{
    public static Sprite GetSprite(string _iconName)
    {
        string path = GetIconAt(_iconName);
        if(path.Contains("Failed to load icon")) Debug.LogError($"Failed to load icon {_iconName}");
        Sprite sprite = Resources.Load<Sprite>(path);
        if(sprite == null) Debug.LogError($"No sprite found at {path}");
        return sprite;
    }
    private static string GetIconAt(string _iconName)
    {
        return _iconName switch
        {
            "playerFaction" => "Sprites/Factions/playerFaction",
            "enemyFaction" => "Sprites/Factions/enemyFaction",

            "LightningStrike" => "Sprites/Spells/LightningStrike",
            "NaturesWrath" => "Sprites/Spells/NaturesWrath",

            "Event" => "Sprites/Map/Event",
            "Skirmish" => "Sprites/Map/Skirmish",
            "Recruit" => "Sprites/Map/Recruit",
            "Warband" => "Sprites/Map/Warband",
            "Shop" => "Sprites/Map/Shop",
            "Town" => "Sprites/Map/Town",
            "Horde" => "Sprites/Map/Horde",
            "Test" => "Sprites/Map/Test",
            "Treasure" => "Sprites/Map/Treasure",

            "PositiveReputation" => "Sprites/Events/PositiveReputation",
            "NegativeReputation" => "Sprites/Events/NegativeReputation",
            "Gold" => "Sprites/Events/Gold",
            "UnitHealth" => "Sprites/Events/TroopHealth",
            "GearDrop" => "Sprites/Events/GearDrop",
            "PrestigeUnit" => "Sprites/Events/PrestigeUnit",

            //gear
            "Arming Swords" => "Sprites/Gear/Arming Swords",
            "Buckler Shields" => "Sprites/Gear/Buckler Shields",
            "Longbows" => "Sprites/Gear/Longbows",
            "Glaives" => "Sprites/Gear/Glaives",
            "Conscription Orders" => "Sprites/Gear/Conscription Orders",
            "Texan BBQ" => "Sprites/Gear/Texan BBQ",
            "Ballistic Charts" => "Sprites/Gear/Ballistic Charts",
            "Iron Bank" => "Sprites/Gear/Iron Bank",
            "Omen of Famine" => "Sprites/Gear/Omen of Famine",
            "Quantitative Easing Policy" => "Sprites/Gear/Quantitative Easing Policy",
            "Mitre" => "Sprites/Gear/Mitre",
            "Michaels Secret Stuff" => "Sprites/Gear/Michaels Secret Stuff",
            "Chug Jug" => "Sprites/Gear/Chug Jug",
            "Enron Accounting" => "Sprites/Gear/Enron Accounting",
            "Ornate Ring" => "Sprites/Gear/Ornate Ring",
            "Jailers Key" => "Sprites/Gear/Jailers Key",
            "Bracelet of the Sun Goddess" => "Sprites/Gear/Bracelet of the Sun Goddess",
            "The Potato" => "Sprites/Gear/The Potato",
            "Cauldron" => "Sprites/Gear/Caldron",
            "Shungite" => "Sprites/Gear/Shungite",

            "Jousting Lances" => "Sprites/Gear/Jousting Lances",
            "Common Builder" => "Sprites/Gear/Common Builder",
            "Uncommon Builder" => "Sprites/Gear/Uncommon Builder",
            "Rare Builder" => "Sprites/Gear/Rare Builder",
            "Gnomish Armorers" => "Sprites/Gear/Gnomish Armorers",
            "Bear Spray" => "Sprites/Gear/Bear Spray",
            "Cookie and Fowl Card" => "Sprites/Gear/Costco Card",
            "Privateering Papers" => "Sprites/Gear/Privateering Papers",
            "Northern Looters" => "Sprites/Gear/Northern Looters",
            "Pumpkin Pie" => "Sprites/Gear/Pumpkin Pie",
            "Aura Farming" => "Sprites/Gear/Hippy Dippy Aura",
            "Dwarven Tax Collectors" => "Sprites/Gear/Dwarven Tax Collectors",
            "Diamond Tipped Arrows" => "Sprites/Gear/Diamond Tipped Arrows",
            "Well Honed Axes" => "Sprites/Gear/Well Honed Axes",
            "Ravens Eye" => "Sprites/Gear/Ravens Eye",
            "Turkey" => "Sprites/Gear/Turkey",
            "River Trout" => "Sprites/Gear/River Trout",
            "Ring of the Elven King" => "Sprites/Gear/Ring of the Elven King",
            "Lucky Horseshoe" => "Sprites/Gear/Lucky Horseshoe",
            "Heavy Weapons" => "Sprites/Gear/Heavy Weapons",

            //icons on gear
            "ArrowBlockChance" => "Sprites/Modifiers/MeleeDefense",
            "Health" => "Sprites/Modifiers/Health",
            "AttackDamage" => "Sprites/Modifiers/MeleeAttack",

            "MeleeAttack" => "Sprites/Modifiers/MeleeAttack",
            "MeleeDefense" => "Sprites/Modifiers/MeleeDefense",
            "WeaponStrength" => "Sprites/Modifiers/WeaponStrength",
            "Speed" => "Sprites/Modifiers/Speed",
            "Armor" => "Sprites/Modifiers/Armor",
            "MissileStrength" => "Sprites/Modifiers/MissileStrength",
            "Range" => "Sprites/Modifiers/Range",
            "Accuracy" => "Sprites/Modifiers/Accuracy",
            "ChargeBonus" => "Sprites/Modifiers/ChargeBonus",
            "Leadership" => "Sprites/Modifiers/Leadership",
            "Ammunition" => "Sprites/Modifiers/Ammunition",
            "ChargeImpactDamage" => "Sprites/Modifiers/ChargeImpactDamage",

            "engagement" => "Sprites/Map/Engagement",
            "shop" => "Sprites/Map/Merchant",
            "Unknown" => "Sprites/Map/Unknown",
            "EndTurn" => "Sprites/Map/EndTurn",

            //consumables
            "MinorHealth" => "Sprites/Consumables/MinorHealth",
            "MajorHealth" => "Sprites/Consumables/MajorHealth",
            "Prestige" => "Sprites/Consumables/Prestige",
            "Duplicate" => "Sprites/Consumables/Duplicate",
            "NewUnit" => "Sprites/Consumables/NewUnit",
            "Alchemist" => "Sprites/Consumables/Alchemist",
            "Rewind" => "Sprites/Consumables/Rewind",
            "RunewellNectar" => "Sprites/Consumables/RunewellNectar",
            "TrialofGrasses" => "Sprites/Consumables/TrialofGrasses",
            "FateshineElixir" => "Sprites/Consumables/FateshineElixir",
            "LambSauce" => "Sprites/Consumables/LambSauce",

            //Heros
            "EdricValeward" => "Screenshots/Processed/EdricValeward",
            "RhydanGreythorne" => "Screenshots/Processed/RhydanGreythorne",
            "BoblinTheGoblinKing" => "Screenshots/Processed/BoblinTheGoblinKing",
            "KragmukGorethirster" => "Screenshots/Processed/KragmukGorethirster",
            "FreyjaStormweaver" => "Screenshots/Processed/FreyjaStormweaver",
            "BjornIronskull" => "Screenshots/Processed/BjornIronskull",
            "IltharionStarpire" => "Screenshots/Processed/IltharionStarpire",
            "SerendaelOfNytherial" => "Screenshots/Processed/SerendaelOfNytherial",
            "LordDravenBloodreaver" => "Screenshots/Processed/LordDravenBloodreaver",
            "SisterMorvayne" => "Screenshots/Processed/SisterMorvayne",
            "OdaNobukage" => "Screenshots/Processed/OdaNobukage",
            "TokugawaHarunobu" => "Screenshots/Processed/TokugawaHarunobu",
            "HrothgarGoblinslayer" => "Screenshots/Processed/HrothgarGoblinslayer",
            "BerthaBarrelstorm" => "Screenshots/Processed/BerthaBarrelstorm",
            "SkrixTheSwarmcaller" => "Screenshots/Processed/SkrixTheSwarmcaller",
            "ValthrexPrimeclaw" => "Screenshots/Processed/ValthrexPrimeclaw",

            _ => "Sprites/Failed to load icon"
        };
    }
   
}