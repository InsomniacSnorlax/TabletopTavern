using UnityEngine;
using UnityEngine.UI;
using Memori.SaveData;

namespace TJ
{
    public class AbandonRunButton : MonoBehaviour
    {
        [SerializeField] private CampaignSaveManager campaignSaveManager;
        SettingsManager settingsManager;
        [SerializeField] private Button button;
        public void SetUp(SettingsManager _settingsManager)
        {
            settingsManager = _settingsManager;
            button.onClick.AddListener(AbandonRun);
        }
        public void AbandonRun()
        {
            campaignSaveManager = FindFirstObjectByType<CampaignSaveManager>();
            
            if(campaignSaveManager == null) 
            {
                SaveDataHandler.DeleteCampaignSave();
            } else {
                #if UNITY_EDITOR
                    campaignSaveManager.OverrideCampaignSave();
                #else
                    campaignSaveManager.DeleteCampaignSave();
                #endif
            }
            settingsManager.AbandonRun();
        }
        
    }
}
