using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Memori.Localization;

namespace TJ
{
    public static class ColorData
    {
        public static string Primary = "#ECF0F1";
        public static string Secondary = "#BDC3C7";
        public static string Green = "#43F86C";
        public static string Player = "#D49B39";
        public static string Enemy = "#D44339";
        public static string Error = "#CC2626";


        public static string Common = "#3AE94D";//"#49c419";//#c2bbb2";BDC3C7
        public static string Uncommon = "#3BA6EA";//"#1982c4";//#336b3e";
        public static string Rare = "#974DF3";//"#6119C4";//#567e9d";
        public static string Legendary = "#F1C40F";//"#4f3663";

        public static string Negative = "#D44339";
        public static string Positive = "#47D439";
        public static string Tan = "#E3BB71";
        public static string Gold = "#E3BB71";
        public static string TroopHealth = "#E37188";
        public static string GearDrop = "#95A5A6";

        public static string UnitStat = "#E3BB71";
        public static string DamageAttribute = "#E3BB71";

        public static string Tier1 = "#BDC3C7";
        public static string Tier2 = "#8AFA88";
        public static string Tier3 = "#BA88FA";
        public static string Tier4 = "#F1C40F";

        public static string MinimapPlayer = "#15ff00ff";
        public static string MinimapEnemy = "#ff0000ff";

        public static string PlayerTeamOutline = "#FFE300";
        public static string EnemyTeamOutline = "#FF0000";

