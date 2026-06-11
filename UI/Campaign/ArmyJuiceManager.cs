using System.Collections.Generic;
using Memori.Audio;
using UnityEngine;

namespace TJ.Map
{
    public enum ArmyJuiceEnum
    {
        None,
        Health,
        Prestige,
        SpawnIn,
        Consumable
    }
    [System.Serializable] public struct ArmyJuice
    {
        public string uniqueID;
        public ArmyJuiceEnum armyJuiceEnum;
        public int value;
    }
    public class ArmyJuiceManager : MonoBehaviour
    {
        CampaignSaveManager campaignSaveManager;
        MapSceneUIManager mapSceneUIManager;
        [SerializeField] List<ArmyJuice> armyJuices = new();
        [SerializeField] List<GearID> newGears = new();
        [SerializeField] List<int> newConsumablesIndexes = new();
        public void SetUp(CampaignSaveManager _campaignSaveManager, MapSceneUIManager _mapSceneUIManager)
        {
            campaignSaveManager = _campaignSaveManager;
            mapSceneUIManager = _mapSceneUIManager;
            campaignSaveManager.OnArmyStructureChanged += ArmyStructureChanged;
        }
        public void UpdateSquadOnChange(ArmyJuice _armyJuice)
        {
            // Debug.Log($"ArmyJuiceManager adding ArmyJuice: {_armyJuice.uniqueID}, Enum: {_armyJuice.armyJuiceEnum}, Value: {_armyJuice.value}");
            armyJuices.Add(_armyJuice);
        }
        private void ArmyStructureChanged()
        {
            // Debug.Log("Army structure changed applying juices: " + armyJuices.Count);
            for (int i = 0; i < armyJuices.Count; i++)
            {
                mapSceneUIManager.HUDPanel.DisplayJuiceOnSquad(armyJuices[i]);
            }
            armyJuices.Clear();
            // Debug.Log($"Cleared armyJuices list. Count: {armyJuices.Count}");
        }
        public void MarkGearAsNew(GearID _gearID)
        {
            // Debug.Log($"ArmyJuiceManager adding new gear: {_gearID}");
            newGears.Add(_gearID);
        }
        public void GearReloaded(GearDisplay[] gearDisplays)
        {
            // Debug.Log("Reloading new gears: " + newGears.Count);
            for (int i = 0; i < gearDisplays.Length; i++)
            {
                if (newGears.Contains(gearDisplays[i].GearID))
                {
                    gearDisplays[i].AquireGearJuice();
                    IAudioRequester.Instance.PlaySFX(SFXData.AddGear);
                }
            }
            newGears.Clear();
        }
        public void MarkConsumableAsNew(int _consumablesLength)
        {
            newConsumablesIndexes.Add(_consumablesLength);
            // Debug.Log($"ArmyJuiceManager adding new consumable: {newConsumablesIndexes.Count}");
        }
        public void ConsumableReloaded(ConsumableUI[] consumableUIs)
        {
            // Debug.Log("Reloading new consumables: " + newConsumablesIndexes.Count);
            for (int i = 0; i < consumableUIs.Length; i++)
            {
                if (newConsumablesIndexes.Contains(i))
                {
                    consumableUIs[i].AcquireConsumableJuice();
                    IAudioRequester.Instance.PlaySFX(SFXData.Discard);
                }
            }
            newConsumablesIndexes.Clear();
        }
        private void OnDestroy() 
        {
            if(campaignSaveManager != null)
                campaignSaveManager.OnArmyStructureChanged -= ArmyStructureChanged;
        }
    }
}