using UnityEngine;
using TMPro;
using Memori.SaveData;
using System.Collections.Generic;
using Memori.Utilities;
using Memori.Localization;
using Memori.Tooltip;
using Memori.Audio;
using Memori.Core;
using Memori.UI;
using UnityEngine.Playables;
using Memori.Notifications;
using Memori.Metaprogression;
using System;

namespace TJ.MainMenu
{
    public class StartingArmyManager : MonoBehaviour
    {
        [Header("Selected Army")]
        [SerializeField] private SquadBattleInfo squadBattleInfo;
        [SerializeField] private Transform startingUnitsParent;
        [SerializeField] private SquadToLoad[] _squadsToLoad;
        public SquadToLoad[] SelectedArmy => _squadsToLoad;
        List<SquadDisplayCardMenu> squadDisplayCards = new();
        [SerializeField] private SquadDisplayCardMenu squadDisplayCardMenu;
        
        [Header("Select Starting Army")]
        [SerializeField] private GameObject lockedBlocker;
        [SerializeField] private TMP_Text tier1CostText;
        [SerializeField] private Transform tier1UnitsParent;
        [SerializeField] private TMP_Text tier2CostText;
        [SerializeField] private Transform tier2UnitsParent;
        [SerializeField] private TMP_Text tier3CostText;
        [SerializeField] private Transform tier3UnitsParent;

        [Header("Gear")]
        [SerializeField] private Transform gearCardParent;
        [SerializeField] private CollectionGearCard[] gearCards;
        [SerializeField] private GearCard selectedGearCard;
        [SerializeField] private MemoriTooltipTrigger gearCardOptionTooltipTrigger;
        [SerializeField] private StartingGearDoubleClickHandler doubleClickHandler;
        ArmySaveData armySaveData;
        public ArmySaveData ArmySaveData => armySaveData;

        [Header("Treasury")]
        [SerializeField] private TMP_Text remainingTreasuryText;
        [SerializeField] private TMP_Text gearCostTextRare, gearCostTextUncommon, gearCostTextCommon;

        [Header("Metaprogression")]
        [SerializeField] private MetaprogressionModel _startingGoldMetaprogressionModel;
        [SerializeField] private MetaprogressionModel _startingGoldMetaprogressionModel2;
        [SerializeField] private MetaprogressionModel _startingGearReducedCostMetaprogressionModel;
        [SerializeField] private MetaprogressionModel _thirdReserveSlotMetaprogressionModel;

        GearID startingGearID;
        PlayPanel playPanel;
        public MonitoredData<int> remainingTreasury = new (0);
        List<UnitName> troopsRecruitied = new ();
        int startingGold;
        public Action<int> OnStartingArmyLengthChanged;

