using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;


namespace TJ
{
    public class EventLocalizationUpdater : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private bool wipeTable = false;
        public string tableName = "EventTable";

        // English locale ID
        private const string englishLocaleId = "en";

        [ContextMenu("Update Localization Table")]
        private async void UpdateLocalizationTable()
        {
            TT_Event[] eventItems = EventData.GetAllEvents();

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
                Debug.LogError("Failed to find String Table!");
                return;
            }
            else
            {
                stringTable = tableOperation.Result as StringTable;
            }

            if (wipeTable) stringTable.Clear();

            //clean up the table and remove any empty entries
            Debug.Log($"tablelength: {stringTable.Count}");

            List<long> keysToRemove = new List<long>();
            foreach (var entry in stringTable)
            {
                // Debug.Log($"Checking entry '{entry.Value}'");
                if (entry.Value == null || string.IsNullOrEmpty(entry.Value.ToString()))
                {
                    Debug.Log($" Removing empty entry '{entry.Key}'");
                    keysToRemove.Add(entry.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                stringTable.RemoveEntry(key);
            }

            // Update the table with event data
            foreach (var eventItem in eventItems)
            {
                string baseKey = eventItem.EventName.Replace(" ", ""); // e.g., "ThePlaguedVillage"

                // Add event name and description
                UpdateOrAddEntry(stringTable, $"{baseKey}Name", eventItem.EventName);
                UpdateOrAddEntry(stringTable, $"{baseKey}Desc", eventItem.EventDescription);

                // Add choices and their outcomes
                for (int i = 0; i < eventItem.EventChoices.Length; i++)
                {
                    var choice = eventItem.EventChoices[i];
                    string choiceKey = $"{baseKey}{i}"; // e.g., "ThePlaguedVillage0"

                    UpdateOrAddEntry(stringTable, $"{choiceKey}Title", choice.eventChoiceTitle);
                    UpdateOrAddEntry(stringTable, $"{choiceKey}Desc", choice.eventChoiceDescription);
                    UpdateOrAddEntry(stringTable, $"{choiceKey}SuccessOutcomeDesc", choice.successOutcome.OutcomeDescription);
                    UpdateOrAddEntry(stringTable, $"{choiceKey}FailureOutcomeDesc", choice.failureOutcome.OutcomeDescription);
                    UpdateOrAddEntry(stringTable, $"{choiceKey}CriticalFailureOutcomeDesc", choice.criticalFailureOutcome.OutcomeDescription);
                    UpdateOrAddEntry(stringTable, $"{choiceKey}CriticalSuccessOutcomeDesc", choice.criticalSuccessOutcome.OutcomeDescription);
                }
            }

            // Mark the table as dirty to ensure changes are saved
            UnityEditor.EditorUtility.SetDirty(stringTable);
            Debug.Log($"tablelength: {stringTable.Count}");
            Debug.Log($"Updated String Table '{tableName}' with {eventItems.Length} events.");
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
