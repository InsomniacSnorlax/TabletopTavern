using Memori.Utilities;
using TMPro;
using UnityEngine;
using Memori.Localization;

namespace TJ
{
[RequireComponent(typeof(MemoriCanvasGroup))]
public class SpawnSquadCanvasGroup : MonoBehaviour
{
    [SerializeField] private bool showOnStart;
    [SerializeField] private TMP_Dropdown prestigeDropdown;
    [SerializeField] private TMP_Dropdown weatherDropdown;

    private void Start() 
    {
        prestigeDropdown.options.Clear();
        weatherDropdown.options.Clear();
        string tierLocalized = LocalizationManager.Instance.GetText("Tier");
        string clearSkiesLocalized = LocalizationManager.Instance.GetText("ClearSkies");
        string rainLocalized = LocalizationManager.Instance.GetText("Rain");
        prestigeDropdown.options.Add(new TMP_Dropdown.OptionData(tierLocalized +"I"));
        prestigeDropdown.options.Add(new TMP_Dropdown.OptionData(tierLocalized +"II"));
        prestigeDropdown.options.Add(new TMP_Dropdown.OptionData(tierLocalized +"III"));
        weatherDropdown.options.Add(new TMP_Dropdown.OptionData(clearSkiesLocalized));
        weatherDropdown.options.Add(new TMP_Dropdown.OptionData(rainLocalized));

        #if !UNITY_EDITOR
            showOnStart = BattleManager.Instance.BattleSaveManager.IsCustomBattle;
        #endif

        if(showOnStart){
            GetComponent<MemoriCanvasGroup>().CGEnable();
        } else {
            GetComponent<MemoriCanvasGroup>().CGDisable();
        }
    }
}
}

