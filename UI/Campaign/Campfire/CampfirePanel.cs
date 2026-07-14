using System.Collections.Generic;
using System.Threading.Tasks;
using Memori.Audio;
using Memori.Localization;
using Memori.Notifications;
using Memori.Utilities;
using TMPro;
using TJ;
using TJ.Map;
using UnityEngine;
using UnityEngine.UI;
using Memori.SaveData;

namespace TJ.Campfire
{
    public class CampfirePanel : MapPanel
    {
        [Header("Options Panel")]
        [SerializeField] private MemoriCanvasGroup optionsPanel;
        [SerializeField] private Button restButton;
        [SerializeField] private Button trainButton;
        [SerializeField] private Button scavengeButton;
        [SerializeField] private Button scoutAheadButton;
        // [SerializeField] private Button viewMapButton;

        [Header("Map Overview")]
        [SerializeField] private MapOverviewPanel mapOverviewPanel;

        [Header("Result Panel")]
        [SerializeField] private MemoriCanvasGroup resultPanel;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultDescriptionText;
        [SerializeField] private Button continueButton;

        [Header("Extra Text")]
        [SerializeField] private TMP_Text scoutDescriptionText;
        [SerializeField] private TMP_Text campfireHealDescriptionText;
        [SerializeField] private TMP_Text campfireScavengeDescriptionText;
        private const float RestHealAmount = 0.3f;
        private const int GearScavengeCount = 3;

        private CampaignSaveManager campaignSaveManager;
        private MapSceneUIManager mapSceneUIManager;
        private MemoriCanvasGroup panelCanvasGroup;

        private void Awake()
        {
            panelCanvasGroup = GetComponent<MemoriCanvasGroup>();
            resultPanel.CGDisable();
        }

        public void SetUp(CampaignSaveManager _csm, MapSceneUIManager _msui)
        {
            campaignSaveManager = _csm;
            mapSceneUIManager = _msui;

            if (restButton != null) restButton.onClick.AddListener(OnRest);
            if (trainButton != null) trainButton.onClick.AddListener(OnTrain);
            if (scavengeButton != null) scavengeButton.onClick.AddListener(OnScavenge);
            if (scoutAheadButton != null) scoutAheadButton.onClick.AddListener(OnScoutAhead);
            // if (viewMapButton != null) viewMapButton.onClick.AddListener(OnViewMap);
            if (continueButton != null) continueButton.onClick.AddListener(() => mapSceneUIManager.TryDrainPendingPrestigeChoices(() => mapSceneUIManager.CompleteLayerAction()));
        }

        public void LoadCampfirePanel()
        {
            resultPanel.CGDisable();
            OpenFeedback.PlayFeedbacks();
            // Disable Train if all units are max prestige
            bool anyTrainable = false;
            foreach (var squad in campaignSaveManager.SaveData.playerArmy)
                if (squad.UnitIndex != -1 && squad.UnitPrestige < 2) { anyTrainable = true; break; }

            if (trainButton != null) trainButton.interactable = anyTrainable;

            SetOptionsVisible(true);
            panelCanvasGroup.FadeInAsync();
            IAudioRequester.Instance.PlaySFX(SFXData.OpenUI);

            scoutDescriptionText.text = LocalizationManager.Instance.GetText("Reveal all Future ? Paths") + "+\n" +
                LocalizationManager.Instance.GetText("heroBonusDescription17") + "\n";
            campfireHealDescriptionText.text = string.Format(LocalizationManager.Instance.GetText("campfireHeal"), (int)(RestHealAmount * 100));
            campfireScavengeDescriptionText.text = string.Format(LocalizationManager.Instance.GetText("campfireGear"), GearScavengeCount);
        }

        private void OnRest()
        {
            if (mapOverviewPanel != null) mapOverviewPanel.Close();
            campaignSaveManager.ModifyTroopHealth(RestHealAmount);
            CampaignManager.Instance.MapSceneUIManager.HUDPanel.ArmyStructureChanged();
            ShowResult(
                LocalizationManager.Instance.GetText("CampfireRest"),
                string.Format(LocalizationManager.Instance.GetText("CampfireRestDesc"), (int)(RestHealAmount * 100)));
        }

