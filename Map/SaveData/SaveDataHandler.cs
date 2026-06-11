using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using TJ;
using System;
using Memori.Steamworks;
using Memori.Metaprogression;

namespace Memori.SaveData
{
    [Serializable] public class CampaignSaveData
    {
        public int seed;
        public int activeMapLayer;
        public int bookNumber;
        public int goldAmount;
        public List<int> nodePath;
        public List<GearID> Gear;
        public List<ConsumableEnum> consumables = new ();
        public int heroID;
        public SquadToLoad[] playerArmy = new SquadToLoad[13];
        public List<SquadBattlePosition> playerSquadBattlePositions = new();
        public List<SavedSquadGroup> playerSquadGroups = new();
        public SquadToLoad[] enemyArmy;
        [SerializeField] private int selectedNodeIndex = -1;
        public TownSaveData townData;
        public UnitName[] recruitableUnits;
        public GearID[] recruitableGear;
        public bool nodeGenerated;
        public bool nodesRevealed;
        public bool battleCompleted;
        public bool playerWonBattle;
        public List<SquadKillsStored> SquadKillsStore;
        public List<SquadKillsStored> HistoricalKillStore;
        public List<UnitNameOverrides> unitNameOverrides;
        public RunStats RunStats;
        public BattleFieldPreset battleFieldPreset;
        public int turnsSincePotato;
        public int Rolls;
        public List<SquadToLoad> withdrawnSquads;
        public List<int> eventOrdering;
        public int BattlesFought;
        public TT_Difficulty difficultyLevel;
        public bool snapShot;
        public bool blank;
        public int signatureUnitPacksPurchased;
        public int townsSacked;
        public bool archerRecruited;
        public Guid runUUID;

        public CampaignSaveData(int _seed, int _hero, int _startingGold, SquadToLoad[] _playerArmy, TT_Difficulty _difficulty, GearID _startingGear, Guid _runUUID)
        {
            runUUID = _runUUID;
            seed = _seed;
            heroID = _hero;
            goldAmount = _startingGold;
            playerArmy = _playerArmy;
            activeMapLayer = -1;
            nodeGenerated = false;
            battleCompleted = false;
            nodePath = new List<int>();
            if(_startingGear == GearID.None) {
                Gear = new List<GearID>();
            } else {
                Gear = new List<GearID>() {
                    _startingGear
                };
            }
            SquadKillsStore = new List<SquadKillsStored>();
            HistoricalKillStore = new List<SquadKillsStored>();
            unitNameOverrides = new List<UnitNameOverrides>();
            RunStats = new RunStats();
            withdrawnSquads = new List<SquadToLoad>();
            difficultyLevel = _difficulty;
            selectedNodeIndex = -1;
            bookNumber = 1;
            eventOrdering = EventData.GetEventOrdering(new System.Random(seed));//order in which events will be generated
            playerSquadBattlePositions = new List<SquadBattlePosition>();
        }
        public int GetSelectedNodeIndex()
        {
            // UnityEngine.Debug.Log($"getting selected node index: {selectedNodeIndex}");
            return selectedNodeIndex;
        }
        public void SetSelectedNodeIndex(int _index)
        {
            // UnityEngine.Debug.Log($"setting selected node index: {_index}");
            selectedNodeIndex = _index;
        }
    }
    [Serializable] public class CustomBattleSaveData
    {
        public SquadToLoad[] playerCustomBattleArmy; 
        public List<SquadBattlePosition> playerCustomBattleSquadBattlePositions = new();
        public List<SavedSquadGroup> playerCustomBattleSquadGroups = new();
        public SquadToLoad[] enemyCustomBattleArmy;
        public List<SquadBattlePosition> enemyCustomBattleSquadBattlePositions = new();
    }
    [System.Serializable] public struct RunStats
    {
        public int chaptersCompleted;
        public int goldEarned;
        public int goldDeposited;
        public int unitsPrestiged;
        public int unitsRecruited;
        public int gearAquired;
        public int enemiesSlain;
    }
    [System.Serializable] public struct UnitNameOverrides
    {
        public string unitGUID;
        public string unitNameOverride;
        public UnitNameOverrides(string _unitGUID, string _unitNameOverride)
        {
            unitGUID = _unitGUID;
            unitNameOverride = _unitNameOverride;
        }
    }
    [System.Serializable] public class PlayerSaveData
    {
        public int campaignsStarted;
        public int campaignsCompleted;
        public List<int> tutorialStepCompleted = new ();
        public bool customBattle;

        //last campaign stats
        public int lastHeroID;
        public GearID lastStartingGearId;
        public TT_Difficulty lastDifficultyLevelSelected;
        public SquadToLoad[] lastArmySaveData;
        public int lastStartingGold;
        #region Collection
        public List<int> gearIdsCollected = new();
        public List<int> gearIdsAcknowledged= new ();
        public List<UnitName> troopsRecruited = new ();
        public List<UnitName> troopsAcknowledged = new ();
        public List<int> consumablesAquired = new ();
        public List<int> consumablesAcknowledged = new ();
        public List<int> metaprogressionNodesUnlocked = new ();
        public List<string> BattlefieldInfoSectionsViewed = new ();
        public int goldToDeposit;
        public int depositedGold;
        #endregion

        public int gameCompletions;
        public List<UnlockCondition> unlockConditionsCompleted = new (){
            UnlockCondition.None
        };
        public List<HeroDifficultiesCompleted> HeroDifficultiesCompleted = new ();
        public int MaxDifficultyOverall;
        public List<HeroLastDifficulty> HeroLastDifficulties = new ();
        public List<Race> unlockedTavernThemes = new ();
        public bool hasTavernThemeSelected = false;
        public Race activeTavernThemeRace;
        public bool isDevToolUser;
    }
    [System.Serializable] public struct SquadKillsStored
    {
        public string SquadGUID;
        public int Kills;
    }
    [System.Serializable] public struct HeroDifficultiesCompleted
    {
        public int HeroID;
        public List<int> DifficultiesCompleted;
    }
    [System.Serializable] public struct HeroLastDifficulty
    {
        public int HeroID;
        public TT_Difficulty LastDifficulty;
    }
    public static class SaveDataHandler
    {
        static readonly bool useLocal = false;

