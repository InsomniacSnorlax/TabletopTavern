using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TJ
{
    [Serializable]
    public struct GearModifierOverrideEntry
    {
        public string gearID;
        public string gearModifierValue;
    }

    [Serializable]
    public class GearOverrideFile
    {
        public List<GearModifierOverrideEntry> overrides = new();
    }

    // GearData has no per-mod-load dictionary to clear on its own (unlike SquadStats/RaceData,
    // which reload fresh from Resources SOs each time) - ClearModifierOverrides must be called
    // once before this loops over mod folders, see TabletopTavernData.ApplyModOverrides.
    public static class GearOverrideLoader
    {
        public const string FileName = "gear_overrides.json";

        public static void ApplyOverridesFromModFolder(string modFolderPath)
        {
            string path = Path.Combine(modFolderPath, FileName);
            string modLabel = ModOverrideValidation.GetModLabel(modFolderPath);

            ModOverrideValidation.TryLoadFile(path,
                () => ApplyJson(File.ReadAllText(path), modLabel),
                $"Gear ({modLabel})");
        }

        private static void ApplyJson(string json, string modLabel)
        {
            var file = JsonUtility.FromJson<GearOverrideFile>(json);
            if (file?.overrides == null) return;

            int applied = 0;
            foreach (var entry in file.overrides)
            {
                string context = $"Gear ({modLabel}) entry '{entry.gearID}'";
                if (!ModOverrideValidation.TryParseEnumOrWarn(entry.gearID, "gearID", context, out GearID gearID) || gearID == GearID.None)
                {
                    Debug.LogWarning($"[ModOverride] {context}: missing or unknown gearID, skipping.");
                    continue;
                }
                if (ModOverrideValidation.TryParseIntOrWarn(entry.gearModifierValue, "gearModifierValue", context, out int modifierValue))
                {
                    GearData.SetModifierOverride(gearID, modifierValue);
                    applied++;
                }
            }

            Debug.Log($"[ModOverride] Gear ({modLabel}): applied overrides for {applied} gear item(s).");
        }

        // Exports every gear item fully populated - a complete, correct starting point a modder
        // trims down to just the entries they actually want to change.
        public static string ExportTemplate()
        {
            var file = new GearOverrideFile();
            foreach (GearID gearID in GearData.GetGearIDs())
            {
                file.overrides.Add(new GearModifierOverrideEntry
                {
                    gearID = gearID.ToString(),
                    gearModifierValue = GearData.GetGear(gearID).GearModifierValue.ToString(),
                });
            }
            return JsonUtility.ToJson(file, true);
        }
    }
}
