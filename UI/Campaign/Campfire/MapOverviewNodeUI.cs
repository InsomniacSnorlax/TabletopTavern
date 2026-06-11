using TJ.Map;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TJ.Campfire
{
    public class MapOverviewNodeUI : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject playerMarker;

        public void Setup(NodeType type, bool isVisited, bool isCurrentNode, bool isSurprise, bool wasHidden,
            Color visitedColor, Color currentColor, Color hiddenColor,
            Sprite nodeSprite)
        {
            if (playerMarker != null)
                playerMarker.SetActive(isCurrentNode);

            if (background != null)
                background.enabled = !isCurrentNode;

            if (background != null)
                background.sprite = nodeSprite;

            Color tint;
            if ((isSurprise || wasHidden) && !isVisited && !isCurrentNode) tint = hiddenColor;
            else if (isVisited || isCurrentNode)                            tint = visitedColor;
            else                                                            tint = Color.white;

            if (background != null)
                background.color = tint;

            // Only show label text as fallback when no sprite is assigned
            if (label != null)
                label.text = nodeSprite == null ? GetLabel(type) : string.Empty;
        }

        private static string GetLabel(NodeType type) => type switch
        {
            NodeType.Event    => "E",
            NodeType.Shop     => "$",
            NodeType.Town     => "T",
            NodeType.Skirmish => "S",
            NodeType.Warband  => "W",
            NodeType.Horde    => "H",
            NodeType.Treasure => "*",
            NodeType.Games    => "G",
            NodeType.Campfire => "C",
            _                 => "?",
        };
    }
}