        private void OnTrain()
        {
            if (mapOverviewPanel != null) mapOverviewPanel.Close();
            List<SquadToLoad> eligible = new();
            foreach (var squad in campaignSaveManager.SaveData.playerArmy)
                if (squad.UnitIndex != -1 && squad.UnitPrestige < 2)
                    eligible.Add(squad);

            if (eligible.Count > 0)
            {
                System.Random random = campaignSaveManager.GetCampaignRandom();
                int idx = random.Next(0, eligible.Count);
                campaignSaveManager.PrestigeSpecificUnit(eligible[idx]);
                string unitName = LocalizationManager.Instance.GetText(eligible[idx].UnitName.ToString());
                ShowResult(
                    LocalizationManager.Instance.GetText("CampfireTrainSuccess"),
                    string.Format(LocalizationManager.Instance.GetText("CampfireTrainSuccessDesc"), unitName));
            }
            else
            {
                ShowResult(
                    LocalizationManager.Instance.GetText("CampfireTrainFail"),
                    LocalizationManager.Instance.GetText("CampfireTrainFailDesc"));
            }
        }

        private void OnScavenge()
        {
            if (!campaignSaveManager.CanAquireGear())
            {
                NotificationManager.Instance.ErrorNotification(LocalizationManager.Instance.GetText("No space for gear"));
                return;
            }
            if (mapOverviewPanel != null) mapOverviewPanel.Close();
            SetOptionsVisible(false);
            panelCanvasGroup.FadeOutAsync();
            mapSceneUIManager.SetActivePanel(mapSceneUIManager.TreasurePanel);
            mapSceneUIManager.TreasurePanel.LoadTreasurePanelFromMapNode(GearScavengeCount);
        }

        private void OnViewMap()
        {
            if (mapOverviewPanel == null) return;
            mapOverviewPanel.Open(
                mapSceneUIManager.MapSceneManager.MapLayers,
                CampaignManager.Instance.CampaignSaveManager.SaveData,
                mapSceneUIManager.LayerNodeSelected);
        }

        private void OnScoutAhead()
        {
            if (mapOverviewPanel != null) mapOverviewPanel.Close();
            int activeLayer = mapSceneUIManager.MapSceneManager.GetActiveChapterIndex();
            mapSceneUIManager.MapSceneManager.RevealNodesInNextLayers(activeLayer, 100);
            if (mapOverviewPanel != null) mapOverviewPanel.Refresh();

            if (!campaignSaveManager.HasRoomForConsumable())
            {
                NotificationManager.Instance.ErrorNotification(LocalizationManager.Instance.GetText("noRoomForConsumable"));
                ShowResult(
                    LocalizationManager.Instance.GetText("CampfireScoutAhead"),
                    LocalizationManager.Instance.GetText("CampfireScoutAheadDesc"));
                return;
            }

            int bookNumber = campaignSaveManager.SaveData.bookNumber;
            ConsumableEnum consumable = ConsumableData.GetWeightedConsumable(bookNumber, campaignSaveManager.GetSeededRandom());
            string consumableName = LocalizationManager.Instance.GetText(consumable.ToString() + "Name");
            campaignSaveManager.AquireConsumable(consumable);
            ShowResult(
                LocalizationManager.Instance.GetText("CampfireScoutAhead"),
                string.Format(LocalizationManager.Instance.GetText("CampfireScavengedDesc"), consumableName));
        }

        private void ShowResult(string title, string description)
        {
            IAudioRequester.Instance.PlaySFX(SFXData.ChoiceMade);
            SetOptionsVisible(false);
            if (resultTitleText != null) resultTitleText.text = title;
            if (resultDescriptionText != null) resultDescriptionText.text = description;
            if (resultPanel != null) resultPanel.CGEnable();
        }

        private void SetOptionsVisible(bool visible)
        {
            if (visible) optionsPanel.FadeInAsync();
            else optionsPanel.FadeOutAsync();
        }

        public override async void ClosePanel()
        {
            CloseFeedback();
            campaignSaveManager.RemoveZeroHealthSquads();
            await Task.Delay(200);
            panelCanvasGroup.FadeOutAsync();
        }
    }
}
