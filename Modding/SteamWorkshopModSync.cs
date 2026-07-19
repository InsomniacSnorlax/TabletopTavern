using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Memori.Steamworks;
using Steamworks.Data;
using UgcItem = Steamworks.Ugc.Item;
using UgcPublishResult = Steamworks.Ugc.PublishResult;

namespace TJ
{
    // Bridges the generic Memori.Steamworks.SteamWorkshop wrapper to this game's mod-folder
    // convention. Lives in the main assembly (not Memori.Steamworks, which is a separate assembly
    // with no knowledge of Mods/ folders or mod.json) so it can reference both SteamWorkshop and
    // ModLoadOrder/ModManifest directly.
    public static class SteamWorkshopModSync
    {
        public const string WorkshopFolderPrefix = "workshop_";

        // True for mod folders that were synced down from a Workshop subscription rather than
        // authored locally - these can't be published (they aren't this player's content, and
        // their mod.json has no workshopFileId of its own).
        public static bool IsWorkshopSyncedFolder(string folderName) =>
            !string.IsNullOrEmpty(folderName) && folderName.StartsWith(WorkshopFolderPrefix);

        // Mirrors every subscribed+installed Workshop item's content into Mods/workshop_<id>/, so
        // the existing local-folder mod loader (ModLoadOrder/ModListManager) picks them up with no
        // changes on its end. Steam Workshop querying is inherently async, but
        // TabletopTavernData.Awake()'s mod loading is synchronous - so this must be awaited to
        // completion during a boot/loading step *before* TabletopTavernData.Instance is first
        // touched, not called from within Awake() itself.
        public static async Task SyncSubscribedItemsToModsFolderAsync()
        {
            List<UgcItem> items = await SteamWorkshop.GetSubscribedItemsAsync();
            if (items.Count == 0) return;

            ModLoadOrder.EnsureModsDirectoryExists();

            foreach (UgcItem item in items)
            {
                if (!item.IsInstalled || string.IsNullOrEmpty(item.Directory))
                {
                    Debug.LogWarning($"[SteamWorkshopModSync] Subscribed item {item.Id} ('{item.Title}') isn't installed yet, skipping this sync pass.");
                    continue;
                }

                string targetFolder = Path.Combine(ModLoadOrder.ModsRootPath, WorkshopFolderPrefix + item.Id.Value);
                try
                {
                    CopyModContent(item.Directory, targetFolder);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SteamWorkshopModSync] Failed to sync item {item.Id} ('{item.Title}') into {targetFolder}: {e.Message}");
                }
            }

            Debug.Log($"[SteamWorkshopModSync] Synced {items.Count} subscribed Workshop item(s) into {ModLoadOrder.ModsRootPath}.");
        }

        private static void CopyModContent(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                string destPath = Path.Combine(targetDir, Path.GetFileName(filePath));
                File.Copy(filePath, destPath, overwrite: true);
            }
        }

        // Publishes modFolderPath as a new Workshop item, or updates the existing one if mod.json
        // already has a workshopFileId from a prior publish - either way, mod.json ends up with
        // the correct id afterward. Returns true on success.
        public static async Task<bool> PublishOrUpdateModAsync(string modFolderPath, IProgress<float> progress = null)
        {
            string manifestPath = Path.Combine(modFolderPath, ModLoadOrder.ModManifestFileName);
            if (!File.Exists(manifestPath))
            {
                Debug.LogError($"[SteamWorkshopModSync] No {ModLoadOrder.ModManifestFileName} found in {modFolderPath}, cannot publish.");
                return false;
            }

            ModManifest manifest = JsonUtility.FromJson<ModManifest>(File.ReadAllText(manifestPath));
            if (manifest == null)
            {
                Debug.LogError($"[SteamWorkshopModSync] Failed to parse {manifestPath}, cannot publish.");
                return false;
            }

            string previewImagePath = Path.Combine(modFolderPath, "preview.png");
            if (!File.Exists(previewImagePath)) previewImagePath = null;

            // existingId must be used inside the same condition that assigns it - the compiler's
            // definite-assignment analysis for `out` variables doesn't trace through a separate
            // bool checked later, even if that bool was computed from this exact TryParse call.
            UgcPublishResult result;
            bool isNewPublish;
            if (ulong.TryParse(manifest.workshopFileId, out ulong existingId))
            {
                isNewPublish = false;
                result = await SteamWorkshop.UpdateItemAsync((PublishedFileId)existingId, modFolderPath, manifest.displayName, manifest.description, previewImagePath: previewImagePath, progress: progress);
            }
            else
            {
                isNewPublish = true;
                result = await SteamWorkshop.PublishNewItemAsync(modFolderPath, manifest.displayName, manifest.description, previewImagePath: previewImagePath, tags: new[] { "Mod" }, progress: progress);
            }

            if (!result.Success) return false;

            if (isNewPublish)
            {
                manifest.workshopFileId = result.FileId.Value.ToString();
                File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));
            }

            return true;
        }
    }
}