        public static Vector4 HexToRgba(string hexColor)
        {
            hexColor = hexColor.TrimStart('#'); // Remove '#' if present

            // Parse hexadecimal values for red, green, blue, and alpha components
            int r = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            int a = hexColor.Length == 8 ? int.Parse(hexColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) : 255;

            // Normalize the color values from 0-255 to 0-1 range
            float rf = r / 255f;
            float gf = g / 255f;
            float bf = b / 255f;
            float af = a / 255f;

            return new Vector4(rf, gf, bf, af);
        }
        public static Vector4 GetEventOutcomeColor(string _eventOutcome)
        {
            return _eventOutcome switch
            {
                "PositiveReputation" => HexToRgba(Positive),
                "NegativeReputation" => HexToRgba(Negative),
                "Gold" => HexToRgba(Gold),
                "TroopHealth" => HexToRgba(TroopHealth),
                "GearDrop" => HexToRgba(GearDrop),
                "PresitgeUnit" => HexToRgba(Tan),
                _ => HexToRgba(Primary)
            };
        }
        public static Vector4 GetGearRarityColor(GearRarity _gearRarity)
        {
            return _gearRarity switch
            {
                GearRarity.Common => HexToRgba(Common),
                GearRarity.Uncommon => HexToRgba(Uncommon),
                GearRarity.Rare => HexToRgba(Rare),
                // GearRarity.Legendary => HexToRgba(Legendary),
                _ => HexToRgba(Primary)
            };
        }
        public static Vector4 GetUnitStatColor(UnitStat _unitStat)
        {
            return _unitStat switch
            {
                // UnitStat.AttackDamage => HexToRgba(AttackDamage),
                // UnitStat.UnitHitPoints => HexToRgba(TroopHealth),
                // UnitStat.Accuracy => HexToRgba(Accuracy),
                // UnitStat.Range => HexToRgba(Range),
                // UnitStat.AttackRate => HexToRgba(AttackRate),
                // UnitStat.HasShield => HexToRgba(HasShield),
                // UnitStat.UnitCount => HexToRgba(UnitCount),
                // UnitStat.Armor => HexToRgba(GearDrop),
                _ => HexToRgba(UnitStat)
            };
        }
        public static Vector4 GetRarityTierColor(UnitRarity _tier)
        {
            return _tier switch
            {
                UnitRarity.Common => HexToRgba(Tier1),
                UnitRarity.Uncommon => HexToRgba(Tier2),
                UnitRarity.Rare => HexToRgba(Tier3),
                UnitRarity.Legendary => HexToRgba(Tier4),
                _ => HexToRgba(Primary)
            };
        }
        public static string GetRarityTierColorString(UnitRarity _tier)
        {
            return _tier switch
            {
                UnitRarity.Common => Tier1,
                UnitRarity.Uncommon => Tier2,
                UnitRarity.Rare => Tier3,
                UnitRarity.Legendary => Tier4,
                _ => Primary
            };
        }
        public static Vector4 GetTeamMinimapColor(bool _isPlayerTeam)
        {
            return _isPlayerTeam ? HexToRgba(MinimapPlayer) : HexToRgba(MinimapEnemy);
        }
        public static string XMLTagColorApplicator(ref string _text)
        {
            if (_text.Length <= 0) return _text;

            string commonLocalized = "[" + LocalizationManager.Instance.GetText("Common") + "]";
            string uncommonLocalized = "[" + LocalizationManager.Instance.GetText("Uncommon") + "]";
            string rareLocalized = "[" + LocalizationManager.Instance.GetText("Rare") + "]";
            string legendaryLocalized = "[" + LocalizationManager.Instance.GetText("Legendary") + "]";

            //if text contains commonlocalized, replace common localized with the colordata xml tag
            if (_text.Contains(commonLocalized))
            {
                _text = _text.Replace(commonLocalized, $"<color={Tier1}>{commonLocalized}</color>");
            }
            if (_text.Contains(uncommonLocalized))
            {
                _text = _text.Replace(uncommonLocalized, $"<color={Tier2}>{uncommonLocalized}</color>");
            }
            if (_text.Contains(rareLocalized))
            {
                _text = _text.Replace(rareLocalized, $"<color={Tier3}>{rareLocalized}</color>");
            }
            if (_text.Contains(legendaryLocalized))
            {
                _text = _text.Replace(legendaryLocalized, $"<color={Tier4}>{legendaryLocalized}</color>");
            }

            string tier1Localized = "[" + LocalizationManager.Instance.GetText("Tier I") + "]";
            string tier2Localized = "[" + LocalizationManager.Instance.GetText("Tier II") + "]";
            string tier3Localized = "[" + LocalizationManager.Instance.GetText("Tier III") + "]";
            string tier4Localized = "[" + LocalizationManager.Instance.GetText("Tier IV") + "]";

            //if text contains commonlocalized, replace common localized with the colordata xml tag
            if (_text.Contains(tier1Localized))
            {
                _text = _text.Replace(tier1Localized, $"<color={Tier1}>{tier1Localized}</color>");
            }
            if (_text.Contains(tier2Localized))
            {
                _text = _text.Replace(tier2Localized, $"<color={Tier2}>{tier2Localized}</color>");
            }
            if (_text.Contains(tier3Localized))
            {
                _text = _text.Replace(tier3Localized, $"<color={Tier3}>{tier3Localized}</color>");
            }
            if (_text.Contains(tier4Localized))
            {
                _text = _text.Replace(tier4Localized, $"<color={Tier4}>{tier4Localized}</color>");
            }

            if (_text.Contains("+"))
            {
                int currentIndex = 0;
                while (currentIndex < _text.Length && currentIndex != -1)
                {
                    currentIndex = _text.IndexOf("+", currentIndex);
                    if (currentIndex == -1) break; // No more + found

                    int endIndex = _text.IndexOf(" ", currentIndex);
                    if (endIndex == -1) endIndex = _text.Length; // Use end of string if no space follows

                    if (endIndex > currentIndex)
                    {
                        string substring = _text.Substring(currentIndex, endIndex - currentIndex);
                        _text = _text.Replace(substring, $"<color={Green}>{substring}</color>");
                        currentIndex += substring.Length + Green.Length + 15; // Move past the replaced text (+15 for <color=...></color>)
                    }
                    else
                    {
                        currentIndex++; // Move past this + if no valid substring found
                    }
                }
            }
            string[] unitStats = new string[] { "MeleeAttack", "MeleeDefense", "WeaponStrength", "Accuracy", "Range", "MissileStrength", "HitPoints", "None", "Speed", "Armor", "ChargeBonus", "Leadership", "Ammunition", "ChargeImpactDamage", "Ranged" };
            string[] damageAttributes = new string[] { "None", "ArmorPiercing", "AntiInfantry", "AntiLarge", "ArmorPiercingAntiInfantry", "ArmorPiercingAntiLarge", "Terror", "Outrider", "Rage", "StandardShields", "Terrifying", "Stalwart", "Ethereal", "SwampCreature", "ForestDweller", "ChickenFlight", "BloodFrenzy", "Emblazing", "Unstoppable", "HeavyShields", "ThrowingAxes", "ArmorSundering", "ForgefuryTempering", "FlamingAmmo", "MonsterSlayer", "DragonsHoard", "BackStabbers" };//"TowerShields",

            string[] unitStatLocalized = new string[unitStats.Length];
            for (int i = 0; i < unitStats.Length; i++)
            {
                unitStatLocalized[i] = "[" + LocalizationManager.Instance.GetText(unitStats[i]) + "]";
            }
            string[] damageAttributesLocalized = new string[damageAttributes.Length];
            for (int i = 0; i < damageAttributes.Length; i++)
            {
                damageAttributesLocalized[i] = "[" + LocalizationManager.Instance.GetText(damageAttributes[i]) + "]";
            }

            for (int i = 0; i < unitStatLocalized.Length; i++)
            {
                if (_text.Contains(unitStatLocalized[i]))
                {
                    _text = _text.Replace(unitStatLocalized[i], $"<color={UnitStat}>{unitStatLocalized[i]}</color>");
                }
            }
            for (int i = 0; i < damageAttributesLocalized.Length; i++)
            {
                if (_text.Contains(damageAttributesLocalized[i]))
                {
                    _text = _text.Replace(damageAttributesLocalized[i], $"<color={DamageAttribute}>{damageAttributesLocalized[i]}</color>");
                }
            }

            return _text;
        }
        public static Color GetColorBasedOnAffordability(bool _canAfford)
        {
            return _canAfford ? (Color)HexToRgba(Primary) : (Color)HexToRgba(Error);
        }
    }
}
