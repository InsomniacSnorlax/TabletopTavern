using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TJ.Settings
{
    public class EdgePanningToggle : MonoBehaviour
    {
        public static event Action<bool> OnEdgePanningChanged;
        public const string PlayerPrefKey = "EdgePanning";

        [SerializeField] private string displayName = "Edge Panning";
        [SerializeField] private TMP_Text displayText, statusText;
        [SerializeField] private Toggle onToggle;
        [SerializeField] private string onText = "On";
        [SerializeField] private string offText = "Off";

        private void Awake()
        {
            displayText.text = displayName;
            int savedValue = PlayerPrefs.GetInt(PlayerPrefKey, 0);
            onToggle.isOn = savedValue == 1;
            statusText.text = onToggle.isOn ? onText : offText;
            onToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        public void OnToggleValueChanged(bool isOn)
        {
            PlayerPrefs.SetInt(PlayerPrefKey, isOn ? 1 : 0);
            statusText.text = isOn ? onText : offText;
            OnEdgePanningChanged?.Invoke(isOn);
        }
    }
}
