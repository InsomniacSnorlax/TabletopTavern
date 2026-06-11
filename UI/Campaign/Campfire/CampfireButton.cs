using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TJ.Campfire
{
    public class CampfireButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image backgroundImage;

        private static readonly Color PrimaryColor   = (Color)ColorData.HexToRgba(ColorData.Primary);
        private static readonly Color SecondaryColor = (Color)ColorData.HexToRgba(ColorData.Secondary);
        private static readonly float AlphaDefault = 25f / 255f;
        private static readonly float AlphaHover   = 50f / 255f;

        private void Start()
        {
            SetColor(SecondaryColor);
            SetImageAlpha(AlphaDefault);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetColor(PrimaryColor);
            SetImageAlpha(AlphaHover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetColor(SecondaryColor);
            SetImageAlpha(AlphaDefault);
        }

        private void SetColor(Color color)
        {
            if (titleText != null)       titleText.color       = color;
            if (descriptionText != null) descriptionText.color = color;
        }

        private void SetImageAlpha(float alpha)
        {
            if (backgroundImage == null) return;
            Color c = backgroundImage.color;
            c.a = alpha;
            backgroundImage.color = c;
        }
    }
}
