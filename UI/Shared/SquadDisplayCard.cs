using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Memori.SaveData;
using Memori.UI;
using Memori.Audio;

namespace TJ
{
    [RequireComponent(typeof(SquadDisplayCardPresenter))]
    public class SquadDisplayCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Image selectionHighlightImage;
        [SerializeField] private Transform itemToScale;
        protected SquadToLoad squad;
        public string UniqueID => squad.UniqueID;
        protected UnitType unitType;
        [SerializeField] protected int squadId;
        public int SquadId => squadId;
        public int SquadPrestige => squad.UnitPrestige;
        protected bool isSelected = false;
        protected bool isLocked = false;
        protected bool isPointerOver = false;
        private bool isHovered = false;
        protected SquadDisplayCardPresenter _presenter;
        private void Awake()
        {
            _presenter = GetComponent<SquadDisplayCardPresenter>();
            _presenter.SelectUnitTypeButton.onClick.AddListener(SelectSquadButtonClicked);
            _presenter.HoveredImage.enabled = true;
            _presenter.SelectedImage.enabled = false;
        }
        public virtual void SetUp(SquadToLoad _squadToLoad, int _squadID)
        {
            if(_presenter == null)
                _presenter = GetComponent<SquadDisplayCardPresenter>();
                
            squad = _squadToLoad;
            squadId = _squadID;
            unitType = TabletopTavernData.Instance.GetUnitTypeFromUnitName(squad.UnitName);
            _presenter.UnitCountText.text = TabletopTavernData.Instance.GetSquadCurrentUnitCount(squad);

            _presenter.HealthSlider.maxValue = squad.SquadMaxHealth;
            _presenter.HealthSlider.value = squad.SquadCurrentHealth;

            _presenter.UnitImage.sprite = TabletopTavernData.Instance.GetUnitIcon(squad.UnitName);
            _presenter.UnitTypeIcon.sprite = TabletopTavernData.Instance.GetSquadTypeIcon(squad.UnitName);
            UnitRarity rarityTier = TabletopTavernData.Instance.GetSquadStats(squad.UnitName).RarityTier;
            _presenter.SetTierVisuals(rarityTier);
            _presenter.SetPrestigeFrames(squad.UnitPrestige);

            // unitAttributesUIContainer.Load(squad.UnitName, );

            _presenter.SetBackgroundImage(TabletopTavernData.Instance.GetRaceFromUnitName(squad.UnitName));
        }
        public virtual void SelectSquadButtonClicked()
        {
            // isSelected = true;
            // selectedImage.enabled = true;
            // SelectSquad(!isSelected);
        }
        public void RefreshUnitCountInBattle(int _unitCount)
        {
            // healthSlider.value = _unitCount;
            _presenter.UnitCountText.text = _unitCount.ToString();
        }
        public void HoverSquad(bool _hovered)
        {
            if (_presenter.Animator == null) return;
            if (isHovered == _hovered) return;

            isHovered = _hovered;
            _presenter.Animator.Play(_hovered ? "SquadDisplayPointerEnter" : "SquadDisplayPointerExit");
            if (isSelected)
            {
                _presenter.HoveredImage.enabled = false;
                return;
            }
            _presenter.HoveredImage.enabled = !_hovered;
            if (_hovered)
                _presenter.OnHoverEnterFeedback.PlayFeedbacks();
            else
                _presenter.OnHoverExitFeedback.PlayFeedbacks();
        }
        public void HoverHighlight(bool _highlighted)
        {
            _presenter.HoveredImage.enabled = !_highlighted && !isSelected;
        }

        public virtual void SelectSquad(bool _selected)
        {
            // Debug.Log($"SelectSquad: {_selected}");
            if (isSelected == _selected) return;

            isSelected = _selected;
            _presenter.SelectedImage.enabled = isSelected;
            _presenter.HoveredImage.enabled = !isSelected;
            
            if (selectionHighlightImage != null) selectionHighlightImage.enabled = isSelected;
            if (!_presenter.HoveredImage.enabled)
                _presenter.OnHoverEnterFeedback.PlayFeedbacks();
            if (!isSelected && !isPointerOver)
                _presenter.OnHoverExitFeedback.PlayFeedbacks();
        }
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _presenter.Animator.Play("SquadDisplayPointerEnter");
        }
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            _presenter.Animator.Play("SquadDisplayPointerExit");
            HoverSquad(false);
        }
        public virtual void OnSelect(BaseEventData eventData)
        {
            IAudioRequester.Instance.PlaySFX(SFXData.ButtonHover);
            MemoriUI.BloomItemScale(itemToScale, 1.015f, 0.1f);
            if (selectionHighlightImage != null) selectionHighlightImage.enabled = true;
            OnPointerEnter(null); // Trigger hover effects
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            MemoriUI.BloomItemScale(itemToScale, 1f, 0.1f);
            if (selectionHighlightImage != null && !isSelected) selectionHighlightImage.enabled = false;
            OnPointerExit(null); // Trigger exit effects
        }
}
}
