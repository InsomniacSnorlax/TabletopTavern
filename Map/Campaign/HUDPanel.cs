using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Memori.Utilities;
using Memori.SaveData;
using Memori.Audio;
using TJ.Map;
using TJ.Settings;
using System;
using Memori.Tooltip;
using System.Collections;
using Memori.UI;
using Unity.Mathematics;
using MoreMountains.Feedbacks;
using Memori.Input;
using Memori.Localization;
using Memori.Steamworks;

namespace TJ.Map
{
    public class HUDPanel : MonoBehaviour
    {
        [SerializeField] private Button showSettingsButton;
        [SerializeField] private MemoriTooltipTrigger settingsTooltipTrigger;

        [Header("Player Company")]
        [SerializeField] private MemoriTooltipTrigger chapterTooltipTrigger;

        [Header("Player Company")]
        [SerializeField] private SquadDisplayCardMenu squadDisplayCardMenuPrefab;
        [SerializeField] private Transform deployedUnitsParent, reserveUnitsParent;
        [SerializeField] private GameObject emptySquadDisplayCardMenuPrefab;

        [Header("Troops Areas")]
        [SerializeField] private RectTransform deployedTroopsArea;
        [SerializeField] private RectTransform reserveTroopsArea;
        [SerializeField] private RectTransform[] troopsIndexAreas;
        public RectTransform DeployedTroopsArea => deployedTroopsArea;
        public RectTransform ReserveTroopsArea => reserveTroopsArea;
        public RectTransform[] TroopsIndexAreas => troopsIndexAreas;
        [SerializeField] private MetaprogressionLockedButton thirdReserveSlotLockedButton;
        [SerializeField] private Image deployedTroopsAreaImage, reserveTroopsAreaImage;
        [SerializeField] private TMP_Text deployedTroopsCountText, reserveTroopsCountText;

        [Header("Popups")]
        [SerializeField] private CanvasGroup disbandSquadConfirmationPopup;
        [SerializeField] private Button disbandSquadButtonConfirm, disbandSquadButtonCancel;
        [SerializeField] private CanvasGroup renameSquadConfirmationPopup;
        [SerializeField] private Button renameSquadButtonConfirm, renameSquadButtonCancel;
        [SerializeField] private TMP_InputField renameSquadInputField;

        [Header("Gold")]
        [SerializeField] private TMP_Text goldAmountText;
        [SerializeField] private MemoriTooltipTrigger goldTooltipTrigger;
        [SerializeField] private MMF_Player goldMMFeedback;

        [Header("Top Row")]
        [SerializeField] private TMP_Text chapterText;
        [SerializeField] private TMP_Text difficultyText, heroNameText, heroRaceText;
        // [SerializeField] private GameObject peasantIcon, squireIcon, knightIcon, baronIcon, dukeIcon, kingIcon, emperorIcon;
        [SerializeField] private MemoriTooltipTrigger difficultyTooltipTrigger, heroNameTooltipTrigger, heroRaceTooltipTrigger;
        [SerializeField] private MMF_Player chapterMMFeedback;

        [Header("Gear")]
        [SerializeField] private GearDisplay[] gearDisplays;

        [Header("Consumables")]
        [SerializeField] private ConsumableUI[] consumableUI;
        public ConsumableUI[] ConsumableUI => consumableUI;
        [SerializeField] private UILineDrawer uILineDrawer;
        public UILineDrawer UILineDrawer => uILineDrawer;

        [Header("SquadBattleInfo")]
        [SerializeField] private SquadBattleInfo squadBattleInfo;

        [Header("Troop Panel for Squad Displays")]
        public Transform DeployedUnitsParent => deployedUnitsParent;
        public Transform ReserveUnitsParent => reserveUnitsParent;
        
        private int deployedTroopsCount, reserveTroopsCount;
        public int DeployedTroopsCount => deployedTroopsCount;
        public int ReserveTroopsCount => reserveTroopsCount;
        public int MaxReserveSlots => campaignSaveManager.MaxReserveSlots;

        [SerializeField] private Animator hudAnimator;
        public Animator HudAnimator => hudAnimator;

        [Header("Legend")]
        [SerializeField] private GameObject legendGO;
        public GameObject LegendGO => legendGO;
        [SerializeField] private MapLabel skirmishLabel;
        [SerializeField] private MapLabel eventLabel;
        [SerializeField] private MapLabel shopLabel;
        [SerializeField] private MapLabel townLabel;
        [SerializeField] private MapLabel treasureLabel;
        [SerializeField] private MapLabel unknownLabel;
        [SerializeField] private MapLabel tavernLabel;
        [SerializeField] private MapLabel campfireLabel;
        // public MapLabel SkirmishLabel => skirmishLabel;
        // public MapLabel EventLabel => eventLabel;
        // public MapLabel ShopLabel => shopLabel;
        // public MapLabel TownLabel => townLabel;
        // public MapLabel TreasureLabel => treasureLabel;
        // public MapLabel UnknownLabel => unknownLabel;

