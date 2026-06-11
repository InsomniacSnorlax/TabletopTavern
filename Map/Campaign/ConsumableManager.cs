using UnityEngine;
using TJ;
using Memori.Audio;
using Memori.Notifications;
using Memori.SaveData;
using System.Linq;
using System;
using Memori.Localization;

namespace TJ.Map
{
    public class ConsumableManager : MonoBehaviour
    {
        [SerializeField] private string targetUnitGuid;
        public void UseConsumable(ConsumableEnum _consumable)
        {
            CampaignManager.Instance.CampaignSaveManager.RemoveConsumable(_consumable);
            switch (_consumable)
            {
                case ConsumableEnum.MinorHealth:
                {
                    SquadToLoad targetedSquad = CampaignManager.Instance.CampaignSaveManager.SaveData.playerArmy.Where(squad => squad.UniqueID == targetUnitGuid).FirstOrDefault();
                    float amountToHeal = 0.5f;
                    // Debug.Log($"ConsumableManager.UseConsumable({_consumable}) amountToHeal = {amountToHeal}");

                    //The Light of Nytherial: Units recieve 2x Healing from all sources,
                    if (HeroBonusManager.Instance.ActiveHeroID == 8)
                    {
                        amountToHeal = 1;
                    }

                    //Sister Morvayne: Common Units gain 3x Healing from all sources
                    // if(HeroBonusManager.Instance.ActiveHeroID == 9 && TabletopTavernData.Instance.GetUnitTierFromUnitName(targetedSquad.UnitName) == 1)
                    // {
                    //     amountToHeal = 1;
                    // }

                    CampaignManager.Instance.CampaignSaveManager.ModifySpecificUnitHealth(amountToHeal, targetUnitGuid);
                    break;
                }
                case ConsumableEnum.MajorHealth:
                {
                    SquadToLoad targetedSquad = CampaignManager.Instance.CampaignSaveManager.SaveData.playerArmy.Where(squad => squad.UniqueID == targetUnitGuid).FirstOrDefault();
                    // Debug.Log($"ConsumableManager.UseConsumable({_consumable}) amountToHeal = {1}");
                    CampaignManager.Instance.CampaignSaveManager.ModifySpecificUnitHealth(1, targetUnitGuid);
                    break;
                }
                case ConsumableEnum.Prestige:
                {
                    SquadToLoad targetedSquad = CampaignManager.Instance.CampaignSaveManager.SaveData.playerArmy.Where(squad => squad.UniqueID == targetUnitGuid).FirstOrDefault();
                    CampaignManager.Instance.CampaignSaveManager.PrestigeSpecificUnit(targetedSquad);
                    IAudioRequester.Instance.PlaySFX(SFXData.PrestigeUnit);
                    break;
                }
                case ConsumableEnum.Duplicate:
                {
                    SquadToLoad targetedSquad = CampaignManager.Instance.CampaignSaveManager.SaveData.playerArmy.Where(squad => squad.UniqueID == targetUnitGuid).FirstOrDefault();
                    CampaignManager.Instance.CampaignSaveManager.RecruitSquad(TabletopTavernData.Instance.GetSquadStats(targetedSquad.UnitName));
                    break;
                }
                case ConsumableEnum.NewUnit:
                {
                    UnitName[] unitNames = TabletopTavernData.Instance.GetSquadsToRecruitBasedOnReputation(0, 1, CampaignManager.Instance.CampaignSaveManager.GetSeededRandom(), CampaignManager.Instance.CampaignSaveManager.GetHeroID());
                    CampaignManager.Instance.CampaignSaveManager.RecruitSquad(TabletopTavernData.Instance.GetSquadStats(unitNames[0]));
                    IAudioRequester.Instance.PlaySFX(SFXData.RecruitUnit);
                    break;
                }
                case ConsumableEnum.RunewellNectar:
                {
                    int consumableSlots = CampaignManager.Instance.CampaignSaveManager.ConsumableCapacity;
                    for (int i = 0; i < consumableSlots; i++)
                    {
                        ConsumableUI consumableUI = CampaignManager.Instance.MapSceneUIManager.HUDPanel.ConsumableUI[i];
                        if (!consumableUI.ConsumableLoaded)
                        {
                            ConsumableEnum randomConsumable = ConsumableData.GetWeightedConsumable();
                            while (randomConsumable == ConsumableEnum.RunewellNectar)
                            {
                                randomConsumable = ConsumableData.GetWeightedConsumable();
                            }
                            CampaignManager.Instance.CampaignSaveManager.AquireConsumable(randomConsumable);
                        }
                    }
                    break;
                }
                case ConsumableEnum.Rewind:
                {
                    if (CampaignManager.Instance.MapSceneUIManager.EventPanel.CanReroll)
                    {
                        CampaignManager.Instance.CampaignSaveManager.IncrementRerollCount();
                        CampaignManager.Instance.MapSceneUIManager.EventPanel.RollDice();
                    }
                    else if (CampaignManager.Instance.MapSceneUIManager.GamesPanel.CanRewind)
                    {
                        CampaignManager.Instance.CampaignSaveManager.IncrementRerollCount();
                        CampaignManager.Instance.MapSceneUIManager.GamesPanel.Rewind();
                    }
                    break;
                }
                case ConsumableEnum.TrialofGrasses:
                {
                    SquadToLoad targetedSquad = CampaignManager.Instance.CampaignSaveManager.SaveData.playerArmy.Where(squad => squad.UniqueID == targetUnitGuid).FirstOrDefault();
                    CampaignManager.Instance.CampaignSaveManager.TrialOfGrassesPrestigeSpecificUnit(targetedSquad);
                    IAudioRequester.Instance.PlaySFX(SFXData.PrestigeUnit);
                    break;
                }
                case ConsumableEnum.FateshineElixir:
                {
                    CampaignManager.Instance.MapSceneUIManager.EventPanel.Guarentee20();
                    break;
                }
                case ConsumableEnum.Alchemist:
                    {
                        CampaignManager.Instance.CampaignSaveManager.ModifyGold(5);
                        break;
                    }
                case ConsumableEnum.LambSauce:
                    {
                        Hero hero = HeroData.GetHeroByID(CampaignManager.Instance.CampaignSaveManager.GetHeroID());
                        CampaignManager.Instance.CampaignSaveManager.RecruitSquad(TabletopTavernData.Instance.GetSquadStats(hero.SignatureUnit));
                        IAudioRequester.Instance.PlaySFX(SFXData.RecruitUnit);
                        break;
                    }
                default:
                Debug.LogError($"ConsumableManager.UseConsumable({_consumable}) not implemented");
                    break;
            }
        }
        public bool AttemptToUseConsumable(ConsumableEnum _consumable, int _targetUnitIndex)
        {
            // Debug.Log($"ConsumableManager.AttemptToUseConsumable({_consumable}, {_targetUnitIndex})");
            if(!ConsumableData.ConsumableRequiresTarget(_consumable)) {

                if(_consumable == ConsumableEnum.Rewind) {
                    bool canReroll = CampaignManager.Instance.MapSceneUIManager.EventPanel.CanReroll;
                    bool canRewind = CampaignManager.Instance.MapSceneUIManager.GamesPanel.CanRewind;
                    if (!canReroll && !canRewind) {
                        NotificationManager.Instance.ErrorNotification(LocalizationManager.Instance.GetText("consumable_cannot_reroll"));
                        return false;
                    }
                }

                return true;
            } 

            if(_targetUnitIndex == -1){
                NotificationManager.Instance.ErrorNotification(LocalizationManager.Instance.GetText("consumable_must_select_target"));
                return false;
            }

            targetUnitGuid = CampaignManager.Instance.MapSceneUIManager.HUDPanel.GetGuidFormHoveredUnit(_targetUnitIndex);
            // Debug.Log($"ConsumableManager.AttemptToUseConsumable({_consumable}, {_targetUnitIndex}) targetUnitGuid = {targetUnitGuid}");

            if(targetUnitGuid == null) {
                NotificationManager.Instance.ErrorNotification(LocalizationManager.Instance.GetText("consumable_must_select_target"));
                return false;
            }

            SquadToLoad targetedSquad = CampaignManager.Instance.CampaignSaveManager.SaveData.playerArmy.Where(squad => squad.UniqueID == targetUnitGuid).FirstOrDefault();

            if((_consumable == ConsumableEnum.NewUnit || _consumable == ConsumableEnum.Duplicate || _consumable == ConsumableEnum.LambSauce) && 
                !CampaignManager.Instance.CampaignSaveManager.CheckForRoomToRecruit()) 
            {
                string localizedMessage = LocalizationManager.Instance.GetText("Max Units Recruited");
                NotificationManager.Instance.ErrorNotification(localizedMessage);
                return false;
            }

            if(_consumable == ConsumableEnum.Prestige && targetedSquad.UnitPrestige == 2) {
                NotificationManager.Instance.ErrorNotification(LocalizationManager.Instance.GetText("consumable_invalid_target_max_prestige"));
                return false;
            }
            if (_consumable == ConsumableEnum.TrialofGrasses)
            {
                if (targetedSquad.SquadCurrentHealth != targetedSquad.SquadMaxHealth)
                {
                    NotificationManager.Instance.ErrorNotification(LocalizationManager.Instance.GetText("consumable_invalid_target_requires_full_health"));
                    return false;
                }
                if(targetedSquad.UnitPrestige == 2)
                {
                    NotificationManager.Instance.ErrorNotification(LocalizationManager.Instance.GetText("consumable_invalid_target_max_prestige"));
                    return false;
                }
            }

            if (targetedSquad.SquadMaxHealth == 0)
            {
                NotificationManager.Instance.ErrorNotification(LocalizationManager.Instance.GetText("consumable_invalid_target_unit_dead"));
                return false;
            }

        return true;
    }
    public bool CanUseConsumable(ConsumableEnum _consumable)
    {
        if(_consumable == ConsumableEnum.Rewind) {
            return CampaignManager.Instance.MapSceneUIManager.EventPanel.CanReroll
                || CampaignManager.Instance.MapSceneUIManager.GamesPanel.CanRewind;
        }
        if(_consumable == ConsumableEnum.NewUnit || _consumable == ConsumableEnum.Duplicate || _consumable == ConsumableEnum.LambSauce) {
            return CampaignManager.Instance.CampaignSaveManager.CheckForRoomToRecruit();
        }

        return true;
    }
}
}