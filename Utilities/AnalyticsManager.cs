using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Analytics;
using UnityEngine;
using Memori.SaveData;

namespace TabletopAnalytics
{
    public enum RunResult { Win, Loss, Abandoned }
    public class AnalyticsManager : MonoBehaviour 
    {
        public static AnalyticsManager Instance { get; private set; }
        bool _initialized = false;

        async void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // await UnityServices.InitializeAsync();
                // AnalyticsService.Instance.StartDataCollection();
                _initialized = true;
                // Consent (GDPR/etc.) - call once on first launch
                // AnalyticsService.Instance.StartDataCollection(); // If pre-6.1
            } else Destroy(gameObject);
        }
        public void LogRunStart(string runUUID, int heroID, int difficultyLevel, string startingGear, SquadToLoad[] startingArmy)
        {
// #if UNITY_EDITOR
            return;
// #endif
            if (!_initialized) return;

            List<UnitName> startingUnits = new ();
            foreach (SquadToLoad squad in startingArmy)
            {
                if(squad.UnitIndex == -1) continue;
                startingUnits.Add(squad.UnitName);
            }

            CustomEvent myEvent = new ("runStartEvent")
            {
                {"runUUID", runUUID },
                {"heroID", heroID },
                {"difficultyLevel", difficultyLevel },
                {"startingGear", startingGear },
                {"startingUnits", string.Join(",", startingUnits) }
            };

            Debug.Log($"Logging Run Start Event: {myEvent.ToString()}");

            AnalyticsService.Instance.RecordEvent(myEvent);
            AnalyticsService.Instance.Flush();
        }
        public void LogRunEnd(string runUUID, RunResult runResult, int turnNumber, int goldCount, int enemiesSlain)
        {
// #if UNITY_EDITOR
            return;
// #endif
            if (!_initialized) return;

            CustomEvent myEvent = new ("runEndEvent")
            {
                {"runUUID", runUUID },
                {"runResult", runResult.ToString() },
                {"turnNumber", turnNumber },
                {"goldCount", goldCount },
                {"enemiesSlain", enemiesSlain }
            };

            Debug.Log($"Logging Run End Event: {myEvent.ToString()}");

            AnalyticsService.Instance.RecordEvent(myEvent);
            AnalyticsService.Instance.Flush();
        }
    }
}