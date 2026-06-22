using UnityEngine;
using TJ.Map;
using System.Collections.Generic;
using Memori.SaveData;
using Memori.Audio;
using System;
using Memori.Localization;

namespace TJ
{
public class GoldManager : MonoBehaviour
{
    [SerializeField] GoldNotificationSpawner goldNotificationSpawner;
    CampaignSaveManager campaignSaveManager;
    public event Action<int> OnGoldAmountChanged;
    private int maxInterest = 5;
    private int potionRewardsOdds = 25;
    public int PotionRewardsOdds => potionRewardsOdds;
    private int _currentGoldAmount = 0;
    public int CurrentGoldAmount => _currentGoldAmount;

    public void ModifyGold(int amount, string localizedString, bool silent = false)
    {
        if(amount == 0) return;

        campaignSaveManager.ModifyGoldSaveDataValue(amount);
        _currentGoldAmount = campaignSaveManager.SaveData.goldAmount;

        OnGoldAmountChanged?.Invoke(_currentGoldAmount);

        Debug.Log($"Modified gold by {amount}. for: {localizedString}");
        if(!silent) 
        {
            IAudioRequester.Instance.PlaySFX(SFXData.AquireGold);
            goldNotificationSpawner.DisplayGoldNotification(_currentGoldAmount, localizedString);
        }
    }

    public void SetUp()
    {
        campaignSaveManager = CampaignManager.Instance.CampaignSaveManager;
    }

    public void LoadGold()
    {
        _currentGoldAmount = campaignSaveManager.SaveData.goldAmount;
        goldNotificationSpawner.LoadGold(campaignSaveManager.SaveData.goldAmount);
        OnGoldAmountChanged?.Invoke(_currentGoldAmount);
    }
    public int GetMaxInterest()
    {
        int _maxInterest = maxInterest;
        if(CampaignManager.Instance.GearManager.CheckForGear(GearID.DwarvenTaxCollectors)) _maxInterest = 10;

        return _maxInterest;
    }
    public int GetBaseInterest()
    {   
        return Mathf.Min(campaignSaveManager.GetInterestEarned(), GetMaxInterest());
    }
    public int GetTotalInterest()
    {
        int interest = GetBaseInterest();
        if(CampaignManager.Instance.GearManager.CheckForGear(GearID.OmenofFamine)) {
            List<GearID> gearIDs = campaignSaveManager.SaveData.Gear;
            int bonus = 2 * (campaignSaveManager.MaxGear - gearIDs.Count);
            interest += bonus;
        }
        if(CampaignManager.Instance.GearManager.CheckForGear(GearID.IronBank))  interest *= 2;
        
        if (HeroBonusManager.Instance.ActiveHeroID == 12) interest *= 2;

        return interest;
    }
    public void CollectInterest()
    {
        int interest = GetTotalInterest();
        int bonus = 0;

        for (int i = 0; i < campaignSaveManager.SaveData.playerArmy.Length; i++)
        {
            var squad = campaignSaveManager.SaveData.playerArmy[i];
            if (squad.isEmptySquad) continue;
            if (TabletopTavernData.Instance.GetSquadStats(squad.UnitName).SquadAttributes.DragonsHoard)
            {
                bonus += 3;
            }
        }

        if(interest == 0) return;
        string interestLocalized = LocalizationManager.Instance.GetText("Interest at turn end");
        ModifyGold(interest, interestLocalized);

        if(bonus > 0) ModifyGold(bonus, LocalizationManager.Instance.GetText("DragonsHoard"));
        // Debug.Log($"Collected {interest} gold in interest");
    }
    public bool CheckIfCanAfford(int _cost)
    {
        return campaignSaveManager.SaveData.goldAmount >= _cost;
    }
    
}
}