        public void SetUp(PlayPanel _playPanel)
        {
            playPanel = _playPanel;
            remainingTreasury.OnValueChanged += UpdateRemainingTreasury;
            remainingTreasury.OnValueChanged += playPanel.RemainingTreasuryChanged;
            startingGold = playPanel.hero.StartingGold;
            
            if(SaveDataHandler.IsMetaprogressionNodeUnlocked(_startingGoldMetaprogressionModel)) {
                startingGold += _startingGoldMetaprogressionModel.NodeValue;
                // Debug.Log($"Increased starting gold to: {startingGold}");
            }
            if(SaveDataHandler.IsMetaprogressionNodeUnlocked(_startingGoldMetaprogressionModel2)) {
                startingGold += _startingGoldMetaprogressionModel2.NodeValue;
                // Debug.Log($"Increased starting gold to: {startingGold}");
            }
            remainingTreasury.Value = startingGold;

            LoadStartingGear();
            LoadStartingArmy();
            RefreshArmyDisplay();
            tier1CostText.text = LocalizationManager.Instance.GetText("Tier I") + " - " + TabletopTavernConstants.GetUnitCost(1).ToString() + " <sprite name=GoldSprite>";
            tier2CostText.text = LocalizationManager.Instance.GetText("Tier II") + " - " + TabletopTavernConstants.GetUnitCost(2).ToString() + " <sprite name=GoldSprite>";
            tier3CostText.text = LocalizationManager.Instance.GetText("Tier III") + " - " + TabletopTavernConstants.GetUnitCost(3).ToString() + " <sprite name=GoldSprite>";

            int commonCost = GearData.GearCost(GearRarity.Common);
            int uncommonCost = GearData.GearCost(GearRarity.Uncommon);
            int rareCost = GearData.GearCost(GearRarity.Rare);

            if(SaveDataHandler.IsMetaprogressionNodeUnlocked(_startingGearReducedCostMetaprogressionModel)) {
                commonCost = Mathf.Max(0, commonCost - _startingGearReducedCostMetaprogressionModel.NodeValue);
                uncommonCost = Mathf.Max(0, uncommonCost - _startingGearReducedCostMetaprogressionModel.NodeValue);
                rareCost = Mathf.Max(0, rareCost - _startingGearReducedCostMetaprogressionModel.NodeValue);
                // Debug.Log($"Reduced starting gear costs to: Common {commonCost}, Uncommon {uncommonCost}, Rare {rareCost}");
            }

            gearCostTextCommon.text = LocalizationManager.Instance.GetText("Common") + "\n" + commonCost.ToString() + " <sprite name=GoldSprite>";
            gearCostTextUncommon.text = LocalizationManager.Instance.GetText("Uncommon") + "\n" + uncommonCost.ToString() + " <sprite name=GoldSprite>";
            gearCostTextRare.text = LocalizationManager.Instance.GetText("Rare") + "\n" + rareCost.ToString() + " <sprite name=GoldSprite>";

            troopsRecruitied = SaveDataHandler.LoadPlayerSaveData().troopsRecruited;
            OnStartingArmyLengthChanged?.Invoke(_squadsToLoad.Length);
        }
        private void AddUnitToArmy(UnitName unitName)
        {
            List<SquadToLoad> updatedSquads = new List<SquadToLoad>(_squadsToLoad);
            SquadToLoad newSquad = new SquadToLoad(
                unitName, 
                _prestige: 0, 
                _unitIndex: updatedSquads.Count
            );

            //int get base unit count
            int baseUnitCount = TabletopTavernData.Instance.GetBaseUnitCount(newSquad.UnitName);
            int hitpointsPerUnit = TabletopTavernData.Instance.GetHitPointsPerUnit(newSquad.UnitName);
            
            newSquad.SquadCurrentHealth = baseUnitCount * hitpointsPerUnit;
            newSquad.maxUnitCount = baseUnitCount;
            newSquad.HitPointsPerUnit = hitpointsPerUnit;

            updatedSquads.Add(newSquad);
            _squadsToLoad = updatedSquads.ToArray();
        }
        private void LoadStartingArmy()
        {
            _squadsToLoad = new SquadToLoad[0];
            UnitName[] unitNames = playPanel.hero.StartingArmyUnits;

            for (int j = 0; j < unitNames.Length; j++) 
            {
                AddUnitToArmy(unitNames[j]);
            }
        }
        public void RefreshArmyDisplay()
        {
            int armyIndex = 0;
            foreach (var card in squadDisplayCards)
            {
                if(card != null)
                    Destroy(card.gameObject);
            }
            squadDisplayCards.Clear();
            foreach (var squad in _squadsToLoad)
            {
                SquadDisplayCardMenu squadDisplayCard = Instantiate(squadDisplayCardMenu, startingUnitsParent);
                squadDisplayCard.SetUp(squad, false, _isEnemy: true);
                squadDisplayCard.LockCard(true);
                squadDisplayCard.gameObject.AddComponent<TroopHoverPlayPanel>().SetUp(armyIndex, playPanel);
                squadDisplayCard.gameObject.AddComponent<StartingTroopDoubleClickHandler>().SetUp(armyIndex, this);
                squadDisplayCard.gameObject.AddComponent<MemoriTooltipTrigger>().SetUpToolTip(
                    LocalizationManager.Instance.GetText(squad.UnitName.ToString()),
                    LocalizationManager.Instance.GetText("DoubleClickRemoveTroop")
                );
                squadDisplayCards.Add(squadDisplayCard);
                armyIndex++;
            }
            OnStartingArmyLengthChanged?.Invoke(_squadsToLoad.Length);
            CalculateRemainingTreasury();
        }
        private void LoadStartingGear()
        {
            gearCards = gearCardParent.GetComponentsInChildren<CollectionGearCard>();
            GearID[] allGear = GearData.GetGearIDs();
            //sort gear by rariity: Common, Uncommon, Rare
            System.Array.Sort(allGear, (b, a) => 
                GearData.GetGear(a).GearRarity.CompareTo(GearData.GetGear(b).GearRarity)
            );
            List<int> gearIdsAsInts = SaveDataHandler.GetGearIDsCollected();
            List<int> gearIdsAcknowledged = SaveDataHandler.GetGearIDsAcknowledged();
            for (int i = 0; i < allGear.Length; i++)
            {
                bool isCollected = gearIdsAsInts.Contains((int)allGear[i]);
                bool acknowledged = gearIdsAcknowledged.Contains((int)allGear[i]);
                gearCards[i].LoadGearCard(allGear[i], isCollected, acknowledged, _startingGear: this);
            }
            SelectGearCard(playPanel.StartingGearID);
            doubleClickHandler.SetStartingArmyManager(this);
        }