        public static bool CheckForGear(GearID _gearID)
        {
            return Load().Gear.Contains(_gearID);
        }

        public static GearIDsSerialized GetGearCollected()
        {
            List<GearID> gearIDs = Load().Gear;
            GearIDsSerialized gearIDsSerialized = new GearIDsSerialized();
            for(int i = 0; i < gearIDs.Count; i++) {
                switch(i) {
                    case 0:
                        gearIDsSerialized.gearID1 = gearIDs[i];
                        break;
                    case 1:
                        gearIDsSerialized.gearID2 = gearIDs[i];
                        break;
                    case 2:
                        gearIDsSerialized.gearID3 = gearIDs[i];
                        break;
                    case 3:
                        gearIDsSerialized.gearID4 = gearIDs[i];
                        break;
                    case 4:
                        gearIDsSerialized.gearID5 = gearIDs[i];
                        break;
                }
            }
            return gearIDsSerialized;
        }
        private static void SaveToJSON<T> (T toSave, string filename)
        {
            string content = JsonUtility.ToJson(toSave, true);  // 'true' for pretty-printing (optional, human-readable)
            string targetPath = GetPath(filename);
            string tempPath = targetPath + ".tmp";  // Temporary file in same directory

            File.WriteAllText(tempPath, content);

            // Atomically replace/move
            if (File.Exists(targetPath))
            {
                try
                {
                    File.Replace(tempPath, targetPath, null);
                }
                catch (IOException)
                {
                    // File.Replace failed (file temporarily locked), fall back to direct overwrite
                    File.Copy(tempPath, targetPath, overwrite: true);
                    File.Delete(tempPath);
                }
            }
            else
            {
                File.Move(tempPath, targetPath);
            }

            // Optional: Verify (for extra safety)
            if (!File.Exists(targetPath))
            {
                UnityEngine.Debug.LogError($"Atomic save failed for {filename}: Target file missing after replace.");
            }
        }
        public static T ReadListFromJSON<T> (string filename)
        {
            string path = GetPath(filename);
            string content = ReadFile(path);

            if (string.IsNullOrWhiteSpace(content))
            {
                // UnityEngine.Debug.LogWarning($"[ReadListFromJSON] Empty or invalid JSON at path: {path}");
                return default;
            }

            try
            {
                return JsonUtility.FromJson<T>(content);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ReadListFromJSON] Failed to parse JSON from file: {path}\n{e}");
                return default;
            }
        }
        private static string GetPath (string filename)
        {
            return useLocal ? Application.dataPath +"/Data/" +filename : Application.persistentDataPath + "/" + filename;
        }
        private static void WriteFile (string path, string content)
        {
            FileStream fileStream = new (path, FileMode.Create);
            using StreamWriter writer = new (fileStream);
            writer.Write(content);
        }
        private static string ReadFile (string path)
        {
            if (File.Exists (path))
            {
                using StreamReader reader = new StreamReader(path);
                string content = reader.ReadToEnd();
                return content;
            }
            return "";
        }
        public static void SaveCampaign(CampaignSaveData toSave)
        {
            toSave.snapShot = false;
            SaveToJSON(toSave, "campaignSaveData.json");
        }
        public static void SaveCampaignSnapshot(CampaignSaveData toSave)
        {
            toSave.snapShot = true;
            SaveToJSON(toSave, "campaignSaveDataSnapshot.json");
        }
        public static void SaveCustomBattleSaveData(CustomBattleSaveData toSave)
        {
            SaveToJSON(toSave, "customBattleSaveData.json");
        }
        public static List<SquadKillsStored> AddToHistoricalKills(List<SquadKillsStored> _historicalKills, List<SquadKillsStored> _currentKills)
        {
            for(int i = 0; i < _currentKills.Count; i++) {
                bool found = false;
                for(int j = 0; j < _historicalKills.Count; j++) {
                    if(_currentKills[i].SquadGUID == _historicalKills[j].SquadGUID) {
                        _historicalKills[j] = new SquadKillsStored() {
                            SquadGUID = _currentKills[i].SquadGUID,
                            Kills = _historicalKills[j].Kills + _currentKills[i].Kills
                        };
                        found = true;
                        break;
                    }
                }
                if(!found) {
                    _historicalKills.Add(_currentKills[i]);
                }
            }
            return _historicalKills;
        }
        /// <summary>
        /// Saves the squads after a manual battle has been completed.
        /// </summary>
        /// <param name="_playerSquads"></param>
        /// <param name="_enemySquads"></param>
        /// <param name="_playerWon"></param>
        /// <param name="_squadIdKillCounter"></param>
        public static void SaveSquadsPostBattle(SquadToLoad[] _playerSquads, SquadToLoad[] _enemySquads, bool _playerWon, List<SquadKillsStored> _squadIdKillCounter)
        {
            UnityEngine.Debug.Log($"SaveDataHandler SaveSquadsPostBattle: player won: {_playerWon}");
            CampaignSaveData saveData = Load();
            SquadToLoad GetPlayerSquad(string _uniqueID)
            {
                for (int i = 0; i < _playerSquads.Length; i++)
                {
                    if (_playerSquads[i].UniqueID == _uniqueID) return _playerSquads[i];
                }
                UnityEngine.Debug.LogError($"Could not find player squad with uniqueID {_uniqueID}");
                return new SquadToLoad();
            }

            for (int i = 0; i < 10; i++)
            {
                if (saveData.playerArmy[i].UnitIndex == -1) continue;

                saveData.playerArmy[i] = GetPlayerSquad(saveData.playerArmy[i].UniqueID);
            }
            saveData.enemyArmy = _enemySquads;
            saveData.battleCompleted = true;
            saveData.playerWonBattle = _playerWon;
            saveData.SquadKillsStore = _squadIdKillCounter;

            saveData.HistoricalKillStore = AddToHistoricalKills(saveData.HistoricalKillStore, _squadIdKillCounter);
            if(saveData.townData == null) {
                saveData.townData = new TownSaveData();
                UnityEngine.Debug.Log($"Created new TownSaveData in SaveSquadsPostBattle");
            }
            saveData.townData.townInteractionStatus = TownInteractionStatus.Sacked;
            // saveData.withdrawnSquads = _withdrawnSquads;

            int totalKills = 0;
            foreach (var squadKill in _squadIdKillCounter)
            {
                totalKills += squadKill.Kills;
            }
            UnityEngine.Debug.Log($"Total enemies slain in battle: {totalKills}");
            saveData.RunStats.enemiesSlain += totalKills;

            //achievement check - cav only
            bool cavOnly = true;
            for (int i = 0; i < 10; i++)
            {
                if (saveData.playerArmy[i].UnitIndex == -1) continue;

                //get unit size for each unit
                UnitSize unitSize = TabletopTavernData.Instance.GetSquadStats(saveData.playerArmy[i].UnitName).unitSize;
                if (unitSize != UnitSize.Cavalry)
                {
                    cavOnly = false;
                    break;
                }
            }
            if (cavOnly)
            {
                SteamStatic.UnlockAchievement(SteamData.ACHIEVEMENT_ONLY_CAV_BATTLE);
            }

            SaveCampaign(saveData);

            //update the last snapshot to overwrite the snapshot of the pre battle state since the battle is now completed
            SaveCampaignSnapshot(saveData);
        }
        public static void SavePlayerSaveData(PlayerSaveData toSave)
        {
            SaveToJSON(toSave, "playerSaveData.json");
        }
        public static bool CampaignSaveExists()
        {
            if (!File.Exists(GetPath("campaignSaveData.json"))) return false;
            CampaignSaveData save = ReadListFromJSON<CampaignSaveData>("campaignSaveData.json");
            return save != null && !save.blank;
        }
        public static void DeleteCampaignSave()
        {
            var blankSave = new CampaignSaveData(0, 0, 0, null, TT_Difficulty.Peasant, GearID.None, Guid.Empty) { blank = true };
            SaveToJSON(blankSave, "campaignSaveData.json");
            blankSave.snapShot = true;
            SaveToJSON(blankSave, "campaignSaveDataSnapshot.json");
        }
        public static bool PlayerSaveDataExists()
        {
            return File.Exists(GetPath("playerSaveData.json"));
        }
        public static void DeletePlayerSaveData()
        {
            string path = GetPath("playerSaveData.json");
            if (File.Exists(path))
                File.Delete(path);
        }
        public static CampaignSaveData Load()
        {
            CampaignSaveData loadedSaveData = ReadListFromJSON<CampaignSaveData>("campaignSaveData.json");
            if (loadedSaveData == null || loadedSaveData.blank)
            {
                // UnityEngine.Debug.LogError($"No campaign save data found, creating new default save data.");
                loadedSaveData = new CampaignSaveData(UnityEngine.Random.Range(0, 100000), HeroData.EdricValeward.HeroID, 0, null, TT_Difficulty.Peasant, GearID.None, Guid.NewGuid());
            }

            return loadedSaveData;
        }
        public static CustomBattleSaveData LoadCustomBattleSaveData()
        {
            CustomBattleSaveData loadedSaveData = ReadListFromJSON<CustomBattleSaveData>("customBattleSaveData.json");
            if (loadedSaveData == null)
            {
                // UnityEngine.Debug.LogError($"No custom battle save data found, creating new default save data.");
                loadedSaveData ??= new CustomBattleSaveData();
            }

            return loadedSaveData;
        }
        public static CampaignSaveData LoadSnapshot()
        {
            // UnityEngine.Debug.Log($"Loading campaign snapshot save data.");
            CampaignSaveData loadedSaveData = ReadListFromJSON<CampaignSaveData>("campaignSaveDataSnapshot.json");
            if (loadedSaveData == null || loadedSaveData.blank) {
                UnityEngine.Debug.Log($"Campaign snapshot save data is blank or does not exist, creating new default save data.");
                loadedSaveData = new CampaignSaveData( UnityEngine.Random.Range(0, 100000), HeroData.EdricValeward.HeroID, 0, null, TT_Difficulty.Peasant, GearID.None, Guid.NewGuid());
            }

            return loadedSaveData;
        }
        public static CampaignSaveData LoadSnapshotNullAllowed()
        {
            CampaignSaveData loadedSaveData = ReadListFromJSON<CampaignSaveData>("campaignSaveDataSnapshot.json");
            if (loadedSaveData != null && loadedSaveData.blank) return null;
            return loadedSaveData;
        }
        public static PlayerSaveData LoadPlayerSaveData()
        {
            PlayerSaveData loadedSaveData = ReadListFromJSON<PlayerSaveData>("playerSaveData.json");
            loadedSaveData ??= new PlayerSaveData();
            
            return loadedSaveData;
        }
        public static List<int> GetHeroDifficultiesCompleted(int _heroID)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            for(int i = 0; i < saveData.HeroDifficultiesCompleted.Count; i++) {
                if(saveData.HeroDifficultiesCompleted[i].HeroID == _heroID) {
                    return saveData.HeroDifficultiesCompleted[i].DifficultiesCompleted;
                }
            }
            return new List<int>();
        }
        public static TT_Difficulty GetHeroLastDifficulty(int heroID)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            int maxAvailable = Math.Min(saveData.MaxDifficultyOverall + 1, (int)TT_Difficulty.Godking);
            if (maxAvailable < 1) maxAvailable = 1;

            for (int i = 0; i < saveData.HeroLastDifficulties.Count; i++)
            {
                if (saveData.HeroLastDifficulties[i].HeroID == heroID)
                {
                    int lastWin = (int)saveData.HeroLastDifficulties[i].LastDifficulty;
                    if (lastWin < saveData.MaxDifficultyOverall)
                        return (TT_Difficulty)lastWin;
                    break;
                }
            }

            return (TT_Difficulty)maxAvailable;
        }
        public static void SaveHeroLastDifficulty(int heroID, TT_Difficulty difficulty)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            for (int i = 0; i < saveData.HeroLastDifficulties.Count; i++)
            {
                if (saveData.HeroLastDifficulties[i].HeroID == heroID)
                {
                    saveData.HeroLastDifficulties[i] = new HeroLastDifficulty { HeroID = heroID, LastDifficulty = difficulty };
                    SavePlayerSaveData(saveData);
                    return;
                }
            }
            saveData.HeroLastDifficulties.Add(new HeroLastDifficulty { HeroID = heroID, LastDifficulty = difficulty });
            SavePlayerSaveData(saveData);
        }
        public static bool IsTavernThemeUnlocked(Race _race)
        {
            if (_race == Race.Special) return true;
            PlayerSaveData saveData = LoadPlayerSaveData();
            return saveData.unlockedTavernThemes.Contains(_race);
        }
        public static void UnlockTavernTheme(Race _race)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            if (!saveData.unlockedTavernThemes.Contains(_race))
            {
                saveData.unlockedTavernThemes.Add(_race);
                SavePlayerSaveData(saveData);
            }
        }
        public static void SetActiveTavernTheme(Race _race)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            saveData.hasTavernThemeSelected = true;
            saveData.activeTavernThemeRace = _race;
            SavePlayerSaveData(saveData);
        }
        public static void ClearActiveTavernTheme()
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            saveData.hasTavernThemeSelected = false;
            SavePlayerSaveData(saveData);
        }
        public static bool TryGetActiveTavernTheme(out Race _race)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            _race = saveData.activeTavernThemeRace;
            return saveData.hasTavernThemeSelected;
        }
        // Returns true if any hero of the given race has completed Godking difficulty
        public static bool HasCompletedGodkingWithRace(Race _race)
        {
            List<Hero> heroes = HeroData.GetHeroesByRace(_race);
            foreach (Hero hero in heroes)
            {
                List<int> difficulties = GetHeroDifficultiesCompleted(hero.HeroID);
                if (difficulties.Contains((int)TT_Difficulty.Godking))
                    return true;
            }
            return false;
        }
        public static bool IsCustomBattle()
        {
            return LoadPlayerSaveData().customBattle;
        }
        public static bool IsDevToolUser()
        {
            return LoadPlayerSaveData().isDevToolUser;
        }
        public static void SaveLastCampaignStats(int _heroID, TT_Difficulty _difficultyLevelSelected, GearID _gearID, SquadToLoad[] _armySaveData, int _startingGold)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();

            saveData.lastHeroID = _heroID;
            saveData.lastDifficultyLevelSelected = _difficultyLevelSelected;
            saveData.lastStartingGearId = _gearID;
            saveData.lastArmySaveData = _armySaveData;
            saveData.lastStartingGold = _startingGold;
            SavePlayerSaveData(saveData);
        }
        public static int GetActiveHeroID()
        {
            return Load().heroID;
        }
        public static Race GetEnemyRace()
        {
            return Load().enemyArmy != null && Load().enemyArmy.Length > 0 ? TabletopTavernData.Instance.GetRaceFromUnitName(Load().enemyArmy[0].UnitName) : Race.Special;
        }

        public static void OpenSaveFolder()
        {
            string path = Application.persistentDataPath;
            if (Directory.Exists(path)) {
                // string formattedPath = $"\"{path}\"";
                string formattedPath = path.Replace("/", "\\");
                formattedPath = $"\"{formattedPath}\"";
                // UnityEngine.Debug.Log($"Opening folder: {formattedPath}");
                Process.Start("explorer.exe", formattedPath);
            } else {
                UnityEngine.Debug.LogWarning($"Directory not found: {path}");
            }
        }
        public static void CreateCampaign(Hero hero, ArmySaveData armySaveData, TT_Difficulty _difficultyLevelSelected, GearID _startingGear, Guid _runUUID, int startingGold)
        {
            SquadToLoad[] squadsToLoad = new SquadToLoad[armySaveData.SquadsInArmy.Length];
            for(int i = 0; i < armySaveData.SquadsInArmy.Length; i++) {
                squadsToLoad[i] = new SquadToLoad(armySaveData.SquadsInArmy[i], 0, i);
            }
            CreateCampaign(hero, squadsToLoad, _difficultyLevelSelected, _startingGear, _runUUID, startingGold);
        }
        public static void CreateCampaign(Hero hero, SquadToLoad[] squadsToLoad, TT_Difficulty _difficultyLevelSelected, GearID _startingGear, Guid _runUUID, int startingGold)
        {
            // UnityEngine.Debug.Log($"Creating campaign with hero: {hero.HeroID} and difficulty: {_difficultyLevelSelected}");
            SquadToLoad[] playerArmy = new SquadToLoad[13];
            for(int i = 0; i < playerArmy.Length; i++) {
                playerArmy[i].UnitIndex = -1;
            }
            float startingHealth = 1f;
            bool archerRecruited = false;

            //DifficultyMod 18
            if(_difficultyLevelSelected >= TT_Difficulty.Overlord) startingHealth = 0.75f;

            for(int i = 0; i < squadsToLoad.Length; i++) {
                playerArmy[i] = new SquadToLoad(
                    squadsToLoad[i].UnitName, 
                    _prestige: 0, 
                    _unitIndex: i, 
                    _modifiedHealthValueByAmount : startingHealth
                );

                //int get base unit count
                int baseUnitCount = TabletopTavernData.Instance.GetBaseUnitCount(playerArmy[i].UnitName);
                int maxUnitCount = TabletopTavernData.Instance.GetHitPointsPerUnit(playerArmy[i].UnitName);
                playerArmy[i].SquadCurrentHealth = (int)((baseUnitCount * maxUnitCount) * startingHealth);
                playerArmy[i].maxUnitCount = baseUnitCount;
                playerArmy[i].HitPointsPerUnit = maxUnitCount;
                AquiredTroop(playerArmy[i].UnitName);

                SquadStats squadStats = TabletopTavernData.Instance.GetSquadStats(playerArmy[i].UnitName);

                if(squadStats.unitType == UnitType.Ranged) {
                    archerRecruited = true;
                }
            }

            int seed = UnityEngine.Random.Range(0, 1000000);
            CampaignSaveData campaignSaveData = new (seed, hero.HeroID, startingGold, playerArmy, _difficultyLevelSelected, _startingGear, _runUUID);
            
            if(archerRecruited) {
                campaignSaveData.archerRecruited = true;
            }
            SaveCampaign(campaignSaveData);
            SaveCampaignSnapshot(campaignSaveData);
            SaveLastCampaignStats(hero.HeroID, _difficultyLevelSelected, _startingGear, squadsToLoad, startingGold);
        }
        public static List<int> GetGearIDsCollected()
        {
            return LoadPlayerSaveData().gearIdsCollected;
        }
        public static void AquiredGear(GearID _gearID)
        {
            if (_gearID == GearID.None) return;
            PlayerSaveData saveData = LoadPlayerSaveData();
            if(saveData.gearIdsCollected.Contains(0)) {
                saveData.gearIdsCollected.Remove(0);
            }
            if(saveData.gearIdsCollected.Contains((int)_gearID)) return;
            saveData.gearIdsCollected.Add((int)_gearID);
            SavePlayerSaveData(saveData);
        }
        public static List<int> GetGearIDsAcknowledged()
        {
            return LoadPlayerSaveData().gearIdsAcknowledged;
        }
        public static void AcknowledgedGear(GearID _gearID)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            if(saveData.gearIdsAcknowledged.Contains(0)) {
                saveData.gearIdsAcknowledged.Remove(0);
                SavePlayerSaveData(saveData);
            }
            if(saveData.gearIdsAcknowledged.Contains((int)_gearID)) return;
            saveData.gearIdsAcknowledged.Add((int)_gearID);
            SavePlayerSaveData(saveData);
        }
        public static List<UnitName> GetTroopsIDsCollected()
        {
            List<UnitName> troopsRecruitied = LoadPlayerSaveData().troopsRecruited;

            return troopsRecruitied;
        }
        public static void AquiredTroop(UnitName _unitName)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            if(saveData.troopsRecruited.Contains(_unitName)) return;
            saveData.troopsRecruited.Add(_unitName);
            SavePlayerSaveData(saveData);
        }
        public static List<UnitName> GetTroopsIDsAcknowledged()
        {
            return LoadPlayerSaveData().troopsAcknowledged;
        }
        public static void AcknowledgedTroop(UnitName _unitName)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            if(saveData.troopsAcknowledged.Contains(_unitName)) return;
            saveData.troopsAcknowledged.Add(_unitName);
            SavePlayerSaveData(saveData);
        }
        public static List<int> GetPotionsIDsCollected()
        {
            return LoadPlayerSaveData().consumablesAquired;
        }
        public static void AquiredPotionForCollection(ConsumableEnum _consumable)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            if(saveData.consumablesAquired.Contains((int)_consumable)) return;
            saveData.consumablesAquired.Add((int)_consumable);
            SavePlayerSaveData(saveData);
        }
        public static List<int> GetPotionsIDsAcknowledged()
        {
            return LoadPlayerSaveData().consumablesAcknowledged;
        }
        public static void AcknowledgedPotion(ConsumableEnum _consumable)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            if(saveData.consumablesAcknowledged.Contains((int)_consumable)) return;
            saveData.consumablesAcknowledged.Add((int)_consumable);
            SavePlayerSaveData(saveData);
        }
        public static void RecordGameOver(bool _playerWon)
        {
            UnityEngine.Debug.Log($"Recording game over, player won: {_playerWon}");
            if(!_playerWon) return;

            PlayerSaveData saveData = LoadPlayerSaveData();
            saveData.gameCompletions++;
            
            int currentHeroID = CampaignManager.Instance.CampaignSaveManager.SaveData.heroID;
            int newDifficulty = (int)CampaignManager.Instance.CampaignSaveManager.SaveData.difficultyLevel;

            //update max difficulty unlocked if needed
            if(newDifficulty > saveData.MaxDifficultyOverall) {
                saveData.MaxDifficultyOverall = newDifficulty;
            }

            bool found = false;
            for (int i = 0; i < saveData.HeroDifficultiesCompleted.Count; i++)
            {
                //hero has an entry
                if (saveData.HeroDifficultiesCompleted[i].HeroID == currentHeroID)
                {
                    // Only update if the new difficulty was not already recorded
                    if (!saveData.HeroDifficultiesCompleted[i].DifficultiesCompleted.Contains(newDifficulty))
                    {
                        saveData.HeroDifficultiesCompleted[i].DifficultiesCompleted.Add(newDifficulty);
                    }
                    found = true;
                    break;
                }
            }

            // Only add if no existing entry was found
            if (!found)
            {
                saveData.HeroDifficultiesCompleted.Add(new HeroDifficultiesCompleted()
                {
                    HeroID = currentHeroID,
                    DifficultiesCompleted = new List<int>() { newDifficulty }
                });
            }

            bool heroLastDiffFound = false;
            for (int i = 0; i < saveData.HeroLastDifficulties.Count; i++)
            {
                if (saveData.HeroLastDifficulties[i].HeroID == currentHeroID)
                {
                    saveData.HeroLastDifficulties[i] = new HeroLastDifficulty { HeroID = currentHeroID, LastDifficulty = (TT_Difficulty)newDifficulty };
                    heroLastDiffFound = true;
                    break;
                }
            }
            if (!heroLastDiffFound)
                saveData.HeroLastDifficulties.Add(new HeroLastDifficulty { HeroID = currentHeroID, LastDifficulty = (TT_Difficulty)newDifficulty });

            saveData.goldToDeposit += CampaignManager.Instance.CampaignSaveManager.SaveData.goldAmount;
            SavePlayerSaveData(saveData);
            DepositGold();
        }
        public static bool IsUnlockConditionUnlocked(UnlockCondition _unlockCondition, int heroID)
        {
            if (_unlockCondition == UnlockCondition.NotAvailableInDemo)
            {
                return false;
            }

            if (_unlockCondition == UnlockCondition.HeroCompletion && heroID > 0)
            {
                return GetHeroDifficultiesCompleted(heroID - 1).Count > 0;
            }

            return LoadPlayerSaveData().unlockConditionsCompleted.Contains(_unlockCondition);
        }
        public static async Task<GameObject> GetPlayerHeroPrefabAsync()
        {
            int heroID = CampaignSaveExists() ? Load().heroID : HeroData.EdricValeward.HeroID;
            return await TabletopTavernData.Instance.LoadHeroPrefabAsync(heroID);
        }
        public static void DepositGold()
        {
            PlayerSaveData playerSaveData = LoadPlayerSaveData();
            // UnityEngine.Debug.Log($"Depositing gold: {playerSaveData.goldToDeposit}");
            playerSaveData.depositedGold += playerSaveData.goldToDeposit;
            //cap at 500
#if DEMO
            if(playerSaveData.depositedGold > TabletopTavernConstants.MAX_DEMO_DEPOSITED_GOLD) {
                playerSaveData.depositedGold = TabletopTavernConstants.MAX_DEMO_DEPOSITED_GOLD;
            }
#endif
            playerSaveData.goldToDeposit = 0;

            if(playerSaveData.depositedGold >= 100) {
                SteamStatic.UnlockAchievement(SteamData.ACHIEVEMENT_A_SNACK_FOR_LATER);
            }
            SavePlayerSaveData(playerSaveData);
        }
        public static int GetDepositedGold()
        {
            return LoadPlayerSaveData().depositedGold;
        }
        public static void UnlockMetaprogressionNode(MetaprogressionModel _node)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            if(saveData.metaprogressionNodesUnlocked.Contains(_node.NodeId)) return;
            saveData.metaprogressionNodesUnlocked.Add(_node.NodeId);
            SavePlayerSaveData(saveData);
        }
        public static List<int> GetUnlockedMetaprogressionNodes()
        {
            return LoadPlayerSaveData().metaprogressionNodesUnlocked;
        }
        public static void ResetMetaprogression()
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            saveData.metaprogressionNodesUnlocked = new List<int>();
            SavePlayerSaveData(saveData);
        }
        public static bool IsMetaprogressionNodeUnlocked(MetaprogressionModel _node)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            bool conditionUnlocked = saveData.metaprogressionNodesUnlocked.Contains(_node.NodeId);
            // UnityEngine.Debug.Log($"IsMetaprogressionNodeUnlocked {_node.NodeName}: {conditionUnlocked}");
            return conditionUnlocked;
        }
        #if UNITY_EDITOR
        //record hero completions for testing
        public static void RecordHeroCompletionForTesting(int _heroID, TT_Difficulty _difficulty)
        {
            PlayerSaveData saveData = LoadPlayerSaveData();
            int newDifficulty = (int)_difficulty;

            //update max difficulty unlocked if needed
            if(newDifficulty > saveData.MaxDifficultyOverall) {
                saveData.MaxDifficultyOverall = newDifficulty;
            }

            bool found = false;
            for (int i = 0; i < saveData.HeroDifficultiesCompleted.Count; i++)
            {
                //hero has an entry
                if (saveData.HeroDifficultiesCompleted[i].HeroID == _heroID)
                {
                    // Only update if the new difficulty was not already recorded
                    if (!saveData.HeroDifficultiesCompleted[i].DifficultiesCompleted.Contains(newDifficulty))
                    {
                        saveData.HeroDifficultiesCompleted[i].DifficultiesCompleted.Add(newDifficulty);
                    }
                    found = true;
                    break;
                }
            }

            // Only add if no existing entry was found
            if (!found)
            {
                saveData.HeroDifficultiesCompleted.Add(new HeroDifficultiesCompleted()
                {
                    HeroID = _heroID,
                    DifficultiesCompleted = new List<int>() { newDifficulty }
                });
            }
            SavePlayerSaveData(saveData);
        }
        #endif
    }
}
