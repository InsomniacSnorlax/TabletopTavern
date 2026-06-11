using Memori.Utilities;
using UnityEngine;
using TJ.Map;

namespace TJ
{
    public class CampaignManager : Singleton<CampaignManager>
    {
        [SerializeField] private CampaignSaveManager campaignSaveManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private MapSceneUIManager mapSceneUIManager;
        [SerializeField] private MapCamera mapCamera;
        [SerializeField] private ConsumableManager consumableManager;
        [SerializeField] private GearManager gearManager;
        [SerializeField] private ArmyJuiceManager armyJuiceManager;

        public CampaignSaveManager CampaignSaveManager => campaignSaveManager;
        public EconomyManager EconomyManager => economyManager;
        public MapSceneUIManager MapSceneUIManager => mapSceneUIManager;
        public MapCamera MapCamera => mapCamera;
        public ConsumableManager ConsumableManager => consumableManager;
        public GearManager GearManager => gearManager;
        public ArmyJuiceManager ArmyJuiceManager => armyJuiceManager;
        protected override void Awake()
        {
            base.Awake();
            Debug.Log($"CampaignManager instantiated!\n{System.Environment.StackTrace}");
        }
        private void Start()
        {
            economyManager.SetUp();
        }
}
}
