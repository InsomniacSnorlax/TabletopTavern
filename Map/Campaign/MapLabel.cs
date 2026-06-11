using UnityEngine;
using UnityEngine.UI;
using Memori.Tooltip;
using TMPro;
using Memori.Localization;
using Memori.UI;

namespace TJ.Map
{
[RequireComponent(typeof(MemoriTooltipTrigger))]
public class MapLabel : MonoBehaviour
{
    [SerializeField] private Image nodeImage;
    [SerializeField] private Color defaultColor, hoverColor;
    [SerializeField] private TMP_Text nodeText;
    [SerializeField] private MemoriTooltipTrigger tooltipTrigger;

    public void SetUp(NodeType nodeType, bool surprise = false)
    {
        string _nodeType = GetNodeTypeLocalized(nodeType, surprise);
        string _nodeDesc = GetNodeDescLocalized(nodeType, surprise);

        nodeText.text = _nodeType;
        tooltipTrigger.SetUpToolTip(_nodeType, _nodeDesc);
    }
    public void HoverUI(bool _hover)
    {
        nodeImage.color = _hover ? hoverColor : defaultColor;
        nodeText.color = _hover ? hoverColor : defaultColor;
        MemoriUI.BloomFontSize(nodeText, _hover ? 22f : 20f, 0.1f);
    }
    private string GetNodeTypeLocalized(NodeType nodeType, bool surprise = false)
    {
        if (surprise) return LocalizationManager.Instance.GetText("Unknown");

        return nodeType switch
        {
            NodeType.Skirmish => LocalizationManager.Instance.GetText("Skirmish"),
            NodeType.Event => LocalizationManager.Instance.GetText("Event"),
            NodeType.Shop => LocalizationManager.Instance.GetText("Shop"),
            NodeType.Town => LocalizationManager.Instance.GetText("Town"),
            NodeType.Treasure => LocalizationManager.Instance.GetText("Treasure"),
            NodeType.Games => LocalizationManager.Instance.GetText("Games"),
            NodeType.Campfire => LocalizationManager.Instance.GetText("Campfire"),
            _ => LocalizationManager.Instance.GetText("Unknown"),
        };
    }
    private string GetNodeDescLocalized(NodeType nodeType, bool surprise = false)
    {
        if (surprise) return LocalizationManager.Instance.GetText("UnknownDesc");
        return nodeType switch
        {
            NodeType.Skirmish => LocalizationManager.Instance.GetText("SkirmishDesc"),
            NodeType.Event => LocalizationManager.Instance.GetText("EventDesc"),
            NodeType.Shop => LocalizationManager.Instance.GetText("ShopDesc"),
            NodeType.Town => LocalizationManager.Instance.GetText("TownDesc"),
            NodeType.Treasure => LocalizationManager.Instance.GetText("TreasureDesc"),
            NodeType.Games => LocalizationManager.Instance.GetText("GamesDesc"),
            NodeType.Campfire => LocalizationManager.Instance.GetText("CampfireDesc"),
            _ => LocalizationManager.Instance.GetText("UnknownDesc"),
        };
    }
}
}