using Memori.Input;
using UnityEngine;
using Memori.Utilities;
using Unity.Entities;

namespace TJ
{
    public class GameSpeedManager : MonoBehaviour
    {
        [SerializeField] private GameSpeedButton pauseButton, slowButton, normalButton, fastButton;
        GameSpeedButton[] gameSpeedButtons;
        private void Start()
        {
            gameSpeedButtons = new GameSpeedButton[] { pauseButton, slowButton, normalButton, fastButton };
            pauseButton.SetUpGameSpeedButton(this, 0);
            slowButton.SetUpGameSpeedButton(this, 0.5f);
            normalButton.SetUpGameSpeedButton(this, 1);
            fastButton.SetUpGameSpeedButton(this, 3);
            SetTimeScale(normalButton);
            InputHandler.Instance.PauseButtonPressed += PauseGame;
        }

        public void PauseGame()
        {
            if (BattleManager.Instance.GamePhase != GamePhase.Battle) return;

            if (FindFirstObjectByType<ReportABugScreen>().GetComponent<CanvasGroup>().interactable) return;

            if (Time.timeScale == 0) {
                Debug.Log("Battle unpaused.");
                SetTimeScale(normalButton);
            } else {
                Debug.Log("Battle paused.");
                SetTimeScale(pauseButton);
            }
        }
        public void SetTimeScale(GameSpeedButton _gameSpeedButton)
        {
            foreach (var button in gameSpeedButtons) {
                if (button == _gameSpeedButton) {
                    button.Select();
                } else {
                    button.Deselect();
                }
            }
            Time.timeScale = _gameSpeedButton.GameSpeed;
            var defaultWorld = World.DefaultGameObjectInjectionWorld;
            var simulationSystemGroup = defaultWorld.GetExistingSystemManaged<SimulationSystemGroup>();
            var initializationSystemGroup = defaultWorld.GetExistingSystemManaged<InitializationSystemGroup>();
            simulationSystemGroup.Enabled = Time.timeScale != 0;
            initializationSystemGroup.Enabled = Time.timeScale != 0;
        }
        public void OnDestroy()
        {
            InputHandler.Instance.PauseButtonPressed -= PauseGame;
        }
        public void LockEndOfBattleSpeed()
        {
            Time.timeScale = 1f;
            var defaultWorld = World.DefaultGameObjectInjectionWorld;
            var simulationSystemGroup = defaultWorld.GetExistingSystemManaged<SimulationSystemGroup>();
            var initializationSystemGroup = defaultWorld.GetExistingSystemManaged<InitializationSystemGroup>();
            simulationSystemGroup.Enabled = false;
            initializationSystemGroup.Enabled = false;
            pauseButton.Lock();
            slowButton.Lock();
            fastButton.Lock();
            normalButton.Lock();
        }
    }
}