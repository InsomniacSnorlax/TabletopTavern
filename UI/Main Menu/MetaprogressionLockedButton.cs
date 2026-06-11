using Memori.Localization;
using Memori.Metaprogression;
using Memori.SaveData;
using Memori.Tooltip;
using UnityEngine;


namespace TJ
{
    public class MetaprogressionLockedButton : LockedButton
    {
        [SerializeField] private MetaprogressionModel metaprogressionModel;
        
        public void CheckLockedState()
        {
            bool isLocked = !SaveDataHandler.IsMetaprogressionNodeUnlocked(metaprogressionModel);
            // SetLockedState(isLocked, "Requires Unlocking in Metaprogression");
            MemoriTooltipTrigger parentTooltipTrigger = null;
            if (transform.parent != null)
                parentTooltipTrigger = transform.parent.GetComponentInParent<MemoriTooltipTrigger>();
            string lockedLocalizedTitle = LocalizationManager.Instance.GetText("Locked");
            string lockedLocalizedDescription = LocalizationManager.Instance.GetText("Unlocked in Metaprogression Tree");

            if(parentTooltipTrigger != null)
            {
                // Debug.Log($"[MetaprogressionLockedButton] Found parent tooltip trigger for {gameObject.name}: {parentTooltipTrigger.gameObject.name}");
                
                if(isLocked)
                {
                    parentTooltipTrigger.enabled = false;
                    _tooltipTrigger.SetUpToolTip(
                        _title: lockedLocalizedTitle,
                        _description: lockedLocalizedDescription);
                    _tooltipTrigger.enabled = true;
                }
                else
                {
                    gameObject.SetActive(false); // Hide the locked button entirely if it's not locked, since it won't do anything
                }
            }
            else
            {
                // Debug.Log($"[MetaprogressionLockedButton] Warning: No parent tooltip trigger found for {gameObject.name}. Attempting to find tooltip trigger on the same GameObject.");
                
                if(isLocked)
                {
                    _tooltipTrigger.enabled = true;
                    _tooltipTrigger.SetUpToolTip(
                        _title: lockedLocalizedTitle,
                        _description: lockedLocalizedDescription);
                }
                else
                {
                    gameObject.SetActive(false); // Hide the locked button entirely if it's not locked, since it won't do anything
                }
            }
        }
    }
}
