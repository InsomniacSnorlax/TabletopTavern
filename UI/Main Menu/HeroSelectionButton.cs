// using UnityEngine;
// using UnityEngine.UI;
// using Memori.UI;
// using Memori.SaveData;
// using Memori.Tooltip;
// using Memori.Localization;
// using UnityEngine.EventSystems;
// using Memori.Steamworks;
// using TMPro;

// namespace TJ.MainMenu
// {
//     [RequireComponent(typeof(MemoriTooltipTrigger))]
//     public class HeroSelectionButton : MemoriButtonV2, ISelectHandler, IDeselectHandler
//     {
//         [SerializeField] private Hero hero;
//         [SerializeField] private PlayPanel playPanel;
//         [SerializeField] private Image heroImage;
//         [SerializeField] private GameObject lockedStuff;
//         [SerializeField] private TMP_Text difficultyText;

//         // [SerializeField] private Transform[] difficultyCompletedIcons;
//         MemoriTooltipTrigger memoriTooltipTrigger;

//         private void Awake()
//         {
//             GetComponent<Button>().onClick.AddListener(OnHeroSelectionButtonClicked);
//         }
//         public void LoadHero(Hero _hero, PlayPanel _playPanel)
//         {
//             if(memoriTooltipTrigger == null)
//                 memoriTooltipTrigger = GetComponent<MemoriTooltipTrigger>();
                
//             hero = _hero;
//             heroImage.sprite = SpriteData.GetSprite(hero.Sprite);
//             lockedStuff.SetActive(false);
//             playPanel = _playPanel;

//             bool isUnlocked = SaveDataHandler.IsUnlockConditionUnlocked(hero.UnlockCondition);

//             // if(hero.UnlockCondition == UnlockCondition.BeatDemo)
//             // {
//             //     isUnlocked = SaveDataHandler.IsMetaprogressionNodeUnlocked(playPanel.BaseHeroMetaprogressionModel);
//             // }
//             bool isDev = SteamStatic.IsDevToolUser();
// #if !UNITY_EDITOR
//             if(isDev) {
//                 isUnlocked = true;
//             }
// #endif

//             // int maxDifficultyCompletedForHero = SaveDataHandler.GetHeroDifficultiesCompleted(hero.HeroID);

//             // difficultyText.text = maxDifficultyCompletedForHero > 0 ? MemoriUI.ConvertNumberToRomanNumeral(maxDifficultyCompletedForHero) : "";
//             // for (int i = 0; i < difficultyCompletedIcons.Length; i++)
//             // {
//             //     foreach (Transform child in difficultyCompletedIcons[i].transform)
//             //     {
//             //         Image image = child.GetComponent<Image>();
//             //         if (image != null)
//             //         {
//             //             if (i >= maxDifficultyCompletedForHero)
//             //             {
//             //                 image.enabled = false;
//             //             }
//             //         }
//             //     }
//             // }
//             lockedStuff.SetActive(!isUnlocked);


//             if (isUnlocked)
//             {
//                 memoriTooltipTrigger.enabled = false;
//             }
//             else
//             {
//                 string lockedText = LocalizationManager.Instance.GetText("Locked");
//                 string unlockRequirementDescription = LocalizationManager.Instance.GetText(DataTypes.GetUnlockRequirementDescription(hero.UnlockCondition));

//                 if(hero.UnlockCondition == UnlockCondition.DiscordExclusive || hero.UnlockCondition == UnlockCondition.NewsletterExclusive)
//                 {
//                     string flavorText = LocalizationManager.Instance.GetText("Click to open window to enter unlock code.");
//                     memoriTooltipTrigger.SetUpToolTip(lockedText, unlockRequirementDescription, flavorText);
//                 }
//                 else
//                 {
//                     memoriTooltipTrigger.SetUpToolTip(lockedText, unlockRequirementDescription);
//                 }
//             }
//         }
//         public void OnHeroSelectionButtonClicked()
//         {
//             playPanel.LoadHeroes(hero, true);
//             EventSystem.current.SetSelectedGameObject(gameObject);
//         }
//         public override void OnSelect(BaseEventData eventData)
//         {
//             // Button is now highlighted/selected
//             // Debug.Log(name + " is selected!");
//             if (selectionHighlightImage != null) selectionHighlightImage.enabled = true;
//             // Add custom logic: Play a sound, scale up, etc.
//             // e.g., GetComponent<Animator>().SetTrigger("Highlight");
//         }

//         public override void OnDeselect(BaseEventData eventData)
//         {
//             // Button is no longer highlighted
//             // Debug.Log(name + " is deselected.");
//             if (selectionHighlightImage != null) selectionHighlightImage.enabled = false;
//             // Reset custom effects
//         }
//         public override void OnPointerEnter(PointerEventData eventData)
//         {
//             base.OnPointerEnter(eventData);
//             playPanel.ShowHeroDetailsBox(hero);
//         }
//         public override void OnPointerExit(PointerEventData eventData)
//         {
//             base.OnPointerExit(eventData);
//             playPanel.RevertToActiveHeroDetailsBox(hero);
//         }
// }
// }