        // [Header("Testing")]
        // [SerializeField] private Button testAquireConsumableButton;

        CampaignSaveManager campaignSaveManager;
        MapSceneUIManager mapSceneUIManager;
        List<SquadDisplayCardMenu> playerSquadsCards;
        public List<SquadDisplayCardMenu> PlayerSquadsCards => playerSquadsCards;
        List<string> pendingDisbandGuids = new();
        string renameSquadGUID;
        Coroutine rollGoldCoroutine;
        int chapter;
        List<SquadDisplayCardMenu> selectedCards = new();
        public IReadOnlyList<SquadDisplayCardMenu> SelectedCards => selectedCards;
        List<GameObject> emptySquadCards = new();
        int hoveredSquadIndex;
        public int HoveredSquadIndex => hoveredSquadIndex;

        public void SetUp(CampaignSaveManager _campaignSaveManager, MapSceneUIManager _mapSceneUIManager)
        {
            campaignSaveManager = _campaignSaveManager;
            mapSceneUIManager = _mapSceneUIManager;
            showSettingsButton.onClick.AddListener(() => SettingsManager.Instance.OpenSettingsPanel());

            disbandSquadButtonConfirm.onClick.AddListener(() => DisbandPendingSquads());
            disbandSquadButtonCancel.onClick.AddListener(() => HideDisbandSquadConfirmation());
            renameSquadButtonConfirm.onClick.AddListener(() => RenameSquad());
            renameSquadButtonCancel.onClick.AddListener(() => { renameSquadConfirmationPopup.CGDisable(); mapSceneUIManager.MapSceneManager.SetMapInput(true); });
            // testAquireConsumableButton.onClick.AddListener(() => CampaignManager.Instance.CampaignSaveManager.AquireConsumable(ConsumableData.GetRandomConsumable()));

            campaignSaveManager.OnChapterCompleted += UpdateChapterText;
            campaignSaveManager.OnGoldChanged += OnGoldChanged;
            campaignSaveManager.OnUnitHealthChanged += ArmyHealthChanged;
            campaignSaveManager.OnGearChanged += ReloadGear;
            campaignSaveManager.OnArmyStructureChanged += ArmyStructureChanged;
            campaignSaveManager.OnConsumablesChanged += ReloadConsumables;
            InputHandler.Instance.SecondaryActionPressed += CloseAllPopUps;

            ReloadGear();
            ReloadConsumables();
            ArmyStructureChanged();
            DeselectAllCards();

            skirmishLabel.SetUp(NodeType.Skirmish);
            eventLabel.SetUp(NodeType.Event);
            shopLabel.SetUp(NodeType.Shop);
            townLabel.SetUp(NodeType.Town);
            treasureLabel.SetUp(NodeType.Treasure);
            unknownLabel.SetUp(NodeType.Skirmish, true);
            if (tavernLabel != null) tavernLabel.SetUp(NodeType.Games);
            if (campfireLabel != null) campfireLabel.SetUp(NodeType.Campfire);
            SetUpDifficultyTooltip();
            UpdateHeroNameAndRace();
            legendGO.SetActive(true);

            string chapterLocalized = LocalizationManager.Instance.GetText("Chapter");
            chapterTooltipTrigger.SetUpToolTip(_title: chapterLocalized);
            settingsTooltipTrigger.SetUpToolTip(_title: LocalizationManager.Instance.GetText("Settings"));
        }
        private void SetUpDifficultyTooltip()
        {
            TT_Difficulty difficulty = CampaignManager.Instance.CampaignSaveManager.SaveData.difficultyLevel;
            DifficultyLevel difficultyData = DifficultyData.GetDifficultyLevelData(difficulty);

            string difficultyLocalized = LocalizationManager.Instance.GetText(difficultyData.difficultyName);
            difficultyText.text = difficultyLocalized;
            // peasantIcon.SetActive(false);
            // squireIcon.SetActive(false);
            // knightIcon.SetActive(false);
            // baronIcon.SetActive(false);
            // dukeIcon.SetActive(false);
            // kingIcon.SetActive(false);
            // emperorIcon.SetActive(false);

            string additionalModifiersDesc = "";
            List<string> allPreviousModifiers = DifficultyData.GetAllDifficultyModifiersBeforeLevel(difficulty+1);

            foreach (string modifier in allPreviousModifiers)
            {
                additionalModifiersDesc += "- " + LocalizationManager.Instance.GetText(modifier) + "\n";
            }
            string difficultLevelTitleLocalized = LocalizationManager.Instance.GetText("Difficulty");
            difficultyTooltipTrigger.SetUpToolTip(_title: $"{difficultLevelTitleLocalized}: {difficultyLocalized}", _description: additionalModifiersDesc);
        }
        private void UpdateHeroNameAndRace()
        {
            Hero hero = HeroData.GetHeroByID(campaignSaveManager.SaveData.heroID);
            string heroRace = HeroData.GetRaceFromHero(CampaignManager.Instance.CampaignSaveManager.GetHeroID()).ToString();
            string heroRaceLocalized = LocalizationManager.Instance.GetText(heroRace);
            string heroNameLocalized = LocalizationManager.Instance.GetText(hero.HeroName);
            heroNameText.text = heroNameLocalized;
            heroRaceText.text = heroRaceLocalized;

            string heroBonusText1string = LocalizationManager.Instance.GetText(hero.HeroBonusDescription[0].Replace("heroBonusDescription", "heroBonusTitle")) + ": " + LocalizationManager.Instance.GetText(hero.HeroBonusDescription[0]);
            string heroBonusText2string = LocalizationManager.Instance.GetText(hero.HeroBonusDescription[1].Replace("heroBonusDescription", "heroBonusTitle")) + ": " + LocalizationManager.Instance.GetText(hero.HeroBonusDescription[1]);
            string raceBonusTextstring = LocalizationManager.Instance.GetText(hero.Race+ "BonusDescription");
            ColorData.XMLTagColorApplicator(ref heroBonusText1string);
            ColorData.XMLTagColorApplicator(ref heroBonusText2string);
            ColorData.XMLTagColorApplicator(ref raceBonusTextstring);
            heroBonusText1string += "\n" + heroBonusText2string;
            heroNameTooltipTrigger.SetUpToolTip(_title: heroNameLocalized, _description: heroBonusText1string);
            heroRaceTooltipTrigger.SetUpToolTip(_title: heroRaceLocalized, _description: raceBonusTextstring);
        }
        public void ArmyStructureChanged()
        {
            // Debug.Log($"Army structure changed");
            RefreshTroopsPanel();
            CloseAllPopUps();
            squadBattleInfo.InvalidateSnapshotCache();
            squadBattleInfo.Unhover();
        }
        public void ArmyHealthChanged()
        {
            // Debug.Log($"Army health changed");
            SquadToLoad[] playerSquadsSaveData = campaignSaveManager.SaveData.playerArmy;
            if(playerSquadsCards == null) return;
            for (int i = 0; i < playerSquadsCards.Count; i++)
            {
                //match squad card with save data by SquadId
                for (int j = 0; j < playerSquadsSaveData.Length; j++)
                {
                    if (playerSquadsCards[i].SquadId == playerSquadsSaveData[j].UnitIndex)
                    {
                        playerSquadsCards[i].UpdateUnitCount(playerSquadsSaveData[j]);
                        break;
                    }
                }
            }
        }
        private void RefreshTroopsPanel()
        {
            // Debug.Log($"Refreshing troops panel");
            playerSquadsCards = new List<SquadDisplayCardMenu>();
            foreach (Transform child in deployedUnitsParent) Destroy(child.gameObject);
            foreach (Transform child in reserveUnitsParent) Destroy(child.gameObject);
            emptySquadCards = new();

            SquadToLoad[] playerSquads = CampaignManager.Instance.CampaignSaveManager.SaveData.playerArmy;
            deployedTroopsCount = 0;
            reserveTroopsCount = 0;
            if(playerSquads == null) return;
            int maxArmySize = 10 + campaignSaveManager.MaxReserveSlots;
            for (int i = 0; i < playerSquads.Length && i < maxArmySize; i++)
            {
                bool isDeployed = i < 10;
                Transform unitParentTransform = isDeployed ? deployedUnitsParent : reserveUnitsParent;

                if (playerSquads[i].UnitIndex == -1)
                {
                    GameObject newEmptyCard = Instantiate(emptySquadDisplayCardMenuPrefab, unitParentTransform);
                    newEmptyCard.name = $"Empty Squad Card {i}";
                    emptySquadCards.Add(newEmptyCard);
                    continue;
                }
                // Debug.Log($"index {i} - loading squad {playerSquads[i].UnitName} (ID: {playerSquads[i].UnitIndex})");

                SquadDisplayCardMenu squadDisplayCardMenu = Instantiate(squadDisplayCardMenuPrefab, unitParentTransform);
                squadDisplayCardMenu.SetUp(playerSquads[i], !isDeployed, this);
                playerSquadsCards.Add(squadDisplayCardMenu);

                if (isDeployed) deployedTroopsCount++;
                else reserveTroopsCount++;
            }
            string deployedLocalized = LocalizationManager.Instance.GetText("Deployed");
            string reserveLocalized = LocalizationManager.Instance.GetText("Reserve");

            deployedTroopsCountText.text = $"{deployedLocalized} {deployedTroopsCount}/10";
            reserveTroopsCountText.text = $"{reserveLocalized} {reserveTroopsCount}/{campaignSaveManager.MaxReserveSlots}";
            deployedTroopsAreaImage.enabled = false;
            reserveTroopsAreaImage.enabled = false;
            if (thirdReserveSlotLockedButton != null) thirdReserveSlotLockedButton.CheckLockedState();

            if(deployedTroopsCount + reserveTroopsCount == 10 + campaignSaveManager.MaxReserveSlots) SteamStatic.UnlockAchievement(SteamData.ACHIEVEMENT_FULL_ARMY);
        }
        public void HoverSquad(SquadToLoad squad, bool _hovered, Transform _squadCardTransform)
        {
            Team team = Team.Player;
            if (mapSceneUIManager.EngagementPanel.EnemyArmyParent == _squadCardTransform.parent)
            {
                team = Team.Enemy;
            }

            if (_hovered)
            {
                squadBattleInfo.SetUpCampaign(squad, team);
                hoveredSquadIndex = squad.UnitIndex;
            }
            else if (selectedCards.Count > 0)
            {
                squadBattleInfo.SetUpCampaign(selectedCards[0].GetSquadToLoad(), team);
            }
            else
            {
                squadBattleInfo.Unhover();
                hoveredSquadIndex = -1;
            }
        }
        public void SelectSingleCard(SquadDisplayCardMenu card)
        {
            foreach (SquadDisplayCardMenu c in selectedCards)
                c.SelectSquad(false);
            foreach (SquadDisplayCardMenu c in playerSquadsCards)
                c.SetOptionsVisibility(false, false);
            selectedCards.Clear();

            if (card == null)
            {
                squadBattleInfo.Unhover();
                return;
            }

            selectedCards.Add(card);
            card.SelectSquad(true);
            UpdateSelectionOptions();
            squadBattleInfo.SetUpCampaign(card.GetSquadToLoad(), Team.Player);
        }
        public void ToggleCardInSelection(SquadDisplayCardMenu card)
        {
            if (selectedCards.Contains(card))
            {
                selectedCards.Remove(card);
                card.SelectSquad(false);
                card.SetOptionsVisibility(false, false);
            }
            else
            {
                selectedCards.Add(card);
                card.SelectSquad(true);
            }

            UpdateSelectionOptions();

            if (selectedCards.Count > 0)
                squadBattleInfo.SetUpCampaign(selectedCards[^1].GetSquadToLoad(), Team.Player);
            else
                squadBattleInfo.Unhover();
        }
        public void DeselectAllCards()
        {
            if (playerSquadsCards == null) return;
            foreach (SquadDisplayCardMenu c in selectedCards)
                c.SelectSquad(false);
            foreach (SquadDisplayCardMenu c in playerSquadsCards)
                c.SetOptionsVisibility(false, false);
            selectedCards.Clear();
            squadBattleInfo.Unhover();
        }
        private void UpdateSelectionOptions()
        {
            bool isSingle = selectedCards.Count == 1;
            bool canMerge = selectedCards.Count >= 2
                && selectedCards.TrueForAll(c => c.GetSquadToLoad().UnitName == selectedCards[0].GetSquadToLoad().UnitName)
                && selectedCards.TrueForAll(c => c.GetSquadToLoad().UnitPrestige == selectedCards[0].GetSquadToLoad().UnitPrestige);

            bool canPrestigeMulti = selectedCards.Count == 3
                && selectedCards.TrueForAll(c => c.GetSquadToLoad().UnitName == selectedCards[0].GetSquadToLoad().UnitName)
                && selectedCards.TrueForAll(c => c.GetSquadToLoad().UnitPrestige == selectedCards[0].GetSquadToLoad().UnitPrestige)
                && selectedCards.TrueForAll(c => c.GetSquadToLoad().SquadCurrentHealth > 0)
                && selectedCards[0].GetSquadToLoad().UnitPrestige < 2;

            SquadDisplayCardMenu mostRecent = selectedCards.Count > 0 ? selectedCards[^1] : null;
            foreach (SquadDisplayCardMenu c in playerSquadsCards)
            {
                if (!selectedCards.Contains(c))
                {
                    c.SetOptionsVisibility(false, false);
                    continue;
                }
                if (isSingle)
                    c.SetOptionsVisibility(true, true);
                else if (c == mostRecent)
                    c.SetOptionsVisibility(true, false, canMerge, canPrestigeMulti);
                else
                    c.SetOptionsVisibility(false, false);
            }
        }
        public void UpdateChapterText(int _chapter)
        {
            chapter = _chapter;
            // string chapterLocalized = LocalizationManager.Instance.GetText("Chapter");
            chapterText.text = $"{_chapter + 1}";
            // chapterText.text = $"{chapterLocalized} {_chapter+1}";
            chapterMMFeedback.PlayFeedbacks();
        }
        private void ReloadGear()
        {
            List<GearID> gearNames = campaignSaveManager.SaveData.Gear;

            for (int i = 0; i < gearDisplays.Length; i++)
                gearDisplays[i].UnloadGearDisplay();

            for (int i = 0; i < gearNames.Count; i++)
                gearDisplays[i].LoadGearDisplay(gearNames[i]);

            for (int i = 0; i < gearDisplays.Length; i++)
            {
                if(gearDisplays[i].GetComponentInChildren<MetaprogressionLockedButton>() != null) {
                    gearDisplays[i].GetComponentInChildren<MetaprogressionLockedButton>().CheckLockedState();
                }
            }

            CampaignManager.Instance.ArmyJuiceManager.GearReloaded(gearDisplays);
        }
        private void ReloadConsumables()
        {
            // Debug.Log($"Reloading consumables");
            List<ConsumableEnum> consumableNames = CampaignManager.Instance.CampaignSaveManager.SaveData.consumables;
            for (int i = 0; i < consumableUI.Length; i++)
            {
                consumableUI[i].UnloadConsumableUI();

                if(consumableUI[i].GetComponentInChildren<MetaprogressionLockedButton>() != null) {
                    consumableUI[i].GetComponentInChildren<MetaprogressionLockedButton>().CheckLockedState();
                }
            }

            for (int i = 0; i < consumableNames.Count; i++){
                consumableUI[i].LoadConsumableUI(consumableNames[i]);
            }

            CampaignManager.Instance.ArmyJuiceManager.ConsumableReloaded(consumableUI);
        }
        public void ShowDisbandSquadConfirmation(string _guID)
        {
            pendingDisbandGuids = new List<string> { _guID };
            disbandSquadConfirmationPopup.CGEnable();
            disbandSquadConfirmationPopup.GetComponentInChildren<SettingsToggle>().OverrideToggleFromSettings();
        }
        public void HideDisbandSquadConfirmation()
        {
            disbandSquadConfirmationPopup.CGDisable();
        }
        public void DisbandSquad(string _guID)
        {
            campaignSaveManager.DisbandSquad(_guID);
            IAudioRequester.Instance.PlaySFX(SFXData.DisbandSquad);
            HideDisbandSquadConfirmation();
        }
        private void DisbandPendingSquads()
        {
            campaignSaveManager.DisbandMultipleSquads(pendingDisbandGuids);
            IAudioRequester.Instance.PlaySFX(SFXData.DisbandSquad);
            HideDisbandSquadConfirmation();
        }
        public void AttemptDisbandSelectedSquads()
        {
            if (selectedCards.Count == 0) return;
            List<string> guids = new();
            foreach (SquadDisplayCardMenu card in selectedCards)
                guids.Add(card.UniqueID);

            if (PlayerPrefs.GetInt("DisbandSquadConfirmation", 0) == 1)
            {
                pendingDisbandGuids = guids;
                DisbandPendingSquads();
            }
            else
            {
                pendingDisbandGuids = guids;
                disbandSquadConfirmationPopup.CGEnable();
                disbandSquadConfirmationPopup.GetComponentInChildren<SettingsToggle>().OverrideToggleFromSettings();
                IAudioRequester.Instance.PlaySFX(SFXData.DisbandSquad);
            }
        }
        public void MergeSelectedSquads()
        {
            List<string> guids = new();
            foreach (SquadDisplayCardMenu card in selectedCards)
                guids.Add(card.UniqueID);
            campaignSaveManager.MergeSquads(guids);
        }
        public void GiveRenameSquadPrompt(string _guID)
        {
            renameSquadGUID = _guID;
            renameSquadInputField.text = campaignSaveManager.GetUnitNameOrUnitNameOverride(_guID);
            renameSquadConfirmationPopup.CGEnable();
            mapSceneUIManager.MapSceneManager.SetMapInput(false);
        }
        public void RenameSquad()
        {
            campaignSaveManager.RenameSquad(renameSquadGUID, renameSquadInputField.text);
            renameSquadConfirmationPopup.CGDisable();
            mapSceneUIManager.MapSceneManager.SetMapInput(true);
        }
        public void MoveUnit(string _guID, int _index)
        {
            campaignSaveManager.MoveUnitToIndex(_guID, _index);
            TutorialManager.Instance.CompleteStepCheck(TutorialStepEnum.ReorderUnits);
        }
        public void ReorderUnits()
        {
            // Debug.Log($"Reordering units");
            List<string> _unitIndexes = new();
            //go through deployed and then reserves, if child object squad battle card add index else add -1
            for (int i = 0; i < deployedUnitsParent.childCount; i++)
            {
                Transform child = deployedUnitsParent.GetChild(i);
                if (child.GetComponent<SquadDisplayCardMenu>())
                {
                    _unitIndexes.Add(child.GetComponent<SquadDisplayCardMenu>().UniqueID);
                }
                else
                {
                    _unitIndexes.Add("-1");
                }
            }
            // partition deployed: real units first, empty slots at the end
            List<string> deployedReal = _unitIndexes.FindAll(x => x != "-1");
            List<string> deployedEmpty = _unitIndexes.FindAll(x => x == "-1");
            _unitIndexes = deployedReal;
            _unitIndexes.AddRange(deployedEmpty);

            List<string> reserveIndexes = new();
            for (int i = 0; i < reserveUnitsParent.childCount; i++)
            {
                Transform child = reserveUnitsParent.GetChild(i);
                if (child.GetComponent<SquadDisplayCardMenu>())
                {
                    reserveIndexes.Add(child.GetComponent<SquadDisplayCardMenu>().UniqueID);
                }
                else
                {
                    reserveIndexes.Add("-1");
                }
            }
            // partition reserves: real units first, empty slots at the end
            List<string> reserveReal = reserveIndexes.FindAll(x => x != "-1");
            List<string> reserveEmpty = reserveIndexes.FindAll(x => x == "-1");
            _unitIndexes.AddRange(reserveReal);
            _unitIndexes.AddRange(reserveEmpty);

            // Pad to full army array length in case fewer reserve slots are visible (e.g. 3rd slot locked)

            //error here if deleted savedata
            if(campaignSaveManager.SaveData == null) {
                return;
            }
            int armyLength = campaignSaveManager.SaveData.playerArmy.Length;
            while (_unitIndexes.Count < armyLength) _unitIndexes.Add("-1");
            // Trim excess entries (e.g. stale UI children after prestige reduces army size)
            while (_unitIndexes.Count > armyLength) _unitIndexes.RemoveAt(_unitIndexes.Count - 1);

            string totallist = "";
            foreach (string item in _unitIndexes)
            {
                totallist += item[..1] + ", ";
            }
            campaignSaveManager.UpdateUnitIndexes(_unitIndexes);
        }
        public void HighlightDeployedTroopsArea(bool _highlight)
        {
            deployedTroopsAreaImage.enabled = _highlight;

            if (_highlight) IAudioRequester.Instance.PlaySFX(SFXData.HoveredDepoyedTroops);
        }
        public void HighlightReserveTroopsArea(bool _highlight)
        {
            reserveTroopsAreaImage.enabled = _highlight;

            if (_highlight) IAudioRequester.Instance.PlaySFX(SFXData.HoveredReserveTroops);
        }
        public void LockCards(bool _lock)
        {
            foreach (SquadDisplayCardMenu squadDisplayCardMenu in playerSquadsCards)
            {
                squadDisplayCardMenu.LockCard(_lock);
            }
        }
        public void PrestigeUnit(string _guID)
        {
            CampaignManager.Instance.ArmyJuiceManager.UpdateSquadOnChange(new ArmyJuice {
                uniqueID = _guID,
                armyJuiceEnum = ArmyJuiceEnum.Prestige,
            });

            bool isMultiSelectPrestige = selectedCards.Count == 3
                && selectedCards.TrueForAll(c => c.GetSquadToLoad().UnitName == selectedCards[0].GetSquadToLoad().UnitName)
                && selectedCards.TrueForAll(c => c.GetSquadToLoad().UnitPrestige == selectedCards[0].GetSquadToLoad().UnitPrestige)
                && selectedCards.TrueForAll(c => c.GetSquadToLoad().SquadCurrentHealth > 0)
                && selectedCards[0].GetSquadToLoad().UnitPrestige < 2;

            if (isMultiSelectPrestige)
            {
                List<string> consumeUIDs = new();
                foreach (SquadDisplayCardMenu c in selectedCards)
                {
                    if (c.GetSquadToLoad().UniqueID != _guID)
                        consumeUIDs.Add(c.GetSquadToLoad().UniqueID);
                }
                campaignSaveManager.PrestigeAndCombineSpecificUnits(_guID, consumeUIDs[0], consumeUIDs[1]);
            }
            else
            {
                campaignSaveManager.PrestigeAndCombineUnits(_guID);
            }

            IAudioRequester.Instance.PlaySFX(SFXData.PrestigeUnit);
        }
        public void CheckForPrestigeAvailability(PrestigeUnitButton _prestigeUnitButton, UnitName _unitName, int _unitLevel)
        {
            bool isAvailable = campaignSaveManager.CheckForPrestigeAvailability(_unitName, _unitLevel);
            _prestigeUnitButton.SetPrestigeAvailability(isAvailable);

            if (isAvailable)
            {
                // IAudioRequester.Instance.PlaySFX(SFXData.PrestigeAvailable);
                TutorialManager.Instance.LoadStepsFromRandomSpot(new TutorialStep[1] { TutorialData.PrestigeUnit });
            }
        }
        public void OnGoldChanged(int _goldAmount)
        {
            goldMMFeedback.StopFeedbacks();
            goldMMFeedback.PlayFeedbacks();
            if (rollGoldCoroutine != null) StopCoroutine(rollGoldCoroutine);
            rollGoldCoroutine = StartCoroutine(MemoriUI.RollTextCoroutine(float.Parse(goldAmountText.text), _goldAmount, goldAmountText));

            string earnedLocalized = LocalizationManager.Instance.GetText("earned interest per");
            string bonusLocalized = LocalizationManager.Instance.GetText("bonus interest from Omen of Famine");
            string ironBankLocalized = LocalizationManager.Instance.GetText("interest bonus from Iron Bank");
            string interestLocalized = LocalizationManager.Instance.GetText("Interest at turn end");
            string maxLocalized = LocalizationManager.Instance.GetText("Max");

            string flavorText = $"(+{CampaignManager.Instance.EconomyManager.GetBaseInterest()}) 1 <sprite name=GoldSprite> {earnedLocalized} {CampaignManager.Instance.CampaignSaveManager.GoldRequiredToGenerateInterest} <sprite name=GoldSprite> ({maxLocalized} {CampaignManager.Instance.EconomyManager.GetMaxInterest()})";

            int bonusFromOmenOfFamine = 0;
            if (CampaignManager.Instance.GearManager.CheckForGear(GearID.OmenofFamine))
            {
                List<GearID> gearIDs = campaignSaveManager.SaveData.Gear;
                bonusFromOmenOfFamine = campaignSaveManager.MaxGear - gearIDs.Count;
                flavorText += $"\n(+{bonusFromOmenOfFamine}) <sprite name=GoldSprite> {bonusLocalized}";
            }

            if (CampaignManager.Instance.GearManager.CheckForGear(GearID.IronBank))
            {
                flavorText += $"\n(+{CampaignManager.Instance.EconomyManager.GetBaseInterest() + bonusFromOmenOfFamine}) <sprite name=GoldSprite> {ironBankLocalized}";
            }

            goldTooltipTrigger.SetUpToolTip(_description: $"+{CampaignManager.Instance.EconomyManager.GetTotalInterest()} <sprite name=GoldSprite> {interestLocalized}", _flavorText: flavorText);

            ReloadGear();
        }
        public void HideZeroHealthSquads()
        {
            for (int i = 0; i < playerSquadsCards.Count; i++)
            {
                playerSquadsCards[i].HideDeadSquads();
            }
            campaignSaveManager.ReorderUnits();
        }
        public string GetGuidFormHoveredUnit(int _index)
        {
            for (int i = 0; i < playerSquadsCards.Count; i++)
            {
                if (playerSquadsCards[i].SquadId == _index)
                {
                    return playerSquadsCards[i].UniqueID;
                }
            }
            Debug.LogError($"GetGuidFormHoveredUnit({_index}) - No squad found");
            return null;
        }
        public void CloseNonSquadPopUps()
        {
            foreach (ConsumableUI consumableUI in consumableUI)
                consumableUI.CloseConsumableOptions();
            foreach (GearDisplay gearDisplay in gearDisplays)
                gearDisplay.CloseGearSellTag();
            HideDisbandSquadConfirmation();
        }
        public void CloseAllPopUps()
        {
            CloseNonSquadPopUps();
            DeselectAllCards();
            IAudioRequester.Instance.PlaySFX(SFXData.ClosePopUp);
        }
        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            if (selectedCards.Count == 0) return;

