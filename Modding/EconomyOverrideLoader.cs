using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace TJ
{
    [Serializable]
    public struct UnitTierCostEntry
    {
        public string tier;
        public string cost;
    }

    [Serializable]
    public struct TownRecruitCostEntry
    {
        public string townSize;
        public string cost;
    }

    [Serializable]
    public struct GearRarityCostEntry
    {
        public string gearRarity;
        public string cost;
    }

    [Serializable]
    public struct ConsumableRarityCostEntry
    {
        public string consumableRarity;
        public string cost;
    }

    [Serializable]
    public struct CardPackPriceEntry
    {
        public string packID;
        public string price;
    }

    [Serializable]
    public struct ConsumableDropOddsEntry
    {
        public string actNumber;
        public string consumable;
        public string weight;
    }

    [Serializable]
    public struct TownBountyRangeEntry
    {
        public string townSize;
        public string min;
        public string max;
    }

    [Serializable]
    public struct BankSettingsEntry
    {
        public string maxInterest;
        public string potionRewardsOdds;
    }

    [Serializable]
    public struct BattleRewardsEntry
    {
        public string ransomCaptivesReward;
        public string skirmishReward;
        public string hordeReward;
    }

    [Serializable]
    public class EconomyOverrideFile
    {
        public List<UnitTierCostEntry> unitTierCosts = new();
        public List<TownRecruitCostEntry> townRecruitCosts = new();
        public List<GearRarityCostEntry> gearRarityCosts = new();
        public List<ConsumableRarityCostEntry> consumableRarityCosts = new();
        public List<CardPackPriceEntry> cardPackPrices = new();
        public List<ConsumableDropOddsEntry> consumableDropOdds = new();
        public List<TownBountyRangeEntry> townBountyRanges = new();
        public BankSettingsEntry bank;
        public BattleRewardsEntry battleRewards;
    }

    // Covers shop/economy values with no prior mod coverage: unit/town recruit costs, gear and
    // consumable prices, card pack prices, consumable drop odds, town bounty gold ranges, bank
    // interest settings, and battle/event gold rewards. Every touched class (TabletopTavernConstants,
    // GearData, ConsumableData, CardPackDataInfo, TownSaveData, GoldManager) owns and clears its own
    // override store - this loader only parses JSON and calls their setters, same shape as
    // GearOverrideLoader/ArmyGenerationRuleOverrideLoader.
    public static class EconomyOverrideLoader
    {
        public const string FileName = "economy_overrides.json";

        public static void ClearOverrides()
        {
            TabletopTavernConstants.ClearEconomyOverrides();
            GearData.ClearCostOverrides();
            ConsumableData.ClearEconomyOverrides();
            CardPackDataInfo.ClearEconomyOverrides();
            TownSaveData.ClearEconomyOverrides();
            GoldManager.ClearEconomyOverrides();
        }

        public static void ApplyOverridesFromModFolder(string modFolderPath)
        {
            string path = Path.Combine(modFolderPath, FileName);
            string modLabel = ModOverrideValidation.GetModLabel(modFolderPath);

            ModOverrideValidation.TryLoadFile(path,
                () => ApplyJson(File.ReadAllText(path), modLabel),
                $"Economy ({modLabel})");
        }

        private static void ApplyJson(string json, string modLabel)
        {
            var file = JsonUtility.FromJson<EconomyOverrideFile>(json);
            if (file == null) return;

            int applied = 0;
            applied += ApplyUnitTierCosts(file.unitTierCosts, modLabel);
            applied += ApplyTownRecruitCosts(file.townRecruitCosts, modLabel);
            applied += ApplyGearRarityCosts(file.gearRarityCosts, modLabel);
            applied += ApplyConsumableRarityCosts(file.consumableRarityCosts, modLabel);
            applied += ApplyCardPackPrices(file.cardPackPrices, modLabel);
            applied += ApplyConsumableDropOdds(file.consumableDropOdds, modLabel);
            applied += ApplyTownBountyRanges(file.townBountyRanges, modLabel);
            applied += ApplyBank(file.bank, modLabel);
            applied += ApplyBattleRewards(file.battleRewards, modLabel);

            Debug.Log($"[ModOverride] Economy ({modLabel}): applied {applied} override(s).");
        }

        private static int ApplyUnitTierCosts(List<UnitTierCostEntry> entries, string modLabel)
        {
            if (entries == null) return 0;
            int applied = 0;
            foreach (var entry in entries)
            {
                string context = $"Economy ({modLabel}) unitTierCosts entry [{entry.tier}]";
                if (!ModOverrideValidation.TryParseIntOrWarn(entry.tier, "tier", context, out int tier))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or invalid tier, skipping.");
                    continue;
                }
                if (ModOverrideValidation.TryParseIntOrWarn(entry.cost, "cost", context, out int cost))
                {
                    TabletopTavernConstants.SetUnitTierCostOverride(tier, cost);
                    applied++;
                }
            }
            return applied;
        }

        private static int ApplyTownRecruitCosts(List<TownRecruitCostEntry> entries, string modLabel)
        {
            if (entries == null) return 0;
            int applied = 0;
            foreach (var entry in entries)
            {
                string context = $"Economy ({modLabel}) townRecruitCosts entry [{entry.townSize}]";
                if (!ModOverrideValidation.TryParseEnumOrWarn(entry.townSize, "townSize", context, out TownSize townSize))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or unknown townSize, skipping.");
                    continue;
                }
                if (ModOverrideValidation.TryParseIntOrWarn(entry.cost, "cost", context, out int cost))
                {
                    TownSaveData.SetRecruitCostOverride(townSize, cost);
                    applied++;
                }
            }
            return applied;
        }

        private static int ApplyGearRarityCosts(List<GearRarityCostEntry> entries, string modLabel)
        {
            if (entries == null) return 0;
            int applied = 0;
            foreach (var entry in entries)
            {
                string context = $"Economy ({modLabel}) gearRarityCosts entry [{entry.gearRarity}]";
                if (!ModOverrideValidation.TryParseEnumOrWarn(entry.gearRarity, "gearRarity", context, out GearRarity rarity))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or unknown gearRarity, skipping.");
                    continue;
                }
                if (ModOverrideValidation.TryParseIntOrWarn(entry.cost, "cost", context, out int cost))
                {
                    GearData.SetCostOverride(rarity, cost);
                    applied++;
                }
            }
            return applied;
        }

        private static int ApplyConsumableRarityCosts(List<ConsumableRarityCostEntry> entries, string modLabel)
        {
            if (entries == null) return 0;
            int applied = 0;
            foreach (var entry in entries)
            {
                string context = $"Economy ({modLabel}) consumableRarityCosts entry [{entry.consumableRarity}]";
                if (!ModOverrideValidation.TryParseEnumOrWarn(entry.consumableRarity, "consumableRarity", context, out ConsumableRarity rarity))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or unknown consumableRarity, skipping.");
                    continue;
                }
                if (ModOverrideValidation.TryParseIntOrWarn(entry.cost, "cost", context, out int cost))
                {
                    ConsumableData.SetCostOverride(rarity, cost);
                    applied++;
                }
            }
            return applied;
        }

        private static int ApplyCardPackPrices(List<CardPackPriceEntry> entries, string modLabel)
        {
            if (entries == null) return 0;
            int applied = 0;
            foreach (var entry in entries)
            {
                string context = $"Economy ({modLabel}) cardPackPrices entry [{entry.packID}]";
                if (!ModOverrideValidation.TryParseIntOrWarn(entry.packID, "packID", context, out int packID))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or invalid packID, skipping.");
                    continue;
                }
                if (ModOverrideValidation.TryParseIntOrWarn(entry.price, "price", context, out int price))
                {
                    CardPackDataInfo.SetPriceOverride(packID, price);
                    applied++;
                }
            }
            return applied;
        }

        private static int ApplyConsumableDropOdds(List<ConsumableDropOddsEntry> entries, string modLabel)
        {
            if (entries == null) return 0;
            int applied = 0;
            foreach (var entry in entries)
            {
                string context = $"Economy ({modLabel}) consumableDropOdds entry [act {entry.actNumber}, {entry.consumable}]";
                if (!ModOverrideValidation.TryParseIntOrWarn(entry.actNumber, "actNumber", context, out int actNumber))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or invalid actNumber, skipping.");
                    continue;
                }
                if (!ModOverrideValidation.TryParseEnumOrWarn(entry.consumable, "consumable", context, out ConsumableEnum consumable))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or unknown consumable, skipping.");
                    continue;
                }
                if (ModOverrideValidation.TryParseFloatOrWarn(entry.weight, "weight", context, out float weight))
                {
                    ConsumableData.SetDropChanceOverride(actNumber, consumable, weight);
                    applied++;
                }
            }
            return applied;
        }

        private static int ApplyTownBountyRanges(List<TownBountyRangeEntry> entries, string modLabel)
        {
            if (entries == null) return 0;
            int applied = 0;
            foreach (var entry in entries)
            {
                string context = $"Economy ({modLabel}) townBountyRanges entry [{entry.townSize}]";
                if (!ModOverrideValidation.TryParseEnumOrWarn(entry.townSize, "townSize", context, out TownSize townSize))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or unknown townSize, skipping.");
                    continue;
                }
                if (!ModOverrideValidation.TryParseIntOrWarn(entry.min, "min", context, out int min) ||
                    !ModOverrideValidation.TryParseIntOrWarn(entry.max, "max", context, out int max))
                {
                    Debug.LogWarning($"[ModOverride] {context}: townBountyRanges requires both min and max, skipping.");
                    continue;
                }
                // System.Random.Next(min,max) throws if max < min - that call happens live during
                // campaign play, well after this boot-time load, so a bad pair must be rejected here.
                if (max < min)
                {
                    Debug.LogWarning($"[ModOverride] {context}: max ({max}) is less than min ({min}), skipping.");
                    continue;
                }
                TownSaveData.SetBountyRangeOverride(townSize, min, max);
                applied++;
            }
            return applied;
        }

        private static int ApplyBank(BankSettingsEntry bank, string modLabel)
        {
            int applied = 0;
            string context = $"Economy ({modLabel}) bank";
            if (ModOverrideValidation.TryParseIntOrWarn(bank.maxInterest, "maxInterest", context, out int maxInterest))
            {
                GoldManager.SetMaxInterestOverride(maxInterest);
                applied++;
            }
            if (ModOverrideValidation.TryParseIntOrWarn(bank.potionRewardsOdds, "potionRewardsOdds", context, out int potionRewardsOdds))
            {
                GoldManager.SetPotionRewardsOddsOverride(potionRewardsOdds);
                applied++;
            }
            return applied;
        }

        private static int ApplyBattleRewards(BattleRewardsEntry rewards, string modLabel)
        {
            int applied = 0;
            string context = $"Economy ({modLabel}) battleRewards";
            if (ModOverrideValidation.TryParseIntOrWarn(rewards.ransomCaptivesReward, "ransomCaptivesReward", context, out int ransom))
            {
                TabletopTavernConstants.SetRansomCaptivesRewardOverride(ransom);
                applied++;
            }
            if (ModOverrideValidation.TryParseIntOrWarn(rewards.skirmishReward, "skirmishReward", context, out int skirmish))
            {
                TabletopTavernConstants.SetSkirmishRewardOverride(skirmish);
                applied++;
            }
            if (ModOverrideValidation.TryParseIntOrWarn(rewards.hordeReward, "hordeReward", context, out int horde))
            {
                TabletopTavernConstants.SetHordeRewardOverride(horde);
                applied++;
            }
            return applied;
        }

        // Exports current effective values as a modder's starting point, same convention as the
        // other loaders. No live-instance dependency for any of the 9 sections - GoldManager's
        // section uses its Default... consts since this runs from the Editor, where no live
        // GoldManager/CampaignManager instance reliably exists.
        public static string ExportTemplate()
        {
            var file = new EconomyOverrideFile();

            for (int tier = 1; tier <= 4; tier++)
                file.unitTierCosts.Add(new UnitTierCostEntry { tier = tier.ToString(), cost = TabletopTavernConstants.GetUnitCost(tier).ToString() });

            foreach (TownSize size in Enum.GetValues(typeof(TownSize)))
                file.townRecruitCosts.Add(new TownRecruitCostEntry { townSize = size.ToString(), cost = TownSaveData.GetTownRecruitCost(size).ToString() });

            foreach (GearRarity rarity in Enum.GetValues(typeof(GearRarity)))
                file.gearRarityCosts.Add(new GearRarityCostEntry { gearRarity = rarity.ToString(), cost = GearData.GearCost(rarity).ToString() });

            foreach (ConsumableRarity rarity in Enum.GetValues(typeof(ConsumableRarity)))
                file.consumableRarityCosts.Add(new ConsumableRarityCostEntry { consumableRarity = rarity.ToString(), cost = ConsumableData.ConsumableCost(rarity).ToString() });

            CardPackData[] allPacks = { CardPackDataInfo.GearPack, CardPackDataInfo.CardPack1, CardPackDataInfo.CardPack2, CardPackDataInfo.CardPack3, CardPackDataInfo.CardPack4 };
            foreach (var pack in allPacks)
                file.cardPackPrices.Add(new CardPackPriceEntry { packID = pack.packID.ToString(), price = pack.packPrice.ToString() });

            foreach (ConsumableEnum consumable in ConsumableData.GetAllConsumableEnums())
                for (int act = 1; act <= 3; act++)
                    file.consumableDropOdds.Add(new ConsumableDropOddsEntry
                    {
                        actNumber = act.ToString(),
                        consumable = consumable.ToString(),
                        weight = ConsumableData.ConsumableDropChance(consumable, act).ToString(CultureInfo.InvariantCulture)
                    });

            foreach (TownSize size in Enum.GetValues(typeof(TownSize)))
            {
                var range = TownSaveData.GetEffectiveBountyRange(size);
                file.townBountyRanges.Add(new TownBountyRangeEntry { townSize = size.ToString(), min = range.Min.ToString(), max = range.Max.ToString() });
            }

            file.bank = new BankSettingsEntry
            {
                maxInterest = GoldManager.DefaultMaxInterest.ToString(),
                potionRewardsOdds = GoldManager.DefaultPotionRewardsOdds.ToString(),
            };

            file.battleRewards = new BattleRewardsEntry
            {
                ransomCaptivesReward = TabletopTavernConstants.GetRansomCaptivesReward().ToString(),
                skirmishReward = TabletopTavernConstants.GetSkirmishReward().ToString(),
                hordeReward = TabletopTavernConstants.GetHordeReward().ToString(),
            };

            return JsonUtility.ToJson(file, true);
        }
    }
}
