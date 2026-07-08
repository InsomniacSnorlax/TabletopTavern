using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Memori.SaveData;
using Memori.Tooltip;
using TJ.Map;
using Memori.Audio;
using MoreMountains.Feedbacks;
using System.Threading.Tasks;
using Memori.Localization;

namespace TJ
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class SquadDisplayCardMenu : SquadDisplayCard, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Projected Health Loss")]
        [SerializeField] private GameObject autoResolveProjectionObject;
        [SerializeField] private Slider redDifferenceInHealthSlider, greenFinalHealthSlider;

        [Header("Units Slain")]
        [SerializeField] private GameObject unitsSlainGroup;
        [SerializeField] private TMP_Text unitsSlainText;

        [Header("Squad Options")]
        [SerializeField] private GameObject squadOptionsGroup;
        [SerializeField] private Button disbandButton, renameButton, mergeButton;
        [SerializeField] private PrestigeUnitButton prestigeUnitButton;
        [SerializeField] private GameObject prestigeGlow;
        [SerializeField] private MemoriTooltipTrigger renameUnitTooltipTrigger;
        [SerializeField] private MemoriTooltipTrigger disbandUnitTooltipTrigger;
        [SerializeField] private MemoriTooltipTrigger mergeUnitTooltipTrigger;

        [Header("Recovery")]
        [SerializeField] private TMP_Text healthChangedPopUpText;
        [SerializeField] private Slider healthRecoverySlider;
        [SerializeField] private TMP_Text healthRecoveryJuiceText;
        [SerializeField] private MMF_Player healthRecoveryJuiceMMF;

        [Header("Dead Squad")]
        [SerializeField] private GameObject deadObject;
        [SerializeField] private GameObject willDieObject;

        [Header("Juice")]
        [SerializeField] private MMF_Player spawnInJuice;
        [SerializeField] private MMF_Player prestigeJuice;
        [SerializeField] private Sprite tier2Sprite, tier3Sprite;
        [SerializeField] private Image prestigeImage;
        [SerializeField] private Transform displayParentScaleReset;

        [Header("Reordering Squad")]
        [SerializeField] private GameObject dummySquadCard;
        [SerializeField] private Canvas healthBarTextCanvas;

        GameObject cachedDummySquadCard;
        bool wasJustSelected;
        HUDPanel hudPanel;
        bool cachedShowOptions, cachedShowRenamePrestige, cachedShowMerge, cachedShowPrestige;
        bool isPrestigeAvailable;
        Canvas canvas;
        bool isEnemy;
        public Team CardTeam => isEnemy ? Team.Enemy : Team.Player;
        bool inReserve;
        public bool InReserve => inReserve;
        bool cachedOverDeployedArea, cachedOverReserveArea;
        int indexHovered;
        Vector3 initialPosition;
        LayoutElement layoutElement;
        GraphicRaycaster graphicRaycaster;
        bool isSpawning;
        int lastDummyIndex = -1;

        public void SetUp(SquadToLoad _squad, bool _inReserve, HUDPanel _hudPanel = null, bool _isEnemy = false)
        {
            isEnemy = _isEnemy;
            base.SetUp(_squad, _squad.UnitIndex);
            squad = _squad;
            hudPanel = _hudPanel;
            inReserve = _inReserve;
            unitType = TabletopTavernData.Instance.GetUnitTypeFromUnitName(squad.UnitName);
            _presenter.UnitCountText.text = TabletopTavernData.Instance.GetSquadCurrentUnitCount(squad);
            SetUpSliders();
            this.name = squad.UnitName.ToString() + " " + squad.UniqueID[..1];
            layoutElement = GetComponent<LayoutElement>();
            canvas = GetComponent<Canvas>();

            disbandButton.onClick.AddListener(() => hudPanel.AttemptDisbandSelectedSquads());
            renameButton.onClick.AddListener(RenameSquad);
            prestigeUnitButton.PrestigeButton.onClick.AddListener(PrestigeUnit);
            mergeButton.onClick.AddListener(() => hudPanel.MergeSelectedSquads());

            if (hudPanel != null && !isEnemy)
            {
                hudPanel.CheckForPrestigeAvailability(prestigeUnitButton, squad.UnitName, squad.UnitPrestige);
            }

            squadOptionsGroup.SetActive(false);
            autoResolveProjectionObject.SetActive(false);
            unitsSlainGroup.SetActive(false);
            willDieObject.SetActive(squad.SquadCurrentHealth == 0);
            graphicRaycaster = GetComponent<GraphicRaycaster>();

            SetUpToolTips();
        }
        public void LockCard(bool _lock)
        {
            if(_lock) {
                isLocked = true;
            } else {
                isLocked = false;
            }
        }
        private int GetHealthRecovery()
        {
            int healthRecovery = (int)(squad.SquadMaxHealth * TabletopTavernConstants.RESERVES_HEAL_AMOUNT);
            if(CampaignManager.HasInstance) healthRecovery *= CampaignManager.Instance.CampaignSaveManager.ReservesHealMultiplier;
            if(CampaignManager.HasInstance && CampaignManager.Instance.GearManager.CheckForGear(GearID.ChugJug)) healthRecovery*=2;
            return healthRecovery;
        }
        public override void SelectSquadButtonClicked()
        {
            if (wasJustSelected) {
                wasJustSelected = false;
                return;
            }

            if (isLocked) return;

            if (squad.SquadCurrentHealth == 0) {
                //check if player squad or enemy squad
                if (isEnemy) {
                    Debug.Log("Enemy squad is dead, cannot select.");
                    return;
                }
                AttemptDisbandSquad();
                return;
            }

            if (!CampaignManager.HasInstance) return;

            CampaignManager.Instance.MapSceneUIManager.HUDPanel.CloseNonSquadPopUps();

            bool modifierHeld = Input.GetKey(KeyCode.LeftControl)
                             || Input.GetKey(KeyCode.RightControl)
                             || Input.GetKey(KeyCode.LeftShift)
                             || Input.GetKey(KeyCode.RightShift);

            if (modifierHeld)
                CampaignManager.Instance.MapSceneUIManager.HUDPanel.ToggleCardInSelection(this);
            else
                CampaignManager.Instance.MapSceneUIManager.HUDPanel.SelectSingleCard(isSelected ? null : this);
        }
        public override void SelectSquad(bool _selected)
        {
            base.SelectSquad(_selected);
            canvas.sortingOrder = _selected ? 4 : 2;
        }
        public void SetOptionsVisibility(bool showOptions, bool showRenamePrestige, bool showMerge = false, bool showPrestige = false)
        {
            cachedShowOptions = showOptions;
            cachedShowRenamePrestige = showRenamePrestige;
            cachedShowMerge = showMerge;
            cachedShowPrestige = showPrestige;
            squadOptionsGroup.SetActive(showOptions && isPointerOver);
            if (!showOptions) return;
            disbandButton.gameObject.SetActive(true);
            renameButton.gameObject.SetActive(showRenamePrestige);
            prestigeUnitButton.gameObject.SetActive((showRenamePrestige && isPrestigeAvailable) || showPrestige);
            mergeButton.gameObject.SetActive(showMerge);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            isPointerOver = true;
            HoverHighlight(true);
            if (isLocked) return;

            if(isSpawning) return;

            base.OnPointerEnter(eventData);
            _presenter.OnHoverEnterFeedback.PlayFeedbacks();

            if (cachedShowOptions)
            {
                squadOptionsGroup.SetActive(true);
                disbandButton.gameObject.SetActive(true);
                renameButton.gameObject.SetActive(cachedShowRenamePrestige);
                prestigeUnitButton.gameObject.SetActive((cachedShowRenamePrestige && isPrestigeAvailable) || cachedShowPrestige);
                mergeButton.gameObject.SetActive(cachedShowMerge);
            }

            if (!CampaignManager.HasInstance) return;
            CampaignManager.Instance.MapSceneUIManager.HUDPanel.HoverSquad(squad, true, this.transform);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
            squadOptionsGroup.SetActive(false);
            HoverHighlight(false);
            if (isLocked) return;

            if(isSpawning) return;

            base.OnPointerExit(eventData);
            if (!CampaignManager.HasInstance) return;
            CampaignManager.Instance.MapSceneUIManager.HUDPanel.HoverSquad(squad, false, this.transform);
        }
        
        #region healthShit
        private void SetUpSliders()
        {
            redDifferenceInHealthSlider.maxValue = squad.SquadMaxHealth;
            greenFinalHealthSlider.maxValue = squad.SquadMaxHealth;
            healthRecoverySlider.maxValue = squad.SquadMaxHealth;
        }
        public void ShowPotentialHealthLoss(SquadToLoad _squad)
        {
            _presenter.UnitCountText.text = TabletopTavernData.Instance.GetSquadCurrentUnitCount(_squad);
            redDifferenceInHealthSlider.value = squad.SquadCurrentHealth;
            greenFinalHealthSlider.value = _squad.SquadCurrentHealth;

            if(squad.SquadCurrentHealth !=0) {
                redDifferenceInHealthSlider.value = _presenter.HealthSlider.value;
                greenFinalHealthSlider.value = _squad.SquadCurrentHealth;
            }

            autoResolveProjectionObject.SetActive(true);
            willDieObject.SetActive(_squad.SquadCurrentHealth == 0);
        }
        public void ShowPotentialHealthRecovery()
        {
            int healtRecovery = squad.SquadCurrentHealth + GetHealthRecovery();
            healthRecoverySlider.value = Mathf.Min(healtRecovery, squad.SquadMaxHealth);
            int unitCount = (int)(healthRecoverySlider.value / squad.HitPointsPerUnit);
            if (healthRecoverySlider.value > 0 && unitCount == 0) unitCount = 1;
            _presenter.UnitCountText.text = unitCount.ToString();
        }
        public void HidePotentialHealthLoss()
        {
            _presenter.UnitCountText.text = TabletopTavernData.Instance.GetSquadCurrentUnitCount(squad);
            autoResolveProjectionObject.SetActive(false);
            willDieObject.SetActive(false);
        }
        public void HidePotentialHealthRecovery()
        {
            _presenter.UnitCountText.text = TabletopTavernData.Instance.GetSquadCurrentUnitCount(squad);
            healthRecoverySlider.value = 0;
        }
        public void ShowUnitsSlain(int _unitsSlain)
        {
            unitsSlainText.text = _unitsSlain.ToString();
            unitsSlainGroup.SetActive(true);
            hudPanel.CheckForPrestigeAvailability(prestigeUnitButton, squad.UnitName, squad.UnitPrestige);
        }
        public void UpdateUnitCount(SquadToLoad _squad)
        {
            _presenter.HealthSlider.value = _squad.SquadCurrentHealth;
            int unitCountOfSquad = int.Parse(TabletopTavernData.Instance.GetSquadCurrentUnitCount(_squad));
            _presenter.UnitCountText.text = unitCountOfSquad.ToString();
            int changeInUnitCount = unitCountOfSquad - int.Parse(TabletopTavernData.Instance.GetSquadCurrentUnitCount(squad));
            // Debug.Log($"changeInUnitCount: {changeInUnitCount}");
            squad = _squad;
            if(changeInUnitCount == 0) return;

            ShowHealthRecoveryJuice(changeInUnitCount);
        }
        public void ShowHealthRecoveryJuice(int changeInUnitCount)
        {
            // Debug.Log($"ShowHealthRecoveryJuice({changeInUnitCount})");
            if (inReserve)
            {
                healthChangedPopUpText.text = "+" + changeInUnitCount.ToString();
            }
            else
            {
                healthChangedPopUpText.text = "";
            }

            healthRecoveryJuiceText.text = (changeInUnitCount > 0 ? "+" : "" ) + changeInUnitCount.ToString();
            healthRecoveryJuiceText.color = changeInUnitCount > 0 ? Color.green : Color.red;

            healthRecoveryJuiceMMF.StopFeedbacks();
            healthRecoveryJuiceMMF.PlayFeedbacks();
        }
        #endregion
        public void ShowPrestigeJuice(int _prestigeLevel)
        {
            spawnInJuice.PlayFeedbacks();
            prestigeImage.sprite = _prestigeLevel == 1 ? tier2Sprite : tier3Sprite;
            prestigeJuice.PlayFeedbacks();
        }
        public void HideUnitsSlain()
        {
            if (unitsSlainGroup != null)
                unitsSlainGroup.SetActive(false);
        }
        public void AttemptDisbandSquad()
        {
            TutorialManager.Instance.CompleteStepCheck(TutorialStepEnum.DisbandUnit);
            OnPointerExit(null);
            TooltipManager.Instance.HideTooltip();

            if(PlayerPrefs.GetInt("DisbandSquadConfirmation", 0) == 1) {
                hudPanel.DisbandSquad(squad.UniqueID);
            } else {
                hudPanel.ShowDisbandSquadConfirmation(squad.UniqueID);
                IAudioRequester.Instance.PlaySFX(SFXData.DisbandSquad);
            }
        }
        public void RenameSquad()
        {
            hudPanel.GiveRenameSquadPrompt(squad.UniqueID);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isEnemy) return;
            
            // Store the initial position

            initialPosition = transform.position;

            bool isDeployedGroup = squad.UnitIndex < 10;
            cachedOverDeployedArea = isDeployedGroup;
            cachedOverReserveArea = !isDeployedGroup;

            lastDummyIndex = -1;
            cachedDummySquadCard = Instantiate(dummySquadCard, transform.position, Quaternion.identity);
            cachedDummySquadCard.transform.SetParent(transform.parent);
            cachedDummySquadCard.transform.SetSiblingIndex(transform.GetSiblingIndex());

            transform.SetSiblingIndex(transform.parent.transform.childCount - 1);

            layoutElement.ignoreLayout = true;
            canvas.sortingOrder = 102;
            healthBarTextCanvas.sortingOrder = 103;

            IAudioRequester.Instance.PlaySFX(SFXData.SquadHovered);

            hudPanel.LockCards(true);
            OnPointerEnter(null);
            hudPanel.DestroyEmptySquadCards();
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (isEnemy) return;
            
            // Move the image with the mouse
            transform.position = eventData.position;
            initialPosition = cachedDummySquadCard.transform.position;


            bool isOverDropArea1 = RectTransformUtility.RectangleContainsScreenPoint(hudPanel.DeployedTroopsArea, eventData.position);
            bool isOverDropArea2 = RectTransformUtility.RectangleContainsScreenPoint(hudPanel.ReserveTroopsArea, eventData.position);

            if (isOverDropArea1 && !cachedOverDeployedArea) //switching from reserves to deployed
            {
                hudPanel.HighlightDeployedTroopsArea(true);
                hudPanel.HighlightReserveTroopsArea(false);
                cachedOverDeployedArea = true;
                cachedOverReserveArea = false;

                if (transform.parent != hudPanel.DeployedUnitsParent)
                {
                    int realDeployedCount = 0;
                    for (int i = 0; i < hudPanel.DeployedUnitsParent.childCount; i++)
                        if (hudPanel.DeployedUnitsParent.GetChild(i).GetComponent<SquadDisplayCard>() != null) realDeployedCount++;
                    if (realDeployedCount >= 10) {
                        Transform squadToSwapOut = null;
                        for (int i = hudPanel.DeployedUnitsParent.childCount - 1; i >= 0; i--) {
                            Transform child = hudPanel.DeployedUnitsParent.GetChild(i);
                            if (child.GetComponent<SquadDisplayCard>() != null) { squadToSwapOut = child; break; }
                        }
                        if (squadToSwapOut != null) {
                            squadToSwapOut.SetParent(hudPanel.ReserveUnitsParent);
                            squadToSwapOut.SetSiblingIndex(squadToSwapOut.parent.childCount - 2);
                        }
                    }

                    //go through all the children of the parent and look for the first child that is not a squad display card and then move it to reserves
                    for(int i = 0; i < hudPanel.DeployedUnitsParent.childCount; i++) {
                        if(hudPanel.DeployedUnitsParent.GetChild(i).GetComponent<SquadDisplayCard>() == null) {
                            // Debug.Log($"Moving to Deployed: {hudPanel.DeployedUnitsParent.GetChild(i).name}");
                            hudPanel.DeployedUnitsParent.GetChild(i).transform.SetParent(hudPanel.ReserveUnitsParent);
                            break;
                        }
                    }

                    cachedDummySquadCard.transform.SetParent(hudPanel.DeployedUnitsParent);
                    transform.SetParent(hudPanel.DeployedUnitsParent);
                }
            }
            else if (isOverDropArea2 && !cachedOverReserveArea) //switching from deployed to reserves
            {
                hudPanel.HighlightReserveTroopsArea(true);
                hudPanel.HighlightDeployedTroopsArea(false);
                cachedOverReserveArea = true;
                cachedOverDeployedArea = false;

                if (transform.parent != hudPanel.ReserveUnitsParent)
                {
                    int realReserveCount = 0;
                    for (int i = 0; i < hudPanel.ReserveUnitsParent.childCount; i++)
                        if (hudPanel.ReserveUnitsParent.GetChild(i).GetComponent<SquadDisplayCard>() != null) realReserveCount++;
                    if (realReserveCount >= hudPanel.MaxReserveSlots) {
                        Transform squadToSwapOut = null;
                        for (int i = 0; i < hudPanel.ReserveUnitsParent.childCount; i++) {
                            Transform child = hudPanel.ReserveUnitsParent.GetChild(i);
                            if (child.GetComponent<SquadDisplayCard>() != null) { squadToSwapOut = child; break; }
                        }
                        if (squadToSwapOut != null) {
                            int lastSquadDeployedSiblingIndex = 0;
                            for(int i = 0; i < hudPanel.DeployedUnitsParent.childCount; i++) {
                                SquadDisplayCard squadDisplayCard = hudPanel.DeployedUnitsParent.GetChild(i).GetComponent<SquadDisplayCard>();
                                if(squadDisplayCard == null) continue;
                                if(squadDisplayCard == this) continue;
                                lastSquadDeployedSiblingIndex = i;
                            }
                            squadToSwapOut.SetParent(hudPanel.DeployedUnitsParent);
                            squadToSwapOut.SetSiblingIndex(lastSquadDeployedSiblingIndex + 1);
                        }
                    }

                    //go through all the children of the parent and look for the first child that is not a squad display card and then move it to reserves
                    for(int i = 0; i < hudPanel.ReserveUnitsParent.childCount; i++) {
                        if(hudPanel.ReserveUnitsParent.GetChild(i).GetComponent<SquadDisplayCard>() == null) {
                            // Debug.Log($"Moving to Reserve: {hudPanel.ReserveUnitsParent.GetChild(i).name}");
                            hudPanel.ReserveUnitsParent.GetChild(i).transform.SetParent(hudPanel.DeployedUnitsParent);
                            break;
                        }
                    }
                    cachedDummySquadCard.transform.SetParent(hudPanel.ReserveUnitsParent);
                    cachedDummySquadCard.transform.SetSiblingIndex(0);
                    transform.SetParent(hudPanel.ReserveUnitsParent);
                }
            }
            else if (!isOverDropArea1 && cachedOverDeployedArea)
            {
                hudPanel.HighlightDeployedTroopsArea(false);
                cachedOverDeployedArea = false;
            }
            else if (!isOverDropArea2 && cachedOverReserveArea)
            {
                hudPanel.HighlightReserveTroopsArea(false);
                cachedOverReserveArea = false;
            }

            for(int i = 0; i < hudPanel.TroopsIndexAreas.Length; i++) {
                if(RectTransformUtility.RectangleContainsScreenPoint(hudPanel.TroopsIndexAreas[i], eventData.position)) {
                //    Debug.Log($"TroopsIndexAreas: {i}");
                    indexHovered = i;
                }
            }

            float distanceInX = initialPosition.x - transform.position.x;
            // Debug.Log($"Distance in X: {distanceInX}");

            if(distanceInX < -60)
            {
                int index = cachedDummySquadCard.transform.GetSiblingIndex();

                if (index < transform.parent.childCount) {
                    cachedDummySquadCard.transform.SetSiblingIndex(index+1);
                    initialPosition = transform.position;

                    if (lastDummyIndex != index + 1) {
                        lastDummyIndex = index + 1;
                        IAudioRequester.Instance.PlaySFX(SFXData.LightMouseOver);
                    }
                }
            }

            if (distanceInX > 60)
            {
                int index = cachedDummySquadCard.transform.GetSiblingIndex();

                if(index < 1) return;

                if (index >= 1) {
                    cachedDummySquadCard.transform.SetSiblingIndex(index-1);
                    initialPosition = transform.position;

                    if (lastDummyIndex != index - 1) {
                        lastDummyIndex = index - 1;
                        IAudioRequester.Instance.PlaySFX(SFXData.LightMouseOver);
                    }
                }
            }
        }
        public async void OnEndDrag(PointerEventData eventData)
        {
            if (isEnemy) return;

            await Task.Delay(1);//wait for the end of the frame to reorder the units

            transform.position = cachedDummySquadCard.transform.position;
            transform.SetSiblingIndex(cachedDummySquadCard.transform.GetSiblingIndex());
            if(layoutElement != null)
                layoutElement.ignoreLayout = false;

            for(int i = 0; i < hudPanel.DeployedUnitsParent.childCount; i++) { // move all the empty slots to the end
                if(hudPanel.DeployedUnitsParent.GetChild(i).GetComponent<SquadDisplayCard>() == null &&
                    hudPanel.DeployedUnitsParent.GetChild(i).gameObject != cachedDummySquadCard) {
                    hudPanel.DeployedUnitsParent.GetChild(i).transform.SetAsLastSibling();
                }
            }            
        
            for(int i = 0; i < hudPanel.ReserveUnitsParent.childCount; i++) { // move all the empty slots to the end
                if(hudPanel.ReserveUnitsParent.GetChild(i).GetComponent<SquadDisplayCard>() == null &&
                    hudPanel.ReserveUnitsParent.GetChild(i).gameObject != cachedDummySquadCard) {
                    hudPanel.ReserveUnitsParent.GetChild(i).transform.SetAsLastSibling();
                }
            }

            transform.position = initialPosition;
            cachedDummySquadCard.transform.SetParent(null); // remove from hierarchy immediately so ReorderUnits gets correct childCounts
            Destroy(cachedDummySquadCard);
            await Task.Delay(1);//wait for the end of the frame to reorder the units
            hudPanel.ReplaceEmptySquadCards();
            hudPanel.ReorderUnits();

            OnPointerExit(null);
            cachedOverDeployedArea = false;
            cachedOverReserveArea = false;
            hudPanel.LockCards(false);
            canvas.sortingOrder = 2;
            healthBarTextCanvas.sortingOrder = 101;
        }
        public void SpawnInJuice(bool makeInteractable)
        {
            if (spawnInJuice == null) return;

            isSpawning = true;

            // MakeInteractable(makeInteractable);
            spawnInJuice.PlayFeedbacks();
            IAudioRequester.Instance.PlaySFX(SFXData.SquadCardSpawn);
        }
        public void SpawnInComplete()
        {
            isSpawning = false;
            displayParentScaleReset.localScale = Vector3.one;
        }
        public void MakeInteractable(bool _isInteractable)
        {
            if(graphicRaycaster == null) return;
            graphicRaycaster.enabled = _isInteractable;
        }
        public void PrestigeUnit()
        {
            hudPanel.PrestigeUnit(squad.UniqueID);
            prestigeUnitButton.TooltipTrigger.OnPointerExit(null);
            TutorialManager.Instance.CompleteStepCheck(TutorialStepEnum.PrestigeUnit);
        }
        public void ShowPrestigeAvailability(bool _isAvailable)
        {
            isPrestigeAvailable = _isAvailable;
            prestigeGlow.SetActive(_isAvailable);
        }
        public void HideDeadSquads()
        {
            // Debug.Log($"squad.currentUnitCount: {squad.currentUnitCount}");
            if(squad.SquadCurrentHealth == 0) {
                deadObject.SetActive(true);
            }
        }
        private void SetUpToolTips()
        {
            string prestigeLocalised = LocalizationManager.Instance.GetText("Prestige");
            string prestigeTooltipLocalised = LocalizationManager.Instance.GetText("PrestigeTooltip");
            string unitTypeLocalised = TabletopTavernConstants.UsesMeleePrestige(squad.UnitName)
                ? LocalizationManager.Instance.GetText(squad.UnitName.ToString())
                : LocalizationManager.Instance.GetText(unitType.ToString());
            string unitsGainLocalised = LocalizationManager.Instance.GetText("UnitsGain");
            string meleeAttackLocalised = LocalizationManager.Instance.GetText("MeleeAttack");
            string meleeDefenseLocalised = LocalizationManager.Instance.GetText("MeleeDefense");
            string accuracyLocalised = LocalizationManager.Instance.GetText("Accuracy");
            string rangeLocalised = LocalizationManager.Instance.GetText("Range");
            string andLocalised = LocalizationManager.Instance.GetText("and");
            string perLevelLocalised = LocalizationManager.Instance.GetText("PerLevel");
            string mergeUnitsDescriptionLocalised = LocalizationManager.Instance.GetText("MergeUnitsDescription");

            if(unitType == UnitType.Melee || unitType == UnitType.Hybrid || TabletopTavernConstants.UsesMeleePrestige(squad.UnitName)) {
                prestigeTooltipLocalised += $"[{unitTypeLocalised}] {unitsGainLocalised} <color={ColorData.Green}>+{TabletopTavernConstants.PRESTIGE_BONUS}</color> <color={ColorData.UnitStat}>{meleeAttackLocalised}</color> {andLocalised} <color={ColorData.Green}>+{TabletopTavernConstants.PRESTIGE_BONUS}</color> <color={ColorData.UnitStat}>{meleeDefenseLocalised}</color> {perLevelLocalised}";
            } else if(unitType == UnitType.Ranged || unitType == UnitType.Artillery) {
                prestigeTooltipLocalised += $"[{unitTypeLocalised}] {unitsGainLocalised} <color={ColorData.Green}>+{TabletopTavernConstants.PRESTIGE_BONUS}</color> <color={ColorData.UnitStat}>{accuracyLocalised}</color> {andLocalised} <color={ColorData.Green}>+{TabletopTavernConstants.PRESTIGE_BONUS}</color> <color={ColorData.UnitStat}>{rangeLocalised}</color> {perLevelLocalised}";
            }

            string renameLocalised = LocalizationManager.Instance.GetText("Rename Unit");
            string disbandLocalised = LocalizationManager.Instance.GetText("Disband Unit");
            string mergeLocalised = LocalizationManager.Instance.GetText("Merge Units");

            prestigeUnitButton.TooltipTrigger.SetUpToolTip(prestigeLocalised, prestigeTooltipLocalised);
            renameUnitTooltipTrigger.SetUpToolTip(renameLocalised, "");
            disbandUnitTooltipTrigger.SetUpToolTip(disbandLocalised, "");
            mergeUnitTooltipTrigger.SetUpToolTip(mergeLocalised, mergeUnitsDescriptionLocalised);
        }
        public void UseConsumable()
        {
            wasJustSelected = true;
        }
        public SquadToLoad GetSquadToLoad()
        {
            return squad;
        }
        public override void OnDeselect(BaseEventData eventData)
        {
            // Your deselect logic
            base.OnDeselect(eventData);
            // squadOptionsGroup.SetActive(false);
        }
    }
}
