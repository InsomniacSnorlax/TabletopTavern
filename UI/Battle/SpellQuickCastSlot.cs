using System;
using Memori.Tooltip;
using TJ.Spells;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Memori.Localization;

namespace TJ
{
    public class SpellQuickCastSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image spellSprite;
        [SerializeField] private MemoriTooltipTrigger tooltipTrigger;
        private Action onPointerEnter, onPointerExit;

        public void SetUp(SpellData spellData, Action _onPointerEnter, Action _onPointerExit)
        {
            spellSprite.sprite = spellData.SpellSprite;
            onPointerEnter = _onPointerEnter;
            onPointerExit = _onPointerExit;

            string localizedSpellName = LocalizationManager.Instance.GetText(spellData.Spell.ToString());
            string localizedSpellDescription = spellData.GetLocalizedSpellDescription(); 

            tooltipTrigger.SetUpToolTip(localizedSpellName, localizedSpellDescription, _delay:0.5f);
        }

        public void OnPointerEnter(PointerEventData eventData) => onPointerEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => onPointerExit?.Invoke();
    }
}
