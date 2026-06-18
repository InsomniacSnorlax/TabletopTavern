using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Memori.Utilities;
using UnityEngine.UI;
using Memori.Scenes;
using TJ.Settings;
using Memori.SaveData;
using Memori.Notifications;
using Memori.Audio;
using Memori.Input;
using Memori.Localization;
using System;
using Memori.Core;
using Memori.UI;

namespace TJ
{
    public class SettingsManager : Memori.Utilities.Singleton<SettingsManager>
    {
        [SerializeField] private MemoriCanvasGroup settingsCanvasGroup;

        [Header("Main Buttons")]
        [SerializeField] private Button resumeGameButton;
        [SerializeField] private Button exitToMenuButton, exitToDesktopButton, abandonRunButton, quickRestartButton, creditsButton, concedeDefeatButton;

        [Header("Abandon Run")]
        [SerializeField] private MemoriCanvasGroup abandonRunConfirmationCanvasGroup;
        [SerializeField] private AbandonRunButton abandonRunConfirmationButton;
        [SerializeField] private Button abandonRunCancelButton;

        [Header("Quick Restart")]
        [SerializeField] private MemoriCanvasGroup quickRestartConfirmationCanvasGroup;
        [SerializeField] private Button quickRestartConfirmationButton, quickRestartCancelButton;

        [Header("Concede Defeat")]
        [SerializeField] private MemoriCanvasGroup concedeDefeatConfirmationCanvasGroup;
        [SerializeField] private Button concedeDefeatConfirmationButton, concedeDefeatCancelButton;

        [Header("Settings")]
        [SerializeField] private Button infoButton;
        [SerializeField] private Button gameSettingsButton, audioSettingsButton, graphicsSettingsButton, controlsSettingsButton;
        [SerializeField] private MemoriCanvasGroup infoCanvasGroup, gameSettingsCanvasGroup, audioSettingsCanvasGroup, graphicsSettingsCanvasGroup, controlsSettingsCanvasGroup, creditsCanvasGroup;
        [SerializeField] private SettingsToggleV2 disbandConfirmationToggle;
        [SerializeField] private SettingsToggleV2 hideUnitInfoInBattleToggle;
        [SerializeField] private SettingsToggleV2 cameraShakeToggle;
        [SerializeField] private SettingsToggleV2 autoRollInitiativeToggle;
        [SerializeField] private MemoriButtonV2 resetTutorialButton;
        public Action<bool> OnSettingsPanelToggled;

        public MonitoredData<bool> HideSquadInfoInBattle = new();
        public MonitoredData<bool> CameraShakeEnabled = new();
        public MonitoredData<bool> AutoRollInitiative = new();

        public MonitoredData<float> CameraRotationSpeed;
        public MonitoredData<float> CameraMovementSpeed;

        [SerializeField] private MonitoredDataSlider cameraRotationSpeedSlider;
        [SerializeField] private MonitoredDataSlider cameraMovementSpeedSlider;

        MemoriCanvasGroup activeCanvasGroup;
        bool inBattle;
        float cachedTimeValue;