        public void SelectGearCard(GearID _gear)
        {
            startingGearID = _gear;
            selectedGearCard.LoadGearCard(_gear);
            #region Metaprogression
            if(gameObject.GetComponentInChildren<MetaprogressionLockedButton>() != null) {
                gameObject.GetComponentInChildren<MetaprogressionLockedButton>().CheckLockedState();
            }
            #endregion
            selectedGearCard.PlayPurchaseFeedbacks();
            playPanel.SetStartingGear(_gear);
            IAudioRequester.Instance.PlaySFX(SFXData.AddGear);
            SetStartingGearTitle(_gear);
            CalculateRemainingTreasury();
        }
        public void SetStartingGearTitle(GearID gearID)
        {
            string gearNameLocalized = LocalizationManager.Instance.GetText(gearID + "Name");
            string startingGearTitleLocalized = LocalizationManager.Instance.GetText("Starting Gear");
            string startingGearTitle = $"<color {ColorData.Secondary}>{startingGearTitleLocalized}:</color> <color {ColorData.Primary}>{gearNameLocalized}</color>";
            string gearOptionDescLocalized = LocalizationManager.Instance.GetText("DoubleClickRemove");
            gearCardOptionTooltipTrigger.SetUpToolTip(startingGearTitle, gearOptionDescLocalized);
        }
        public void CalculateRemainingTreasury()
        {
            // Debug.Log($"Calculating remaining treasury");
            int totalAmountSpent = 0;
            if(startingGearID != GearID.None) {
                int gearCost = GearData.GearCost(GearData.GetGear(startingGearID).GearRarity);
                if(SaveDataHandler.IsMetaprogressionNodeUnlocked(_startingGearReducedCostMetaprogressionModel)) {
                    gearCost -= _startingGearReducedCostMetaprogressionModel.NodeValue;
                    gearCost = Mathf.Max(0, gearCost);
                    Debug.Log($"Reduced starting gear cost to: {gearCost}");
                }
                totalAmountSpent += gearCost;
            }
            //get cost of each unit in army
            foreach (var squad in _squadsToLoad)
            {
                int unitCost = TabletopTavernData.Instance.GetUnitCost(squad.UnitName);
                totalAmountSpent += unitCost;
            }
            int remainingAmount = startingGold - totalAmountSpent;
            remainingTreasury.Value = remainingAmount;
        }
        public void UpdateRemainingTreasury(int _amount)
        {
            remainingTreasuryText.text = $"{_amount}";
        }
        public void PointerOverTroop(int _index)
        {
            if(_index == 99) 
            {
                squadBattleInfo.SetUpCampaign(playPanel.uniqueSquad, Team.Player);
                return;
            }
            squadBattleInfo.SetUpCampaign(_squadsToLoad[_index], Team.Player);
        }
        public void PointerOverTroop(SquadToLoad _squadToLoad)
        {
            squadBattleInfo.SetUpCampaign(_squadToLoad, Team.Player);
        }
        public void PointerOffTroop()
        {
            squadBattleInfo.Unhover();
        }
        public void RemoveTroop(int _index)
        {
            if(_index < 0 || _index >= _squadsToLoad.Length) return;

            //here
            if(playPanel.StartingArmyLockedForHero) {
                NotificationManager.Instance.ErrorNotification(
                    LocalizationManager.Instance.GetText("OneCompletionRequired")
                );
                return;
            }

            List<SquadToLoad> updatedSquads = new (_squadsToLoad);
            updatedSquads.RemoveAt(_index);
            _squadsToLoad = updatedSquads.ToArray();
            PointerOffTroop();
            RefreshArmyDisplay();
            TooltipManager.Instance.HideTooltip();
            IAudioRequester.Instance.PlaySFX(SFXData.DisbandSquad);
        }
        public void AddTroop(SquadToLoad _squadToAdd)
        {
            int maxArmySize = 10;//SaveDataHandler.IsMetaprogressionNodeUnlocked(_thirdReserveSlotMetaprogressionModel) ? 13 : 12;
            if(_squadsToLoad.Length == maxArmySize)
            {
                NotificationManager.Instance.ErrorNotification(
                    LocalizationManager.Instance.GetText("You cannot add more than 10 units to your starting army.")
                );
                return;
            }

            List<SquadToLoad> updatedSquads = new(_squadsToLoad)
            {
                _squadToAdd
            };
            _squadsToLoad = updatedSquads.ToArray();
            RefreshArmyDisplay();
            TooltipManager.Instance.HideTooltip();
            IAudioRequester.Instance.PlaySFX(SFXData.RecruitUnit);
        }
        public void LoadUnitsOfRace(Race race)
        {
            // Clear existing unit cards
            foreach (Transform child in tier1UnitsParent)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in tier2UnitsParent)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in tier3UnitsParent)
            {
                Destroy(child.gameObject);
            }

