using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Memori.Localization;

namespace TJ.Settings
{
    public class SettingsToggleV2 : MonoBehaviour
    {
        [SerializeField] private string playerPrefValue;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Toggle onToggle;
        [SerializeField] private bool defaultOn = false;
        private string onText, offText;

        public Toggle OnToggle => onToggle;

        private void Awake()
        {
            onText = LocalizationManager.Instance.GetText("settingsOn");
            offText = LocalizationManager.Instance.GetText("settingsOff");
            
            int defaultValue = defaultOn ? 1 : 0;
            int settingValue = PlayerPrefs.GetInt(playerPrefValue, defaultValue);
            statusText.text = settingValue == 1 ? onText : offText;
            onToggle.isOn = settingValue == 1;
            onToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
        public void OnToggleValueChanged(bool isOn)
        {
            PlayerPrefs.SetInt(playerPrefValue, isOn ? 1 : 0);
            statusText.text = isOn ? onText : offText;
        }
    }
}
