using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TJ
{
    [Serializable]
    public struct TierCountEntry
    {
        public string tier;
        public string count;
    }

    [Serializable]
    public struct EnemyArmyRuleEntry
    {
        public string board;
        public string finalBattle;
        public string knightDifficulty;
        public string battlesFoughtMin;
        public string battlesFoughtMax;
        public List<TierCountEntry> tierCounts;
    }

    [Serializable]
    public struct TownGarrisonRuleEntry
    {
        public string townSize;
        public string bookNumber;
        public string difficultyImperator;
        public List<TierCountEntry> tierCounts;
    }

    [Serializable]
    public struct EnemyPrestigeRuleEntry
    {
        public string actNumber;
        public string enhanced;
        public string chancePerSquad;
        public string maxPrestigedSquads;
        public string prestigeTwoChance;
    }

    [Serializable]
    public class ArmyGenerationRuleFile
    {
        public List<EnemyArmyRuleEntry> enemyArmyRules = new();
        public List<TownGarrisonRuleEntry> townGarrisonRules = new();
        public List<EnemyPrestigeRuleEntry> enemyPrestigeRules = new();
    }

    // Unlike the sparse field-patch files (unit/race/gear overrides), rules here are matched, not
    // patched: ArmyGenerationRuleData.PrependModRules puts a mod's rules ahead of the built-in
    // defaults, and the first rule whose specified fields all match wins - see the schema notes in
    // ArmyGenerationRuleData for why (the source data is a branching decision tree, not a flat table).
    public static class ArmyGenerationRuleOverrideLoader
    {
        public const string FileName = "army_generation_rules.json";

        public static void ApplyOverridesFromModFolder(string modFolderPath)
        {
            string path = Path.Combine(modFolderPath, FileName);
            string modLabel = ModOverrideValidation.GetModLabel(modFolderPath);

            ModOverrideValidation.TryLoadFile(path,
                () => ApplyJson(File.ReadAllText(path), modLabel),
                $"ArmyGeneration ({modLabel})");
        }

        private static void ApplyJson(string json, string modLabel)
        {
            var file = JsonUtility.FromJson<ArmyGenerationRuleFile>(json);
            if (file == null) return;

            List<EnemyArmyRule> enemyArmyRules = new();
            for (int i = 0; i < file.enemyArmyRules.Count; i++)
            {
                EnemyArmyRuleEntry entry = file.enemyArmyRules[i];
                string context = $"ArmyGeneration ({modLabel}) enemyArmyRules[{i}]";

                if (!ModOverrideValidation.TryParseIntOrWarn(entry.board, "board", context, out int board))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or invalid required field 'board', skipping.");
                    continue;
                }
                if (!ModOverrideValidation.TryParseBoolOrWarn(entry.finalBattle, "finalBattle", context, out bool finalBattle))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or invalid required field 'finalBattle', skipping.");
                    continue;
                }

                EnemyArmyRule rule = new() { Board = board, FinalBattle = finalBattle, TierCounts = ParseTierCounts(entry.tierCounts, context) };
                if (ModOverrideValidation.TryParseBoolOrWarn(entry.knightDifficulty, "knightDifficulty", context, out bool knightDifficulty)) rule.KnightDifficulty = knightDifficulty;
                if (ModOverrideValidation.TryParseIntOrWarn(entry.battlesFoughtMin, "battlesFoughtMin", context, out int battlesFoughtMin)) rule.BattlesFoughtMin = battlesFoughtMin;
                if (ModOverrideValidation.TryParseIntOrWarn(entry.battlesFoughtMax, "battlesFoughtMax", context, out int battlesFoughtMax)) rule.BattlesFoughtMax = battlesFoughtMax;
                enemyArmyRules.Add(rule);
            }
            if (enemyArmyRules.Count > 0) ArmyGenerationRuleData.PrependModRules(enemyArmyRules);

            List<TownGarrisonRule> townGarrisonRules = new();
            for (int i = 0; i < file.townGarrisonRules.Count; i++)
            {
                TownGarrisonRuleEntry entry = file.townGarrisonRules[i];
                string context = $"ArmyGeneration ({modLabel}) townGarrisonRules[{i}]";

                if (!ModOverrideValidation.TryParseEnumOrWarn(entry.townSize, "townSize", context, out TownSize townSize))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or unknown townSize, skipping.");
                    continue;
                }
                if (!ModOverrideValidation.TryParseIntOrWarn(entry.bookNumber, "bookNumber", context, out int bookNumber))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or invalid required field 'bookNumber', skipping.");
                    continue;
                }

                TownGarrisonRule rule = new() { TownSize = townSize, BookNumber = bookNumber, TierCounts = ParseTierCounts(entry.tierCounts, context) };
                if (ModOverrideValidation.TryParseBoolOrWarn(entry.difficultyImperator, "difficultyImperator", context, out bool difficultyImperator)) rule.DifficultyImperator = difficultyImperator;
                townGarrisonRules.Add(rule);
            }
            if (townGarrisonRules.Count > 0) ArmyGenerationRuleData.PrependModRules(townGarrisonRules);

            List<EnemyPrestigeRule> prestigeRules = new();
            for (int i = 0; i < file.enemyPrestigeRules.Count; i++)
            {
                EnemyPrestigeRuleEntry entry = file.enemyPrestigeRules[i];
                string context = $"ArmyGeneration ({modLabel}) enemyPrestigeRules[{i}]";

                if (!ModOverrideValidation.TryParseIntOrWarn(entry.actNumber, "actNumber", context, out int actNumber))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or invalid required field 'actNumber', skipping.");
                    continue;
                }
                if (!ModOverrideValidation.TryParseBoolOrWarn(entry.enhanced, "enhanced", context, out bool enhanced))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or invalid required field 'enhanced', skipping.");
                    continue;
                }

                EnemyPrestigeRule rule = new() { ActNumber = actNumber, Enhanced = enhanced };
                ModOverrideValidation.TryParseFloatOrWarn(entry.chancePerSquad, "chancePerSquad", context, out rule.ChancePerSquad);
                ModOverrideValidation.TryParseIntOrWarn(entry.maxPrestigedSquads, "maxPrestigedSquads", context, out rule.MaxPrestigedSquads);
                ModOverrideValidation.TryParseFloatOrWarn(entry.prestigeTwoChance, "prestigeTwoChance", context, out rule.PrestigeTwoChance);
                prestigeRules.Add(rule);
            }
            if (prestigeRules.Count > 0) ArmyGenerationRuleData.PrependModRules(prestigeRules);

            Debug.Log($"[ModOverride] ArmyGeneration ({modLabel}): applied {enemyArmyRules.Count} enemy army rule(s), {townGarrisonRules.Count} town garrison rule(s), {prestigeRules.Count} prestige rule(s).");
        }

        private static TierCount[] ParseTierCounts(List<TierCountEntry> entries, string context)
        {
            if (entries == null) return Array.Empty<TierCount>();
            List<TierCount> result = new();
            foreach (TierCountEntry e in entries)
            {
                if (ModOverrideValidation.TryParseIntOrWarn(e.tier, "tierCounts.tier", context, out int tier) &&
                    ModOverrideValidation.TryParseIntOrWarn(e.count, "tierCounts.count", context, out int count))
                {
                    result.Add(new TierCount { Tier = tier, Count = count });
                }
            }
            return result.ToArray();
        }

        // Exports the full built-in default tables - a complete, correct starting point a modder
        // trims down to just the specific situations they want to change.
        public static string ExportTemplate()
        {
            var file = new ArmyGenerationRuleFile();

            foreach (EnemyArmyRule rule in ArmyGenerationRuleData.DefaultEnemyArmyRules)
            {
                file.enemyArmyRules.Add(new EnemyArmyRuleEntry
                {
                    board = rule.Board.ToString(),
                    finalBattle = rule.FinalBattle.ToString(),
                    knightDifficulty = rule.KnightDifficulty?.ToString() ?? "",
                    battlesFoughtMin = rule.BattlesFoughtMin?.ToString() ?? "",
                    battlesFoughtMax = rule.BattlesFoughtMax?.ToString() ?? "",
                    tierCounts = ToEntries(rule.TierCounts),
                });
            }
            foreach (TownGarrisonRule rule in ArmyGenerationRuleData.DefaultTownGarrisonRules)
            {
                file.townGarrisonRules.Add(new TownGarrisonRuleEntry
                {
                    townSize = rule.TownSize.ToString(),
                    bookNumber = rule.BookNumber.ToString(),
                    difficultyImperator = rule.DifficultyImperator?.ToString() ?? "",
                    tierCounts = ToEntries(rule.TierCounts),
                });
            }
            foreach (EnemyPrestigeRule rule in ArmyGenerationRuleData.DefaultEnemyPrestigeRules)
            {
                file.enemyPrestigeRules.Add(new EnemyPrestigeRuleEntry
                {
                    actNumber = rule.ActNumber.ToString(),
                    enhanced = rule.Enhanced.ToString(),
                    chancePerSquad = rule.ChancePerSquad.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    maxPrestigedSquads = rule.MaxPrestigedSquads.ToString(),
                    prestigeTwoChance = rule.PrestigeTwoChance.ToString(System.Globalization.CultureInfo.InvariantCulture),
                });
            }

            return JsonUtility.ToJson(file, true);
        }

        private static List<TierCountEntry> ToEntries(TierCount[] tierCounts)
        {
            List<TierCountEntry> result = new();
            foreach (TierCount tc in tierCounts)
                result.Add(new TierCountEntry { tier = tc.Tier.ToString(), count = tc.Count.ToString() });
            return result;
        }
    }
}
