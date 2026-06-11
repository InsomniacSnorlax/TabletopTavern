using Memori.Tooltip;
using Memori.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Memori.Localization;

namespace TJ.Recruit
{
[RequireComponent(typeof(MemoriTooltipTrigger))]
public class RecruitStatUI : MonoBehaviour
{
    [SerializeField] private TMP_Text eventRewardText;
    [SerializeField] private Image eventOutcomeImage;
    public void LoadRecruitStatUI(UnitStat unitStat, string amount)
    {
        eventOutcomeImage.sprite = SpriteData.GetSprite(unitStat.ToString());
        eventOutcomeImage.color = ColorData.GetUnitStatColor(unitStat);
        eventRewardText.text = amount;

        MemoriTooltipTrigger memoriTooltipTrigger = GetComponent<MemoriTooltipTrigger>();
        string localizedStats = LocalizationManager.Instance.GetText(unitStat.ToString());
        memoriTooltipTrigger.SetUpToolTip(localizedStats, _delay: 1f);
    }
}
}