            // Get units of the specified race
            UnitName[] unitsOfRace = TabletopTavernData.Instance.GetUnitsOfRace(race);
            foreach (var unitName in unitsOfRace)
            {
                int unitTier = TabletopTavernData.Instance.GetUnitTierFromUnitName(unitName);
                GameObject unitCardObj = Instantiate(squadDisplayCardMenu.gameObject);
                SquadDisplayCardMenu unitCard = unitCardObj.GetComponent<SquadDisplayCardMenu>();

                SquadToLoad newSquad = new SquadToLoad(unitName, 0, 0);
                int baseUnitCount = TabletopTavernData.Instance.GetBaseUnitCount(newSquad.UnitName);
                int hitpointsPerUnit = TabletopTavernData.Instance.GetHitPointsPerUnit(newSquad.UnitName);
                
                newSquad.SquadCurrentHealth = baseUnitCount * hitpointsPerUnit;
                newSquad.maxUnitCount = baseUnitCount;
                newSquad.HitPointsPerUnit = hitpointsPerUnit;
                
                unitCard.SetUp(newSquad, false, _isEnemy: true);
                unitCard.LockCard(true);
                unitCard.gameObject.AddComponent<TroopHoverPlayPanel>().SetUp(-1, playPanel);
                
                //check if squad is discovered
                if(troopsRecruitied.Contains(unitName))
                {
                    unitCard.gameObject.AddComponent<MemoriTooltipTrigger>().SetUpToolTip(
                        LocalizationManager.Instance.GetText(unitName.ToString()),
                        LocalizationManager.Instance.GetText("DoubleClickAddTroop")
                    );
                    unitCard.gameObject.AddComponent<StartingTroopDoubleClickHandler>().SetUp(-1, this);
                } 
                else 
                {
                    //instantiate locked blocker
                    GameObject lockedBlockerInstance = Instantiate(lockedBlocker, unitCard.transform);
                    unitCard.gameObject.AddComponent<MemoriTooltipTrigger>().SetUpToolTip(
                        "Locked",
                        "Recruit this troop in Campaign to unlock."
                    );
                }


                switch (unitTier)
                {
                    case 1:
                        unitCardObj.transform.SetParent(tier1UnitsParent, false);
                        break;
                    case 2:
                        unitCardObj.transform.SetParent(tier2UnitsParent, false);
                        break;
                    case 3:
                        unitCardObj.transform.SetParent(tier3UnitsParent, false);
                        break;
                    default:
                        Destroy(unitCardObj);
                        break;
                }
            }
        }
        private void OnDestroy() 
        {
            remainingTreasury.OnValueChanged -= UpdateRemainingTreasury;
            remainingTreasury.OnValueChanged -= playPanel.RemainingTreasuryChanged;
        }
    }
}