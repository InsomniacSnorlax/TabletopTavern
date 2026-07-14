using TJ.Map;
using TJ;
using UnityEngine;
using UnityEngine.UI;
using Memori.Utilities;
using Memori.Audio;
using System.Threading.Tasks;
using System.Collections.Generic;
using Memori.SaveData;
using System;
using MoreMountains.Feedbacks;

namespace TJ.Prestige
{
    public class PrestigeTraitPanel : MapPanel
    {
        [SerializeField] private SquadBattleInfo squadBattleInfo;
        [SerializeField] private Transform traitCardParent;
        [SerializeField] private PrestigeTraitCard traitCardPrefab;

        [Header("Unit Prefab")]
        [SerializeField] private Transform prefabHolder;
        [SerializeField] private MMF_Player dropInAnimation;
        [SerializeField] private Camera troopCamera;
        [SerializeField] private GameObject troopLights;
        [SerializeField] private RawImage troopImage;

        MemoriCanvasGroup memoriCanvasGroup;
        CampaignSaveManager campaignSaveManager;
        MapSceneUIManager mapSceneUIManager;

        readonly List<PrestigeTraitCard> traitCards = new();
        SquadToLoad currentSquad;
        Action onResolved;

        Transform prefabObject;
        RaceBasePrefab baseObject;
        string loadedRecruitmentPrefabKey;
        System.Threading.CancellationTokenSource hoverCts;

        private void Awake()
        {
            memoriCanvasGroup = GetComponent<MemoriCanvasGroup>();
            memoriCanvasGroup.CGDisable();
            squadBattleInfo.gameObject.SetActive(false);
        }
        public void SetUp(CampaignSaveManager _campaignSaveManager, MapSceneUIManager _mapSceneUIManager)
        {
            campaignSaveManager = _campaignSaveManager;
            mapSceneUIManager = _mapSceneUIManager;
        }
        public void LoadPrestigeTraitPanel(SquadToLoad squad, List<UnitAttribute> eligibleTraits, Action _onResolved)
        {
            currentSquad = squad;
            onResolved = _onResolved;

            squadBattleInfo.gameObject.SetActive(true);
            SquadToLoad displaySquad = new(squad.UnitName, 1);
            squadBattleInfo.SetUpCampaign(displaySquad, Team.Player);
            LoadUnitPrefabAsync(squad.UnitName);

            ClearCards();
            foreach (UnitAttribute trait in eligibleTraits)
            {
                PrestigeTraitCard card = Instantiate(traitCardPrefab, traitCardParent);
                card.LoadTraitCard(trait);
                card.OnTraitCardSelected += SelectTrait;
                traitCards.Add(card);
            }

            memoriCanvasGroup.CGEnable();
            OpenFeedback.PlayFeedbacks();
            IAudioRequester.Instance.PlaySFX(SFXData.OpenUI);
        }
        private async void SelectTrait(UnitAttribute trait)
        {
            foreach (PrestigeTraitCard card in traitCards)
            {
                card.OnTraitCardSelected -= SelectTrait;
                card.NotifyOfSelection(trait);
            }

            campaignSaveManager.ResolvePrestigeTraitChoice(currentSquad.UniqueID, trait);
            IAudioRequester.Instance.PlaySFX(SFXData.PrestigeUnit);

            memoriCanvasGroup.FadeOutAsync(0.25f);
            await Task.Delay(250);

            Action resolved = onResolved;
            ClosePanel();
            resolved?.Invoke();
        }
        public override void ClosePanel()
        {
            memoriCanvasGroup.CGDisable();
            CloseFeedback();
            squadBattleInfo.gameObject.SetActive(false);
            HideUnitPrefab();
            ClearCards();
        }
        private void ClearCards()
        {
            foreach (PrestigeTraitCard card in traitCards)
                if (card != null) Destroy(card.gameObject);
            traitCards.Clear();
        }

        private async void LoadUnitPrefabAsync(UnitName unitName)
        {
            hoverCts?.Cancel();
            hoverCts?.Dispose();
            hoverCts = new System.Threading.CancellationTokenSource();
            var token = hoverCts.Token;

            string key = TabletopTavernData.Instance.GetRecruitmentPrefabKey(unitName);
            GameObject prefab = await TabletopTavernData.Instance.LoadRecruitmentPrefabAsync(unitName);

            if (token.IsCancellationRequested)
            {
                AddressablesManager.Instance.Release(key);
                return;
            }

            if (loadedRecruitmentPrefabKey == key)
                loadedRecruitmentPrefabKey = null;
            LoadUnitPrefab(prefab, unitName);
            loadedRecruitmentPrefabKey = key;
        }
        private void LoadUnitPrefab(GameObject prefab, UnitName unitName)
        {
            ClearUnitPrefab();

            Race race = TabletopTavernData.Instance.GetRaceFromUnitName(unitName);
            bool bigBase = TabletopTavernData.Instance.GetUnitSizeFromUnitName(unitName) != UnitSize.Infantry;

            prefabObject = Instantiate(prefab, prefabHolder).GetComponent<Transform>();
            prefabObject.localPosition = Vector3.zero;
            baseObject = Instantiate(TabletopTavernData.Instance.GetRaceData(race).RaceBasePrefab, prefabHolder);
            baseObject.transform.localPosition = Vector3.zero;
            baseObject.SetUp(true, null);
            dropInAnimation.PlayFeedbacks();
            baseObject.transform.localScale = bigBase ? new Vector3(2f, 1f, 2f) : Vector3.one;

            prefabObject.SetParent(baseObject.transform);

            troopCamera.enabled = true;
            troopImage.enabled = true;
            troopLights.SetActive(true);
        }
        private void ClearUnitPrefab()
        {
            if (prefabObject != null)
                Destroy(prefabObject.gameObject);
            if (baseObject != null)
                Destroy(baseObject.gameObject);
            ReleaseRecruitmentPrefab();
        }
        private void HideUnitPrefab()
        {
            if (prefabObject != null)
                Destroy(prefabObject.gameObject);
            ReleaseRecruitmentPrefab();
            troopCamera.enabled = false;
            troopLights.SetActive(false);
            troopImage.enabled = false;
        }
        private void ReleaseRecruitmentPrefab()
        {
            if (loadedRecruitmentPrefabKey == null) return;
            AddressablesManager.Instance.Release(loadedRecruitmentPrefabKey);
            loadedRecruitmentPrefabKey = null;
        }
    }
}
