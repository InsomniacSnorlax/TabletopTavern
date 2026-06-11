using Memori.Tooltip;
using TJ;
using UnityEngine;
using UnityEngine.UI;

namespace TJ.Map
{
[RequireComponent(typeof(MemoriTooltipTrigger))]
public class PrestigeUnitButton : MonoBehaviour
{
    [SerializeField] private Button prestigeButton;
    public Button PrestigeButton => prestigeButton;
    [SerializeField] private Image prestigeButtonImage, outlineImage;
    [SerializeField] private Color availableColor, unavailableColor;
    SquadDisplayCardMenu squadDisplayCardMenu;
    [SerializeField] MemoriTooltipTrigger tooltipTrigger;
    public MemoriTooltipTrigger TooltipTrigger => tooltipTrigger;
    private void Awake()
    {
        squadDisplayCardMenu = GetComponentInParent<SquadDisplayCardMenu>();
    }
    public void SetPrestigeAvailability(bool isAvailable)
    {
        gameObject.SetActive(isAvailable);
        if(squadDisplayCardMenu == null) squadDisplayCardMenu = GetComponentInParent<SquadDisplayCardMenu>();
        squadDisplayCardMenu.ShowPrestigeAvailability(isAvailable);
    }
}
}