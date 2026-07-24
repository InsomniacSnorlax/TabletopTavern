#if UNITY_EDITOR
using System.Collections.Generic;
using System.Threading.Tasks;
using Memori.SaveData;
using UnityEditor;
using UnityEngine;

namespace TJ
{
    public class CustomBattleGeneratorEditor : EditorWindow
    {
        // Shared parameters
        private int _boardNumber = 1;
        private int _battlesFought = 0;
        private bool _finalBattle = false;
        private bool _knightDifficulty = false;
        private bool _autoRandomizeSeed = false;

        // Per-side parameters
        private Race _playerRace = Race.IronLegion;
        private int _playerSeed = 11111;
        private Race _enemyRace = Race.Gruntkin;
        private int _enemySeed = 22222;

        private struct GeneratedUnit
        {
            public UnitName unitName;
            public int tier;
            public UnitType unitType;
            public UnitSize unitSize;
            public SquadStats stats;
        }

        private List<GeneratedUnit> _playerResults = new();
        private List<GeneratedUnit> _enemyResults = new();
        private Vector2 _scrollPos;
        private string _errorMessage = "";
        private string _statusMessage = "";

        [MenuItem("TabletopTavern/Custom Battle Generator")]
        public static void ShowWindow()
        {
            GetWindow<CustomBattleGeneratorEditor>("Custom Battle Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Army Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Shared parameters
            GUILayout.Label("Battle Parameters", EditorStyles.miniBoldLabel);
            _boardNumber = EditorGUILayout.IntField("Board Number", _boardNumber); // no hardcoded cap - mods can add rules for boards beyond the base game's 3
            _battlesFought = EditorGUILayout.IntField("Battles Fought", _battlesFought);
            _finalBattle = EditorGUILayout.Toggle("Final Battle (Horde)", _finalBattle);
            _knightDifficulty = EditorGUILayout.Toggle("Knight Difficulty", _knightDifficulty);
            _autoRandomizeSeed = EditorGUILayout.Toggle("Auto-randomize Seeds", _autoRandomizeSeed);

            EditorGUILayout.Space();

            // Player side
            GUILayout.Label("Player", EditorStyles.miniBoldLabel);
            _playerRace = (Race)EditorGUILayout.EnumPopup("Race", _playerRace);
            EditorGUILayout.BeginHorizontal();
            _playerSeed = EditorGUILayout.IntField("Seed", _playerSeed);
            if (GUILayout.Button("Randomize", GUILayout.Width(80)))
                _playerSeed = Random.Range(0, int.MaxValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Enemy side
            GUILayout.Label("Enemy", EditorStyles.miniBoldLabel);
            _enemyRace = (Race)EditorGUILayout.EnumPopup("Race", _enemyRace);
            EditorGUILayout.BeginHorizontal();
            _enemySeed = EditorGUILayout.IntField("Seed", _enemySeed);
            if (GUILayout.Button("Randomize", GUILayout.Width(80)))
                _enemySeed = Random.Range(0, int.MaxValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Both Armies"))
            {
                _statusMessage = "";
                if (_autoRandomizeSeed)
                {
                    _playerSeed = Random.Range(0, int.MaxValue);
                    _enemySeed = Random.Range(0, int.MaxValue);
                }
                GenerateArmy(_playerRace, _playerSeed, ref _playerResults);
                GenerateArmy(_enemyRace, _enemySeed, ref _enemyResults);
            }

            if (!string.IsNullOrEmpty(_errorMessage))
            {
                EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);
                return;
            }

            bool hasResults = _playerResults.Count > 0 || _enemyResults.Count > 0;
            if (!hasResults)
                return;

            EditorGUILayout.Space();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MaxHeight(400));

            if (_playerResults.Count > 0)
            {
                GUILayout.Label($"Player ({_playerResults.Count} squads):", EditorStyles.boldLabel);
                DrawArmyList(_playerResults);
                DrawTierSummary(_playerResults);
                EditorGUILayout.Space();
            }

            if (_enemyResults.Count > 0)
            {
                GUILayout.Label($"Enemy ({_enemyResults.Count} squads):", EditorStyles.boldLabel);
                DrawArmyList(_enemyResults);
                DrawTierSummary(_enemyResults);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("Load into Custom Battle"))
                LoadIntoBattle();

            if (!string.IsNullOrEmpty(_statusMessage))
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
        }

        private static void DrawArmyList(List<GeneratedUnit> results)
        {
            for (int i = 0; i < results.Count; i++)
            {
                var u = results[i];
                EditorGUILayout.LabelField(
                    $"{i + 1,2}. {u.unitName,-32}  T{u.tier}  {u.unitType,-8}  {u.unitSize}");
            }
        }

        private static void DrawTierSummary(List<GeneratedUnit> results)
        {
            Dictionary<int, int> counts = new();
            foreach (var u in results)
            {
                if (!counts.ContainsKey(u.tier)) counts[u.tier] = 0;
                counts[u.tier]++;
            }
            GUILayout.Label("Tier breakdown:", EditorStyles.miniLabel);
            foreach (var kv in counts)
                GUILayout.Label($"  Tier {kv.Key}: {kv.Value} squad(s)", EditorStyles.miniLabel);
        }

        private void LoadIntoBattle()
        {
            if (_playerResults.Count == 0 && _enemyResults.Count == 0)
            {
                _statusMessage = "Generate armies first.";
                return;
            }

            CustomBattleSaveData data = SaveDataHandler.LoadCustomBattleSaveData();

            if (_playerResults.Count > 0)
                data.playerCustomBattleArmy = BuildSquadArray(_playerResults, isPlayer: true);

            if (_enemyResults.Count > 0)
                data.enemyCustomBattleArmy = BuildSquadArray(_enemyResults, isPlayer: false);

            SaveDataHandler.SaveCustomBattleSaveData(data);

            if (Application.isPlaying && BattleManager.InstanceIfExists != null)
            {
                _ = ReloadArmiesAsync();
            }
            else
            {
                _statusMessage = $"Saved {_playerResults.Count} player + {_enemyResults.Count} enemy squads. Enter play mode to see them.";
            }
        }

        private static SquadToLoad[] BuildSquadArray(List<GeneratedUnit> results, bool isPlayer)
        {
            var squads = new SquadToLoad[results.Count];
            for (int i = 0; i < results.Count; i++)
            {
                SquadStats s = results[i].stats;
                int maxCount = s.baseUnitCount;
                int hp = s.HitPointsPerUnit;
                squads[i] = new SquadToLoad
                {
                    UnitName = results[i].unitName,
                    UniqueID = System.Guid.NewGuid().ToString(),
                    UnitPrestige = 0,
                    // BattleSaveManager filters player squads by UnitIndex != -1 && < 10
                    UnitIndex = isPlayer ? i : -1,
                    isEmptySquad = false,
                    maxUnitCount = maxCount,
                    HitPointsPerUnit = hp,
                    SquadMaxHealth = maxCount * hp,
                    SquadCurrentHealth = maxCount * hp,
                };
            }
            return squads;
        }

        private async Task ReloadArmiesAsync()
        {
            await BattleManager.Instance.ArmySpawnManager.ClearBothArmies();
            await BattleManager.Instance.ArmySpawnManager.LoadBothArmies();
            _statusMessage = $"Reloaded {_playerResults.Count} player + {_enemyResults.Count} enemy squads into the running battle.";
            Repaint();
        }

        private void GenerateArmy(Race race, int seed, ref List<GeneratedUnit> results)
        {
            _errorMessage = "";
            results.Clear();

            SquadData[] allData = Resources.LoadAll<SquadData>("SquadData");
            if (allData.Length == 0)
            {
                _errorMessage = "No SquadData found in Resources/SquadData.";
                return;
            }

            var statsByName = new Dictionary<UnitName, SquadStats>();
            var pool = new List<UnitTier>();
            foreach (SquadData so in allData)
            {
                if (so.assets.race != race) continue;
                statsByName[so.stats.unitName] = so.stats;
                pool.Add(new UnitTier
                {
                    unitName = so.stats.unitName,
                    tier = (int)so.stats.RarityTier + 1
                });
            }

            if (pool.Count == 0)
            {
                _errorMessage = $"No units found for race {race}.";
                return;
            }

            // Reads the same rule table ArmyCreator.GenerateEnemyArmy uses at runtime (including
            // any mod overrides currently loaded), so this preview can never drift from real
            // gameplay the way the old hand-mirrored switch could.
            TierCount[] tierCounts = ArmyGenerationRuleData.ResolveEnemyArmyTierCounts(_boardNumber, _finalBattle, _knightDifficulty, _battlesFought);
            if (tierCounts.Length == 0)
            {
                _errorMessage = $"No army-generation rule matches board={_boardNumber}, finalBattle={_finalBattle}, knightDifficulty={_knightDifficulty}, battlesFought={_battlesFought}.";
                return;
            }
            var spec = new List<(int tier, int count)>();
            foreach (TierCount tc in tierCounts)
                spec.Add((tc.Tier, tc.Count));

            // Pick units — mirrors ArmyCreator.CreateArmyFromUnitsByTier + GetUnitOfTier
            var random = new System.Random(seed);
            foreach (var (tier, count) in spec)
            {
                for (int i = 0; i < count; i++)
                {
                    int randomIndex = random.Next(0, pool.Count);
                    UnitName picked = PickUnitOfTier(tier, randomIndex, pool);
                    SquadStats stats = statsByName.TryGetValue(picked, out var s) ? s : default;
                    results.Add(new GeneratedUnit
                    {
                        unitName = picked,
                        tier = tier,
                        unitType = stats.unitType,
                        unitSize = stats.unitSize,
                        stats = stats
                    });
                }
            }
        }

        // Mirrors ArmyCreator.GetUnitOfTier exactly
        private static UnitName PickUnitOfTier(int tier, int randomIndex, List<UnitTier> pool)
        {
            var random = new System.Random(randomIndex);
            var shuffled = new List<UnitTier>(pool);
            for (int i = 0; i < shuffled.Count; i++)
            {
                int j = random.Next(i, shuffled.Count);
                (shuffled[j], shuffled[i]) = (shuffled[i], shuffled[j]);
            }
            foreach (UnitTier u in shuffled)
            {
                if (u.tier == tier)
                    return u.unitName;
            }
            Debug.LogError($"CustomBattleGenerator: no unit found for tier {tier}");
            return default;
        }
    }
}
#endif
