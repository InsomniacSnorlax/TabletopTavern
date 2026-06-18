using UnityEngine;
using TJ.Map;
using System.Collections.Generic;
using Memori.SaveData;
using Memori.Audio;

namespace TJ
{
public class EconomyManager : MonoBehaviour
{
    CampaignSaveManager campaignSaveManager;
    public event System.Action<int> OnGoldAmountChangedEconomyManager;
    private int maxInterest = 5;
    private int potionRewardsOdds = 25;
    public int PotionRewardsOdds => potionRewardsOdds;
    private int _currentGoldAmount = 0;
    public int CurrentGoldAmount => _currentGoldAmount;
    public void SetUp()
    {
        campaignSaveManager = CampaignManager.Instance.CampaignSaveManager;
        campaignSaveManager.OnGoldChanged += OnGoldAmountChanged;
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

        for (int i = 0; i < campaignSaveManager.SaveData.playerArmy.Length; i++)
        {
            if (campaignSaveManager.SaveData.playerArmy[i].UnitName == UnitName.EmeraldAncient)
            {
                Debug.Log($"Collecting interest from Emerald Ancient");
                interest += 3;
            }
        }

        if(interest == 0) return;

        campaignSaveManager.ModifyGold(interest);
        // Debug.Log($"Collected {interest} gold in interest");
    }
    public void LoseGoldOnTurnEnd()
    {
        int goldPenalty = -1;
        campaignSaveManager.ModifyGold(goldPenalty);
    }
    public bool CheckIfCanAfford(int _cost)
    {
        return campaignSaveManager.SaveData.goldAmount >= _cost;
    }
    public void OnGoldAmountChanged(int _amount)
    {
        IAudioRequester.Instance.PlaySFX(SFXData.AquireGold);
        _currentGoldAmount = _amount;
        OnGoldAmountChangedEconomyManager?.Invoke(_amount);
    }
    public void SpendGold(int _cost)
    {
        campaignSaveManager.ModifyGold(-_cost);
    }
    // public int RerollCost()
    // {
    //     int rerollCost = 2;
    //     int rerolls = campaignSaveManager.SaveData.Rolls + 1;
    //     rerollCost *= rerolls;

    //     if(CampaignManager.Instance.GearManager.CheckForGear(GearID.CostcoCard) && campaignSaveManager.SaveData.Rolls==0) {
    //         rerollCost = 0;
    //     }
    //     return rerollCost;
    // }
    [ContextMenu("Give 500 gold")]
    public void TestEconomyManager()
    {
        SpendGold(-500);
    }
    private void OnDestroy() 
    {
        if(campaignSaveManager != null)
            campaignSaveManager.OnGoldChanged -= OnGoldAmountChanged;
    }
}
}