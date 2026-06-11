using Memori.Localization;
using Memori.Tooltip;
using UnityEngine;


namespace TJ
{
    [RequireComponent(typeof(MemoriTooltipTrigger))]
    public class LockedButton : MonoBehaviour
    {
        [SerializeField] protected MemoriTooltipTrigger _tooltipTrigger;
        public void SetLockedState(bool isLocked, string lockReason = "")
        {
            if(_tooltipTrigger == null)
                _tooltipTrigger = GetComponent<MemoriTooltipTrigger>();

            string lockedLocalized = LocalizationManager.Instance.GetText("Locked");
            _tooltipTrigger.SetUpToolTip(_title: lockedLocalized, _description: lockReason);
            gameObject.SetActive(isLocked);
        }
    }
}