            PointerEventData pointerData = new(EventSystem.current) { position = Input.mousePosition };
            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponentInParent<SquadDisplayCardMenu>() != null)
                    return;
            }

            DeselectAllCards();
        }
        public void OnDestroy()
        {
            if (InputHandler.HasInstance) {
                InputHandler.Instance.SecondaryActionPressed -= CloseAllPopUps;
            }

            if (campaignSaveManager == null) return;
            campaignSaveManager.OnChapterCompleted -= UpdateChapterText;
            campaignSaveManager.OnGoldChanged -= OnGoldChanged;
            campaignSaveManager.OnUnitHealthChanged -= ArmyHealthChanged;
            campaignSaveManager.OnGearChanged -= ReloadGear;
            campaignSaveManager.OnArmyStructureChanged -= ArmyStructureChanged;
            campaignSaveManager.OnConsumablesChanged -= ReloadConsumables;
        }
        public void DestroyEmptySquadCards()
        {
            foreach (GameObject emptySquadCard in emptySquadCards)
            {
                Destroy(emptySquadCard);
            }
            emptySquadCards.Clear();
        }
        public void ReplaceEmptySquadCards()
        {
            DestroyEmptySquadCards();
            for (int i = deployedUnitsParent.childCount; i < 10; i++)
            {
                GameObject newEmptyCard = Instantiate(emptySquadDisplayCardMenuPrefab, deployedUnitsParent);
                newEmptyCard.name = $"Empty Squad Card {i}";
                emptySquadCards.Add(newEmptyCard);
            }
            for (int i = reserveUnitsParent.childCount; i < campaignSaveManager.MaxReserveSlots; i++)
            {
                GameObject newEmptyCard = Instantiate(emptySquadDisplayCardMenuPrefab, reserveUnitsParent);
                newEmptyCard.name = $"Empty Squad Card {i}";
                emptySquadCards.Add(newEmptyCard);
            }
        }
        public void MarkUnitAsJustUsedConsumable(int _unitIndex)
        {
            for (int i = 0; i < playerSquadsCards.Count; i++)
            {
                if (playerSquadsCards[i].SquadId == _unitIndex)
                {
                    playerSquadsCards[i].UseConsumable();
                    break;
                }
            }
        }
        public void ShowHoveredNodeText(NodeType nodeType, bool surprise, bool _hover)
        {
            if (surprise) unknownLabel.HoverUI(_hover);
            else
            {
                switch (nodeType)
                {
                    case NodeType.Skirmish:
                        skirmishLabel.HoverUI(_hover);
                        break;
                    case NodeType.Event:
                        eventLabel.HoverUI(_hover);
                        break;
                    case NodeType.Shop:
                        shopLabel.HoverUI(_hover);
                        break;
                    case NodeType.Town:
                        townLabel.HoverUI(_hover);
                        break;
                    // case NodeType.Warband:
                    //     unknownLabel.HoverUI(_hover);
                    //     break;
                    case NodeType.Treasure:
                        treasureLabel.HoverUI(_hover);
                        break;
                    case NodeType.Games:
                        if (tavernLabel != null) tavernLabel.HoverUI(_hover);
                        break;
                    case NodeType.Campfire:
                        if (campfireLabel != null) campfireLabel.HoverUI(_hover);
                        break;
                }
            }
        }
        public void DisplayJuiceOnSquad(ArmyJuice _armyJuice)
        {
            for (int i = 0; i < playerSquadsCards.Count; i++)
            {
                if (playerSquadsCards[i].UniqueID == _armyJuice.uniqueID)
                {
                    if (_armyJuice.armyJuiceEnum == ArmyJuiceEnum.Health)
                    {
                        Debug.Log($"DisplayJuiceOnSquad({playerSquadsCards[i].UniqueID}) - {_armyJuice.value}");
                        playerSquadsCards[i].ShowHealthRecoveryJuice(_armyJuice.value);
                    }
                    else if (_armyJuice.armyJuiceEnum == ArmyJuiceEnum.Prestige)
                    {
                        playerSquadsCards[i].ShowPrestigeJuice(playerSquadsCards[i].SquadPrestige);
                    }
                    else if (_armyJuice.armyJuiceEnum == ArmyJuiceEnum.SpawnIn)
                    {
                        playerSquadsCards[i].SpawnInJuice(true);
                    }
                }
            }
        }
    }
}