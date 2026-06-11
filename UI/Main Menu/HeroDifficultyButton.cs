using UnityEngine;
using UnityEngine.UI;
using Memori.SaveData;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Memori.UI;
using Memori.Audio;
using Memori.Tooltip;
using Memori.Localization;
using TMPro;

namespace TJ.MainMenu
{
    [RequireComponent(typeof(MemoriTooltipTrigger))]
    public class HeroDifficultyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image heroImage, heroFrameImage, selectedFrameImage, maxDifficultyFrameImage, maxDifficultyBackgroundImage;
        [SerializeField] private TMP_Text maxDifficultyText;
        [SerializeField] private GameObject completedGO, lockedGO, selectedGO;
        Hero _hero;
        PlayPanel _playPanel;
        List<int> _difficultiesCompletedForHero;
        int _playerSaveMaxDifficultyCompletedOverall;
        bool _hasAccessToHero;
        MemoriTooltipTrigger memoriTooltipTrigger;
        enum ButtonType { HeroSelection, HeroDifficulty }
        [SerializeField] private ButtonType buttonType;
        bool listenerSetUp = false;
        private UnlockCondition _unlockCondition;

        public void LoadHeroSelectionPage(Hero hero, PlayPanel playPanel)
        {
            // Debug.Log($"Loading hero selection page for hero: {hero.HeroID}");
            buttonType = ButtonType.HeroSelection;
            if(memoriTooltipTrigger == null)
                memoriTooltipTrigger = GetComponent<MemoriTooltipTrigger>();

            _hero = hero;
            _playPanel = playPanel;

            LoadSprite();

            #if DEMO
                _unlockCondition = _hero.DemoUnlockCondition;
            #else
                _unlockCondition = _hero.UnlockCondition;
            #endif

            _hasAccessToHero = SaveDataHandler.IsUnlockConditionUnlocked(_unlockCondition, hero.HeroID);

#if DEMO
            if(SaveDataHandler.IsDevToolUser()) _hasAccessToHero = true;
#endif

            _difficultiesCompletedForHero = SaveDataHandler.GetHeroDifficultiesCompleted(hero.HeroID);
            //Display Max Difficulty
            int maxDifficultyCompleted = -1;
            foreach(int difficulty in _difficultiesCompletedForHero)
            {
                if(difficulty > maxDifficultyCompleted)
                    maxDifficultyCompleted = difficulty;
            }
            if(maxDifficultyCompleted >= 0)
            {
                string romanNumeral = MemoriUI.ConvertNumberToRomanNumeral(maxDifficultyCompleted);
                maxDifficultyText.text = $"{romanNumeral}";
                completedGO.SetActive(true);
            }
            else
            {
                maxDifficultyText.text = "";
                completedGO.SetActive(false);
            }

            maxDifficultyBackgroundImage.enabled = maxDifficultyCompleted == 10;
            maxDifficultyFrameImage.enabled = maxDifficultyCompleted == 10;

            // _playerSaveMaxDifficultyCompletedOverall = SaveDataHandler.LoadPlayerSaveData().MaxDifficultyOverall;
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(OnHeroSelectionButtonClicked);

            ShowVisuals(_hasAccessToHero);
            selectedFrameImage.enabled = false;
            selectedGO.SetActive(false);

            //setup tooltip
            SetUpTooltip(_hasAccessToHero, HeroBonusManager.GetLocalizedHeroUnlockDescription(_hero, _unlockCondition));
            selectedFrameImage.enabled = false;
            selectedGO.SetActive(false);
            if(!listenerSetUp)
            {
                _playPanel.OnActiveHeroChanged += OnActiveHeroChanged;
                listenerSetUp = true;
            }
        }
        public async void LoadSprite()
        {
            Sprite loadedSprite = await TabletopTavernData.Instance.LoadHeroSpriteAsync(_hero.HeroID);
            if (this == null) return;
            if (loadedSprite == null) Debug.LogError($"[HeroDifficultyButton] Sprite null for hero {_hero.HeroID}");
            heroImage.sprite = loadedSprite;
        }
        public void CheckDifficultyStatus(int difficultyLevel)
        {
            if(!_hasAccessToHero)
            {
                completedGO.SetActive(false);
                ShowVisuals(false);
                return;
            }

            //for peasant just check if we have access
            if(difficultyLevel == (int)TT_Difficulty.Peasant)
            {
                completedGO.SetActive(_difficultiesCompletedForHero.Contains(difficultyLevel));
                ShowVisuals(_hasAccessToHero);
                return;
            }
            
            //otherwise, check if max hero difficulty completed overall is atleast this difficulty level -1
            // Debug.Log($"Checking difficulty status for level: {difficultyLevel} for hero: {_hero.HeroID} max completed overall: {_playerSaveMaxDifficultyCompletedOverall}");

            if(difficultyLevel - 1 > _playerSaveMaxDifficultyCompletedOverall)
            {
                completedGO.SetActive(false);
                ShowVisuals(false);
                string unlockRequirementDescription = LocalizationManager.Instance.GetText("Previous difficulty level not completed");
                SetUpTooltip(false, unlockRequirementDescription);
                return;
            }

            completedGO.SetActive(_difficultiesCompletedForHero.Contains(difficultyLevel));
            ShowVisuals(true);
        }
        public void SetUpTooltip(bool hasAccessToHero, string unlockRequirementDescription = "")
        {
            if (hasAccessToHero)
            {
                memoriTooltipTrigger.enabled = false;
            }
            else
            {
                string lockedText = LocalizationManager.Instance.GetText("Locked");
                memoriTooltipTrigger.SetUpToolTip(lockedText, unlockRequirementDescription);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IAudioRequester.Instance.PlaySFX(SFXData.HoverHero);
            MemoriUI.BloomItemScale(transform, 1.025f, 0.1f);

            if(buttonType == ButtonType.HeroSelection)
                _playPanel.ShowHeroDetailsBox(_hero);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            MemoriUI.BloomItemScale(transform, 1f, 0.1f);
            
            if(buttonType == ButtonType.HeroSelection)
                _playPanel.RevertToActiveHeroDetailsBox(_hero);
        }
        private void ShowVisuals(bool unlockedAtThisDifficulty)
        {
            heroFrameImage.color = unlockedAtThisDifficulty ? Color.white : Color.gray;
            heroImage.color = unlockedAtThisDifficulty ? Color.white : Color.gray;
            lockedGO.SetActive(!unlockedAtThisDifficulty);
        }
        public void OnHeroSelectionButtonClicked()
        {
            _playPanel.LoadHeroes(_hero, true);
        }
        public void OnActiveHeroChanged(Hero activeHero)
        {
            if(activeHero.HeroID == _hero.HeroID)
            {
                selectedFrameImage.enabled = true;
                selectedGO.SetActive(true);
            }
            else
            {
                selectedFrameImage.enabled = false;
                selectedGO.SetActive(false);
            }
        }
        private void OnDestroy() 
        {
            if(_playPanel != null && listenerSetUp)
                _playPanel.OnActiveHeroChanged -= OnActiveHeroChanged;
        }
    }
}