using UnityEngine;
using UnityEngine.EventSystems;
using Memori.SaveData;
using Memori.Tooltip;
using TJ.MainMenu;
using Memori.Steamworks;

namespace TJ
{
    public class SquadDisplayCardCollection: SquadDisplayCard
    {
        [SerializeField] private GameObject collectionUnacknowledgedIndicator, collectionFrame;
        bool isCollected, isAcknowledged;
        CollectionPanel collectionPanel;
        public GameObject soonBlocker;
        Race race;
        public void SetUp(SquadToLoad _squad, bool _isCollected, bool _isAcknowledged, CollectionPanel _collectionPanel, Race _race)
        {
            squad = _squad;
            isCollected = _isCollected;
            isAcknowledged = _isAcknowledged;
            collectionPanel = _collectionPanel;
            race = _race;

            _presenter.UnitImage.sprite = TabletopTavernData.Instance.GetUnitIcon(squad.UnitName);
            _presenter.UnitTypeIcon.sprite = TabletopTavernData.Instance.GetSquadTypeIcon(squad.UnitName);
            _presenter.SetTierVisuals(TabletopTavernData.Instance.GetSquadStats(squad.UnitName).RarityTier);
            _presenter.SetPrestigeFrames(-1);

            collectionFrame.SetActive(true);
            soonBlocker.SetActive(false);

            _presenter.SetBackgroundImage(race);

            bool comingSoon = false;
            soonBlocker.SetActive(comingSoon);

            unitType = TabletopTavernData.Instance.GetUnitTypeFromUnitName(squad.UnitName);
            // unitCountText.text = squad.currentUnitCount.ToString();
            _presenter.HealthSlider.maxValue = squad.maxUnitCount;
            // healthSlider.value = squad.currentUnitCount;

            if(isCollected && !isAcknowledged) collectionUnacknowledgedIndicator.SetActive(true);
            else collectionUnacknowledgedIndicator.SetActive(false);

            _presenter.UnitImage.color = isCollected ? Color.white : new Color(0.15f, 0.15f, 0.15f, 1f);
        }
        public override void SelectSquadButtonClicked()
        {
            SelectSquad(isSelected);
        }
        public override void SelectSquad(bool _selected)
        {

        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            // base.OnPointerEnter(eventData);
            base.HoverSquad(true);
            collectionPanel.HoverSquad(squad, isCollected);
            // onHoverEnterFeedback.PlayFeedbacks();

            if(isCollected && !isAcknowledged) {
                isAcknowledged = true;
                collectionUnacknowledgedIndicator.SetActive(false);
                SaveDataHandler.AcknowledgedTroop(squad.UnitName);
                collectionPanel.UpdateAcknowledged();
            }
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            // if(isLocked) return;
            _presenter.Animator.Play("SquadDisplayPointerExit");
            // onHoverExitFeedback.PlayFeedbacks();

            // CampaignManager.Instance.MapSceneUIManager.HUDPanel.HoverSquad(squad, false);
        }
    }
}
