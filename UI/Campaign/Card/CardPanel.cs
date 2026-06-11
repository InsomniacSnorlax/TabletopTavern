using System.Collections.Generic;
using TJ.Map;
using Memori.Utilities;
using TJ.Engagement;
using TJ.Town;
using TJ.Event;
using UnityEngine;
using UnityEngine.UI;
using Memori.Audio;
using System.Threading.Tasks;
using TJ.Treasure;
using MoreMountains.Feedbacks;

namespace TJ
{
    [RequireComponent(typeof(MemoriCanvasGroup))]
    public class CardPanel : MonoBehaviour
    {
        CampaignSaveManager campaignSaveManager;
        [SerializeField] private Transform cardParentTransform;
        [SerializeField] private GearCard gearCardPrefab;
        [SerializeField] private Button skipButton;
        [SerializeField] private Image randomGearImage;
        [SerializeField] private MMF_Player rollingCardsMMF;

        MemoriCanvasGroup memoriCanvasGroup;
        EngagementPanel engagementPanel;
        EventAquireRewardButton aquireGearButton;
        TownPanel townPanel;
        TreasurePanel treasurePanel;
        List<GearID> gearList = new();
        Gear[] allGearList;

        public void SetUp(CampaignSaveManager _campaignSaveManager)
        {
            memoriCanvasGroup = GetComponent<MemoriCanvasGroup>();
            campaignSaveManager = _campaignSaveManager;
            skipButton.onClick.AddListener(CloseCardPanel);
            allGearList = GearData.GetAllGear();
        }
        /// <summary>
        /// Treasure panel
        /// </summary>
        /// <param name="_gearList"></param>
        /// <param name="_treasurePanel"></param>
        public void LoadCardPanel(List<GearID> _gearList, TreasurePanel _treasurePanel)
        {
            gearList = _gearList;
            treasurePanel = _treasurePanel;
            townPanel = null;
            engagementPanel = null;
            aquireGearButton = null;
            LoadCardPanel(true);
        }
        public void LoadCardPanel(List<GearID> _gearIDsList, TownPanel _townPanel)
        {
            Debug.Log($"Loading card panel with {string.Join(", ", _gearIDsList)}");
            gearList = _gearIDsList;
            townPanel = _townPanel;
            treasurePanel = null;
            engagementPanel = null;
            aquireGearButton = null;
            LoadCardPanel();
        }
        public void LoadCardPanel(List<GearID> _gearList, EventAquireRewardButton _aquireGearButton)
        {
            gearList = _gearList;
            townPanel = null;
            engagementPanel = null;
            treasurePanel = null;
            aquireGearButton = _aquireGearButton;
            LoadCardPanel();
        }
        private async void LoadCardPanel(bool _playRollAnimation = false)
        {
            Debug.Log($"Loading card panel with {string.Join(", ", gearList)}");
            skipButton.gameObject.SetActive(false);
            IAudioRequester.Instance.PlaySFX(SFXData.OpenUI);
            GearCard[] gearCards = cardParentTransform.GetComponentsInChildren<GearCard>();
            for (int i = 0; i < gearCards.Length; i++)
            {
                Destroy(gearCards[i].gameObject);
            }
            memoriCanvasGroup.CGEnable();

            if (_playRollAnimation)
            {
                await RollGearCards();
            }

            foreach (GearID GearID in gearList)
            {
                GearCard gearCard = Instantiate(gearCardPrefab, cardParentTransform);
                IAudioRequester.Instance.PlaySFX(SFXData.CardDraw);
                gearCard.LoadGearCardReward(GearID, this);
                await Task.Delay(100);
            }

            skipButton.gameObject.SetActive(true);
        }
        public void SelectCard(GearID _gear)
        {
            IAudioRequester.Instance.PlaySFX(SFXData.SelectCard);

            campaignSaveManager.AquireGear(_gear);

            if (townPanel != null)
            {
                townPanel.OnLootGearCardSelected();
            }
            else if (treasurePanel != null)
            {
                treasurePanel.OnCardSelected();
            }
            else if (aquireGearButton != null)
            {
                Destroy(aquireGearButton.gameObject);
            }

            CloseCardPanel();
        }
        public void CloseCardPanel()
        {
            memoriCanvasGroup.FadeOut();

            if (townPanel != null)
            {
                townPanel.ReloadTownPanel(); ;
            }
            else if (engagementPanel != null)
            {
                // engagementPanel.Reload();
                Debug.LogError($"Reloading engagement panel from card panel");
            }
            else if (treasurePanel != null)
            {
                treasurePanel.Skip();
            }
            else if (aquireGearButton != null)
            {
                CampaignManager.Instance.MapSceneUIManager.EventPanel.RevealActionButton();
            }
        }
        bool continueToSpin = true;
        private async Task RollGearCards()
        {
            rollingCardsMMF.PlayFeedbacks();
            IAudioRequester.Instance.PlaySFX(SFXData.Title);
            continueToSpin = true;
            void RollCard()
            {
                //get random gear from the list
                Gear randomGear = allGearList[Random.Range(0, allGearList.Length)];

                //get the icon
                randomGearImage.sprite = SpriteData.GetSprite(randomGear.GearName);
                IAudioRequester.Instance.PlaySFX(SFXData.TinyClick);
            }

            while (continueToSpin)
            {
                RollCard();
                await Task.Delay(100);
            }

            await Task.Delay(2000);
            rollingCardsMMF.StopFeedbacks();
        }
        public void DoneSpinning()
        {
            continueToSpin = false;
            randomGearImage.sprite = SpriteData.GetSprite(GearData.GetGear(gearList[0]).GearName);
            IAudioRequester.Instance.PlaySFX(SFXData.BattleWin);
        }
    }
}