        public bool SettingsPanelOpen => settingsCanvasGroup.canvasGroup.alpha == 1;
        private void Start()
        {
            settingsCanvasGroup.CGDisable();
            resumeGameButton.onClick.AddListener(CloseSettingsPanel);
            exitToMenuButton.onClick.AddListener(ExitToMenu);
            exitToDesktopButton.onClick.AddListener(ExitToDesktop);

            abandonRunButton.onClick.AddListener(AbandonRunConfirmationPopUp);
            abandonRunConfirmationButton.SetUp(this);
            abandonRunCancelButton.onClick.AddListener(CancelAbandonRun);
            abandonRunConfirmationCanvasGroup.CGDisable();

            quickRestartButton.onClick.AddListener(QuickRestartConfirmationPopUp);
            quickRestartConfirmationCanvasGroup.CGDisable();
            quickRestartConfirmationButton.onClick.AddListener(QuickRestart);
            quickRestartCancelButton.onClick.AddListener(CancelQuickRestart);

            concedeDefeatButton.onClick.AddListener(ConcedeDefeatConfirmationPopUp);
            concedeDefeatConfirmationCanvasGroup.CGDisable();
            concedeDefeatConfirmationButton.onClick.AddListener(ConcedeDefeat);
            concedeDefeatCancelButton.onClick.AddListener(CancelConcedeDefeat);

            activeCanvasGroup = gameSettingsCanvasGroup;
            gameSettingsCanvasGroup.CGEnable();

            infoButton.onClick.RemoveAllListeners();
            gameSettingsButton.onClick.RemoveAllListeners();
            audioSettingsButton.onClick.RemoveAllListeners();
            graphicsSettingsButton.onClick.RemoveAllListeners();
            controlsSettingsButton.onClick.RemoveAllListeners();
            creditsButton.onClick.RemoveAllListeners();
            resetTutorialButton.Button.onClick.RemoveAllListeners();
            
            infoButton.onClick.AddListener(() => SwitchSettingsFocus(infoCanvasGroup));
            gameSettingsButton.onClick.AddListener(() => SwitchSettingsFocus(gameSettingsCanvasGroup));
            audioSettingsButton.onClick.AddListener(() => SwitchSettingsFocus(audioSettingsCanvasGroup));
            graphicsSettingsButton.onClick.AddListener(() => SwitchSettingsFocus(graphicsSettingsCanvasGroup));
            controlsSettingsButton.onClick.AddListener(() => SwitchSettingsFocus(controlsSettingsCanvasGroup));
            creditsButton.onClick.AddListener(() => SwitchSettingsFocus(creditsCanvasGroup));
            resetTutorialButton.Button.onClick.AddListener(() => ResetTutorial());

            SceneHandler.Instance.OnGameStateChanged += OnGameStateChanged;
            InputHandler.Instance.SettingsButtonPressed += SettingsButtonPressed;
            infoCanvasGroup.gameObject.SetActive(false);
            hideUnitInfoInBattleToggle.OnToggle.onValueChanged.AddListener(SetHideSquadInfoInBattle);
            CameraShakeEnabled.Value = cameraShakeToggle.OnToggle.isOn;
            cameraShakeToggle.OnToggle.onValueChanged.AddListener(val => CameraShakeEnabled.Value = val);

            AutoRollInitiative.Value = autoRollInitiativeToggle.OnToggle.isOn;
            autoRollInitiativeToggle.OnToggle.onValueChanged.AddListener(val => AutoRollInitiative.Value = val);

            CameraRotationSpeed.Value = PlayerPrefs.GetFloat("cameraRotationSpeed", 0.5f);
            CameraMovementSpeed.Value = PlayerPrefs.GetFloat("cameraMovementSpeed", 0.5f);
            cameraRotationSpeedSlider.AssignMonitoredData(CameraRotationSpeed);
            cameraMovementSpeedSlider.AssignMonitoredData(CameraMovementSpeed);
        }
        private void SettingsButtonPressed()
        {
            // Debug.Log($"SettingsManager.SettingsButtonPressed()");
            if(settingsCanvasGroup.canvasGroup.alpha == 1) {
                CloseSettingsPanel();
            } else {
                OpenSettingsPanel();
            }
        }
        public void OpenSettingsPanel()
        {
            SwitchSettingsFocus(gameSettingsCanvasGroup);
            infoCanvasGroup.gameObject.SetActive(true);
            // disbandConfirmationToggle.OverrideToggleFromSettings();
            settingsCanvasGroup.CGEnable();
            IAudioRequester.Instance.PlaySFX(SFXData.OpenUI);
            if(inBattle) {
                cachedTimeValue = Time.timeScale;
                Time.timeScale = 0;
            }
            OnSettingsPanelToggled?.Invoke(true);
        }
        public void CloseSettingsPanel()
        {
            abandonRunConfirmationCanvasGroup.CGDisable();
            settingsCanvasGroup.CGDisable();
            infoCanvasGroup.gameObject.SetActive(false);
            IAudioRequester.Instance.PlaySFX(SFXData.CloseUI);
            if (inBattle)
            {
                Time.timeScale = cachedTimeValue;
            }
            OnSettingsPanelToggled?.Invoke(false);
        }
        public void AbandonRunConfirmationPopUp()
        {
            abandonRunConfirmationCanvasGroup.CGEnable();
        }
        public void AbandonRun()
        {
            abandonRunConfirmationCanvasGroup.CGDisable();
            ExitToMenu();
        }
        public void CancelAbandonRun()
        {
            abandonRunConfirmationCanvasGroup.CGDisable();
        }
        public void QuickRestartConfirmationPopUp()
        {
            quickRestartConfirmationCanvasGroup.CGEnable();
        }
        public void QuickRestart()
        {
            CampaignSaveManager campaignSaveManager = FindFirstObjectByType<CampaignSaveManager>();
            campaignSaveManager.QuickRestartCampaign();
            quickRestartConfirmationCanvasGroup.CGDisable();
            CloseSettingsPanel();
            SceneHandler.Instance.RequestQuickRestart();
            SceneHandler.Instance.RequestSceneCleanUpFunction(GameStateEnum.MainMenu);
        }
        public void CancelQuickRestart()
        {
            quickRestartConfirmationCanvasGroup.CGDisable();
        }
        public void ExitToMenu()
        {
            CloseSettingsPanel();
            SceneHandler.Instance.RequestSceneCleanUpFunction(GameStateEnum.MainMenu);
        }
        public void ConcedeDefeatConfirmationPopUp()
        {
            concedeDefeatConfirmationCanvasGroup.CGEnable();
        }
        public void ConcedeDefeat()
        {
            concedeDefeatConfirmationCanvasGroup.CGDisable();
            CloseSettingsPanel();
            BattleManager.Instance.ConcedeDefeat();
        }
        public void CancelConcedeDefeat()
        {
            concedeDefeatConfirmationCanvasGroup.CGDisable();
        }
        public void ExitToDesktop()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        public void SwitchSettingsFocus(MemoriCanvasGroup _canvasGroup)
        {
            if(activeCanvasGroup == _canvasGroup) return;

            activeCanvasGroup.CGDisable();
            _canvasGroup.CGEnable();

            activeCanvasGroup = _canvasGroup;
        }
        private void OnGameStateChanged(GameStateEnum gameStateEnum)
        {
            // Debug.Log($"SettingsManager.OnGameStateChanged({gameStateEnum})");
            if(gameStateEnum.Equals(GameStateEnum.MainMenu)) {
                abandonRunButton.gameObject.SetActive(false);
                quickRestartButton.gameObject.SetActive(false);
                exitToMenuButton.gameObject.SetActive(false);
                concedeDefeatButton.gameObject.SetActive(false);
            } else if(gameStateEnum.Equals(GameStateEnum.Map)) {
                abandonRunButton.gameObject.SetActive(true);
                exitToMenuButton.gameObject.SetActive(true);
                quickRestartButton.gameObject.SetActive(true);
                concedeDefeatButton.gameObject.SetActive(false);
            } else if(gameStateEnum.Equals(GameStateEnum.Battle)) {
                bool IsCustomBattle = SaveDataHandler.LoadPlayerSaveData().customBattle;
                abandonRunButton.gameObject.SetActive(!IsCustomBattle);
                quickRestartButton.gameObject.SetActive(false);
                exitToMenuButton.gameObject.SetActive(false);
                concedeDefeatButton.gameObject.SetActive(true);
            }
            inBattle = gameStateEnum.Equals(GameStateEnum.Battle);
        }
        private void ResetTutorial()
        {
            PlayerSaveData saveData = SaveDataHandler.LoadPlayerSaveData();
            saveData.tutorialStepCompleted.Clear();
            saveData.BattlefieldInfoSectionsViewed.Clear();
            SaveDataHandler.SavePlayerSaveData(saveData);

            string notificationText = LocalizationManager.Instance.GetText("tutorialprogressreset");
            NotificationManager.Instance.DisplayNotification(notificationText);

            PlayerPrefs.SetInt("battleTutorial", 0);
        }
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                Cursor.lockState = Screen.fullScreen ? CursorLockMode.Confined : CursorLockMode.None;
        }

        public void OnDestroy()
        {
            if (SceneHandler.Instance != null)
            {
                SceneHandler.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.SettingsButtonPressed -= SettingsButtonPressed;
            }
        }
        private void SetHideSquadInfoInBattle(bool isOn)
        {
            HideSquadInfoInBattle.Value = isOn;
            // Debug.Log($"Setting HideSquadInfoInBattle set to {HideSquadInfoInBattle.Value}");
        }
    }
}
