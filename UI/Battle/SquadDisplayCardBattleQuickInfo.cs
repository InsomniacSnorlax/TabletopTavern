
using UnityEngine;
using System.Collections.Generic;

namespace TJ
{
    public class SquadDisplayCardBattleQuickInfo : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _movementIcon, _chargeIcon, _braceIcon, _defensiveStanceIcon, _volleyFireIcon, _rapidFireIcon;
        private bool _showingInfo;

        public void UpdateSquadDisplay(bool isMoving, bool isCharging, bool isBracing, bool defensiveStance, bool volleyFiring, bool rapidFiring)
        {
            if((!isMoving && !isCharging && !isBracing && !defensiveStance && !volleyFiring && !rapidFiring) && _showingInfo)
            {
                _animator.Play("SQIHide");
                _showingInfo = false;
            }
            else if((isMoving || isCharging || isBracing || defensiveStance || volleyFiring || rapidFiring) && !_showingInfo)
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

        }
    }
}