using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TJ
{
public class GearLocalizationUpdater : MonoBehaviour
{
#if UNITY_EDITOR
    public string tableName = "MainLocalizationTable";
    private const string englishLocaleId = "en";


    [ContextMenu("Update Localization Table")]

    private async void UpdateLocalizationTable()
    {
        Gear[] gearItems = GearData.GetAllGear();
        // Get the English locale
        Locale englishLocale = LocalizationSettings.AvailableLocales.GetLocale(englishLocaleId);
        if (englishLocale == null)
        {
            Debug.LogError($"Locale '{englishLocaleId}' not found!");
            return;
        }

        // Load or create the String Table
        var tableOperation = LocalizationSettings.StringDatabase.GetTableAsync(tableName, englishLocale);
        await tableOperation.Task;

        StringTable stringTable;
        if (tableOperation.Result == null)
        {
            Debug.LogError("Failed to find String Table");
            return;
        }
        else
        {
            stringTable = tableOperation.Result as StringTable;
        }

        // Update the table with gear data
        foreach (var gear in gearItems)
        {
            string nameKey = $"{gear.GearName.Replace(" ", "")}Name"; // e.g., "ArmingSwordsName"
            string descKey = $"{gear.GearName.Replace(" ", "")}Desc";  // e.g., "ArmingSwordsDesc"
            string flavorKey = $"{gear.GearName.Replace(" ", "")}Flavor"; // e.g., "ArmingSwordsFlavor"

            // Update or add entries
            // UpdateOrAddEntry(stringTable, nameKey, gear.GearName);
            // UpdateOrAddEntry(stringTable, descKey, gear.GearDescription);
            // UpdateOrAddEntry(stringTable, flavorKey, gear.GearFlavorText);
        }

        // Mark the table as dirty to ensure changes are saved
        UnityEditor.EditorUtility.SetDirty(stringTable);
        Debug.Log($"Updated String Table '{tableName}' with {gearItems.Length} gear items.");
    }

    private void UpdateOrAddEntry(StringTable table, string key, string value)
    {
        var entry = table.GetEntry(key);
        if (entry != null)
        {
            // Entry exists, overwrite it
            entry.Value = value;
            Debug.Log($"Overwrote entry '{key}' with value '{value}'");
        }
        else
        {
            // Entry doesn’t exist, create it
            table.AddEntry(key, value);
            Debug.Log($"Added new entry '{key}' with value '{value}'");
        }
    }
#endif
}
}