
using UnityEngine;
using System.Collections.Generic;
using Memori.Tooltip;
using Memori.Localization;
using UnityEngine.EventSystems;

namespace TJ
{
    public class SquadDisplayCardBattleQuickInfo : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _movementIcon, _chargeIcon, _braceIcon, _defensiveStanceIcon, _volleyFireIcon, _rapidFireIcon, _meleeIcon, _ceaseFireIcon;
        private bool _showingInfo;

        private void Start()
        {
            SetIconTooltip(_movementIcon,        "Moving");
            SetIconTooltip(_chargeIcon,          "Charging");
            SetIconTooltip(_braceIcon,           "Braced");
            SetIconTooltip(_defensiveStanceIcon, "DefensiveStanceTitle");
            SetIconTooltip(_volleyFireIcon,      "VolleyFireTitle");
            SetIconTooltip(_rapidFireIcon,       "FireAtWillTitle");
            SetIconTooltip(_meleeIcon,           "InCombatDesc");
            SetIconTooltip(_ceaseFireIcon,       "CeaseFireTitle");
        }

        public void OnPointerClick(PointerEventData eventData) { }
        public void OnPointerDown(PointerEventData eventData) { }
        public bool IsPointerOver { get; private set; }
        public void OnPointerEnter(PointerEventData eventData) { IsPointerOver = true; }
        public void OnPointerExit(PointerEventData eventData) { IsPointerOver = false; }

        private void SetIconTooltip(GameObject icon, string descKey)
        {
            if (icon == null) return;
            if (!icon.TryGetComponent<MemoriTooltipTrigger>(out var trigger)) return;
            trigger.SetUpToolTip(_description: LocalizationManager.Instance.GetText(descKey));
        }

        public void UpdateSquadDisplay(bool isMoving, bool isCharging, bool isBracing, bool defensiveStance, bool volleyFiring, bool rapidFiring, bool inMeleeCombat, bool ceaseFiring)
        {
            if((!isMoving && !isCharging && !isBracing && !defensiveStance && !volleyFiring && !rapidFiring && !inMeleeCombat && !ceaseFiring) && _showingInfo)
            {
                _animator.Play("SQIHide");
                _showingInfo = false;
            }
            else if((isMoving || isCharging || isBracing || defensiveStance || volleyFiring || rapidFiring || inMeleeCombat || ceaseFiring) && !_showingInfo)
            {
                _animator.Play("SQIShow");
                _showingInfo = true;
            }

            if(!isMoving && _movementIcon.activeSelf)
            {
                _movementIcon.SetActive(false);
            }
            else if(isMoving && !_movementIcon.activeSelf)
            {
                _movementIcon.SetActive(true);
            }

            if(isCharging && !_chargeIcon.activeSelf)
            {
                _chargeIcon.SetActive(true);
            }
            else if(!isCharging && _chargeIcon.activeSelf)
            {
                _chargeIcon.SetActive(false);
            }
            
            if(isBracing && !_braceIcon.activeSelf)
            {
                _braceIcon.SetActive(true);
            }
            else if(!isBracing && _braceIcon.activeSelf)
            {
                _braceIcon.SetActive(false);
            }

            if(defensiveStance && !_defensiveStanceIcon.activeSelf)
            {
                _defensiveStanceIcon.SetActive(true);
            }
            else if(!defensiveStance && _defensiveStanceIcon.activeSelf)
            {
                _defensiveStanceIcon.SetActive(false);
            }

            if(volleyFiring && !_volleyFireIcon.activeSelf)
            {
                _volleyFireIcon.SetActive(true);
            }
            else if(!volleyFiring && _volleyFireIcon.activeSelf)
            {
                _volleyFireIcon.SetActive(false);
            }

            if(rapidFiring && !_rapidFireIcon.activeSelf)
            {
                _rapidFireIcon.SetActive(true);
            }
            else if(!rapidFiring && _rapidFireIcon.activeSelf)
            {
                _rapidFireIcon.SetActive(false);
            }

            if(inMeleeCombat && !_meleeIcon.activeSelf)
            {
                _meleeIcon.SetActive(true);
            }
            else if(!inMeleeCombat && _meleeIcon.activeSelf)
            {
                _meleeIcon.SetActive(false);
            }

            if(ceaseFiring && !_ceaseFireIcon.activeSelf)
            {
                _ceaseFireIcon.SetActive(true);
            }
            else if(!ceaseFiring && _ceaseFireIcon.activeSelf)
            {
                _ceaseFireIcon.SetActive(false);
            }

        }
    }
}