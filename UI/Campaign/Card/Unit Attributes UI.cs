using Memori.Tooltip;
using TMPro;
using UnityEngine;
using Memori.UI;
using Memori.Localization;

namespace TJ
{
    [RequireComponent(typeof(MemoriTooltipTrigger))]
    public class UnitAttributesUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text attributeText;
        public TMP_Text AttributeText => attributeText;
        [Tooltip("Set true only on the distinct-colored prefab variant used for the prestige-granted trait.")]
        [SerializeField] private bool isPrestigeVariant;
        public bool IsPrestigeVariant => isPrestigeVariant;
        MemoriTooltipTrigger tooltipTrigger;
        UnitAttribute unitAttribute;
        UnitCondition unitCondition;
        public void Load(UnitAttribute _unitAttribute)
        {
            unitAttribute = _unitAttribute;
            string localizedAttribute = LocalizationManager.Instance.GetText(unitAttribute.ToString());
            // string localizedDescription = LocalizationManager.Instance.GetText(unitAttribute.ToString() + "Desc");

            attributeText.text = localizedAttribute;
            tooltipTrigger = GetComponent<MemoriTooltipTrigger>();
            tooltipTrigger.enabled = false;
            // tooltipTrigger.SetUpToolTip(_description: localizedDescription);
        }
        public void Load(UnitCondition _unitCondition)
        {
            unitCondition = _unitCondition;
            string localizedAttribute = LocalizationManager.Instance.GetText(unitCondition.ToString());
            string localizedDescription = LocalizationManager.Instance.GetText(unitCondition.ToString() + "Desc");

            attributeText.text = localizedAttribute;
            tooltipTrigger = GetComponent<MemoriTooltipTrigger>();
            tooltipTrigger.SetUpToolTip(_description: localizedDescription);
        }
        public void SetUpTooltip()
        {
            string localizedAttribute = LocalizationManager.Instance.GetText(unitAttribute.ToString());
            string localizedDescription = LocalizationManager.Instance.GetText(unitAttribute.ToString() + "Desc");
            tooltipTrigger = GetComponent<MemoriTooltipTrigger>();
            tooltipTrigger.enabled = true;
            tooltipTrigger.SetUpToolTip(_title: localizedAttribute, _description: localizedDescription);
        }
    }
}