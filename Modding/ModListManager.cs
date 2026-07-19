using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TJ
{
    [Serializable]
    public class ModManifest
    {
        public string displayName = "My Mod";
        public string author = "";
        public string version = "1.0.0";
        public string description = "";
        // Empty until first published via SteamWorkshopModSync.PublishOrUpdateModAsync, which
        // fills this in so subsequent publishes update the existing Workshop item instead of
        // creating a duplicate.
        public string workshopFileId = "";
    }

    [Serializable]
    public struct ModListItem
    {
        public string folder;
        public bool enabled;
        public ModManifest manifest;
    }

    // Backing logic for an in-game mod list UI - enumerates every mod folder under Mods/
    // regardless of enabled state (unlike ModLoadOrder.GetEnabledModFolderPathsInOrder, which
    // only returns enabled ones in load order), and persists enable/order state to modlist.json.
    // Changes apply on next restart - ModLoadOrder is only read once, during
    // TabletopTavernData.Awake(), not live-reloaded mid-session.
    public static class ModListManager
    {
        public static List<ModListItem> GetAllMods()
        {
            var result = new List<ModListItem>();
            string modsRoot = ModLoadOrder.ModsRootPath;
            if (!Directory.Exists(modsRoot)) return result;

            var knownOrder = new List<ModListEntry>();
            string modListPath = Path.Combine(modsRoot, ModLoadOrder.ModListFileName);
            if (File.Exists(modListPath))
            {
                try
                {
                    var parsed = JsonUtility.FromJson<ModListFile>(File.ReadAllText(modListPath));
                    if (parsed?.loadOrder != null) knownOrder = parsed.loadOrder;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ModListManager] Failed to parse {modListPath}: {e.Message}");
                }
            }

            var foldersOnDisk = new HashSet<string>(
                Directory.GetDirectories(modsRoot)
                    .Select(dir => Path.GetFileName(dir))
                    .Where(ModLoadOrder.IsRealModFolder)
                    .Where(name => File.Exists(Path.Combine(modsRoot, name, ModLoadOrder.ModManifestFileName))),
                StringComparer.OrdinalIgnoreCase);

            // Known mods first, in their saved order, skipping any that no longer exist on disk.
            foreach (var entry in knownOrder)
            {
                if (!foldersOnDisk.Contains(entry.folder)) continue;
                result.Add(new ModListItem { folder = entry.folder, enabled = entry.enabled, manifest = ReadManifest(modsRoot, entry.folder) });
                foldersOnDisk.Remove(entry.folder);
            }

            // Any mod folders present on disk but not yet in modlist.json - new installs -
            // appended at the end, enabled by default.
            foreach (var folder in foldersOnDisk.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(new ModListItem { folder = folder, enabled = true, manifest = ReadManifest(modsRoot, folder) });
            }

            return result;
        }

        public static void SaveModList(List<ModListItem> mods)
        {
            ModLoadOrder.EnsureModsDirectoryExists();
            var file = new ModListFile();
            foreach (var mod in mods)
            {
                file.loadOrder.Add(new ModListEntry { folder = mod.folder, enabled = mod.enabled });
            }

            string path = Path.Combine(ModLoadOrder.ModsRootPath, ModLoadOrder.ModListFileName);
            File.WriteAllText(path, JsonUtility.ToJson(file, true));
        }

        private static ModManifest ReadManifest(string modsRoot, string folder)
        {
            string path = Path.Combine(modsRoot, folder, ModLoadOrder.ModManifestFileName);
            try
            {
                var manifest = JsonUtility.FromJson<ModManifest>(File.ReadAllText(path));
                return manifest ?? new ModManifest { displayName = folder };
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ModListManager] Failed to parse {path}: {e.Message}");
                return new ModManifest { displayName = folder };
            }
        }
    }
}
