using UnityEngine;

namespace TJ.MainMenu
{
    public class PlayPanelExitArea : MonoBehaviour
    {
        [SerializeField] private PlayPanel playPanel;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, Input.mousePosition))
            {
                playPanel.CloseActiveButton();
            }
        }
    }
}
