using UnityEngine;
using UnityEngine.EventSystems;

namespace TJ.Engagement
{
    public class AutoResolvePreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private EngagementPanel engagementPanel;
        bool wasSetUp = false;
        public void SetUp(EngagementPanel _engagementPanel)
        {
            wasSetUp = true;
            engagementPanel = _engagementPanel;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!wasSetUp) return;
            engagementPanel.ShowAutoResolvePrediction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!wasSetUp) return;
            engagementPanel.HideAutoResolvePrediction();
        }
        public void CheckIfMouseOverTooltip()
        {
            if (!wasSetUp) return;
            // Check if the cursor is already over the button when it becomes interactable
            if (IsPointerOverGameObject())
            {
                OnPointerEnter(null); // Manually trigger the tooltip logic
            }
        }
        // Helper method to check if the cursor is over this button
        private bool IsPointerOverGameObject()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current) {
                position = UnityEngine.Input.mousePosition
            };

            var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            foreach (var result in raycastResults)
            {
                if (result.gameObject == gameObject)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
