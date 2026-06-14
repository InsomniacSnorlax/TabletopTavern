using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Memori.Tooltip;
using Memori.Localization;

namespace TJ.Event
{
    public class EventAquireRewardButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private Button button;
        [SerializeField] private Image rewardImage;
        public Button Button => button;
        public event Action onClick;

        [Header("Combine")]
        [SerializeField] private GameObject canCombineGO;
        [SerializeField] private MemoriTooltipTrigger canCombineTooltip;

        private UnitName unitName;
        private bool hasCombineCheck;
        private bool canCombine;
        public bool CanCombine => canCombine;

        public void SetUp(Sprite _rewardSprite, string _rewardText, Action action)
        {
            onClick = action;
            rewardImage.sprite = _rewardSprite;
            rewardText.text = _rewardText;
            button.onClick.AddListener(() => onClick?.Invoke());
        }

        public void SetUpCombineCheck(UnitName _unitName)
        {
            unitName = _unitName;
            hasCombineCheck = true;
            RefreshCombineState();
            CampaignManager.Instance.CampaignSaveManager.OnArmyStructureChanged += RefreshCombineState;
        }

        private void RefreshCombineState()
        {
            if (canCombineGO == null) return;
            canCombine = false;
            canCombineGO.SetActive(false);
            if (CampaignManager.Instance.CampaignSaveManager.CheckForRoomToRecruit()) return;

            var army = CampaignManager.Instance.CampaignSaveManager.SaveData.playerArmy;
            int minPrestige = int.MaxValue;
            int matchCount = 0;
            for (int i = 0; i < army.Length; i++)
            {
                if (army[i].UnitIndex == -1 || army[i].UnitName != unitName) continue;
                if (army[i].UnitPrestige < minPrestige)
                {
                    minPrestige = army[i].UnitPrestige;
                    matchCount = 1;
                }
                else if (army[i].UnitPrestige == minPrestige)
                {
                    matchCount++;
                }
            }
            if (matchCount >= 2 && minPrestige == 0)
            {
                canCombine = true;
                canCombineGO.SetActive(true);
                string combineTitle = LocalizationManager.Instance.GetText("Prestige");
                string combineDesc = LocalizationManager.Instance.GetText("PrestigeTooltip");
                canCombineTooltip.SetUpToolTip(combineTitle, combineDesc);
            }
        }

        private void OnDestroy()
        {
            if (hasCombineCheck && CampaignManager.HasInstance && CampaignManager.Instance.CampaignSaveManager != null)
                CampaignManager.Instance.CampaignSaveManager.OnArmyStructureChanged -= RefreshCombineState;
        }
    }
}
