using UnityEngine;
using Memori.SaveData;
using TJ.Map;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TJ
{
    public class BattlefieldTutorial : MonoBehaviour
    {
        [System.Serializable] public struct BattlefieldInfoOverrideData
        {
            public GameObject Header;
            public GameObject Content;
            public string ContentDescription;
        }

        [SerializeField] private GameObject _battlefieldTutorialCanvas;
        [SerializeField] private Button _showAnotherTipButton, _returnToBattleButton;
        public bool TutorialIsOpen => _battlefieldTutorialCanvas.activeSelf;
        [SerializeField] private BattlefieldInfoOverrideData[] battlefieldInfoOverrideDataArray;

        public void HandleTutorialStuff()
        {
            _showAnotherTipButton.onClick.RemoveAllListeners();
            _showAnotherTipButton.onClick.AddListener(ShowAnotherTip);
            _returnToBattleButton.onClick.RemoveAllListeners();
            _returnToBattleButton.onClick.AddListener(ReturnToBattle);

            TutorialManager.Instance.LoadStepsFromRandomSpot(new TutorialStep[5] {
                TutorialData.SelectUnit,
                TutorialData.RepositionUnit,
                // TutorialData.GiveAttackOrders,
                TutorialData.SelectMultipleUnits,
                TutorialData.ChangeBattleSpeed,
                TutorialData.StartBattle});

            // TutorialManager.Instance.LoadTooltip(TutorialData.GuardMode, BattleManager.Instance.UIManager.GuardModeButtonTransform);
            if(CheckForUnseenTips())
            {
                _battlefieldTutorialCanvas.SetActive(true);
                OpenTip();
            }
        }

        public bool CheckForUnseenTips()
        {
            PlayerSaveData playerSaveData = SaveDataHandler.LoadPlayerSaveData();
            // for each override data, check to see if it is saved in the playerSaveData, if not, set the header and content to active and save it in the playerSaveData as seen
            foreach (BattlefieldInfoOverrideData overrideData in battlefieldInfoOverrideDataArray)
            {
                if (!playerSaveData.BattlefieldInfoSectionsViewed.Contains(overrideData.ContentDescription))
                {
                    return true;
                }
            }
           return false;
        }
        public void OpenTip()
        {
            PlayerSaveData playerSaveData = SaveDataHandler.LoadPlayerSaveData();
            // for each override data, check to see if it is saved in the playerSaveData, if not, set the header and content to active and save it in the playerSaveData as seen
            foreach (BattlefieldInfoOverrideData overrideData in battlefieldInfoOverrideDataArray)
            {
                if (!playerSaveData.BattlefieldInfoSectionsViewed.Contains(overrideData.ContentDescription))
                {
                    overrideData.Content.SetActive(true);
                    overrideData.Header.SetActive(true);
                    playerSaveData.BattlefieldInfoSectionsViewed.Add(overrideData.ContentDescription);
                    SaveDataHandler.SavePlayerSaveData(playerSaveData);
                    break;
                }
            }
            if(!CheckForUnseenTips())
            {
                _showAnotherTipButton.gameObject.SetActive(false);
            }
            EventSystem.current.SetSelectedGameObject(_returnToBattleButton.gameObject);
            _returnToBattleButton.GetComponent<Animator>().SetTrigger("Selected");
        }
        public void ShowAnotherTip()
        {
            foreach (BattlefieldInfoOverrideData overrideData in battlefieldInfoOverrideDataArray)
            {
                overrideData.Header.SetActive(false);
                overrideData.Content.SetActive(false);
            }
            OpenTip();
        }
        public void ReturnToBattle()
        {
            _battlefieldTutorialCanvas.SetActive(false);
        }
        [ContextMenu("Reset Battlefield Tutorial")]
        public void ResetBattlefieldTutorial()
        {
            PlayerSaveData playerSaveData = SaveDataHandler.LoadPlayerSaveData();
            playerSaveData.BattlefieldInfoSectionsViewed.Clear();
            SaveDataHandler.SavePlayerSaveData(playerSaveData);
        }
    }
}
