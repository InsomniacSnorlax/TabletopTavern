using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TJ
{
    [Serializable]
    public struct RaceColorOverride
    {
        public string race;
        public Color primaryColor;
        public Color secondaryColor;
        public Color accentColor;
    }

    [Serializable]
    public class RaceOverrideFile
    {
        public List<RaceColorOverride> overrides = new();
    }

    // RaceData only exposes cosmetic fields for override (colors) - RaceBasePrefab/MapRegion are
    // asset references, out of scope for data-only overrides. Each race entry fully specifies all
    // 3 colors (no per-field patching) since JsonUtility can't distinguish "field absent" from
    // "field defaulted" for a value type like Color.
    public static class RaceDataOverrideLoader
    {
        public const string FileName = "race_overrides.json";

        public static void ApplyOverridesFromModFolder(string modFolderPath, Dictionary<Race, RaceData> raceDataDictionary)
        {
            string path = Path.Combine(modFolderPath, FileName);
            string modLabel = ModOverrideValidation.GetModLabel(modFolderPath);

            ModOverrideValidation.TryLoadFile(path,
                () => ApplyJson(File.ReadAllText(path), modLabel, raceDataDictionary),
                $"RaceData ({modLabel})");
        }

        private static void ApplyJson(string json, string modLabel, Dictionary<Race, RaceData> raceDataDictionary)
        {
            var file = JsonUtility.FromJson<RaceOverrideFile>(json);
            if (file?.overrides == null) return;

            int applied = 0;
            foreach (var entry in file.overrides)
            {
                string context = $"RaceData ({modLabel}) entry '{entry.race}'";
                if (!ModOverrideValidation.TryParseEnumOrWarn(entry.race, "race", context, out Race race))
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or unknown race, skipping.");
                    continue;
                }
                if (!raceDataDictionary.TryGetValue(race, out RaceData raceData))
                {
                    Debug.LogWarning($"[ModOverride] {context}: no RaceData loaded for {race}, skipping.");
                    continue;
                }

                raceData.PrimaryColor = entry.primaryColor;
                raceData.SecondaryColor = entry.secondaryColor;
                raceData.AccentColor = entry.accentColor;
                applied++;
            }

            Debug.Log($"[ModOverride] RaceData ({modLabel}): applied color overrides for {applied} race(s).");
        }

        public static string ExportTemplate(Dictionary<Race, RaceData> raceDataDictionary)
        {
            var file = new RaceOverrideFile();
            foreach (var kvp in raceDataDictionary)
            {
                file.overrides.Add(new RaceColorOverride
                {
                    race = kvp.Key.ToString(),
                    primaryColor = kvp.Value.PrimaryColor,
                    secondaryColor = kvp.Value.SecondaryColor,
                    accentColor = kvp.Value.AccentColor
                });
            }
            return JsonUtility.ToJson(file, true);
        }
    }
}
