using UnityEngine;
using UnityEngine.UI;
using Memori.SaveData;
using TMPro;
using Memori.UI;
using Memori.Utilities;
using Memori.Scenes;
using Memori.Audio;
using Memori.Localization;
using System.Threading.Tasks;
using TabletopAnalytics;
using Memori.Notifications;
using MoreMountains.Feedbacks;

namespace TJ.Map
{
[RequireComponent(typeof(MemoriCanvasGroup))]
public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text demoCompletionText;
    [SerializeField] private TMP_Text heroNameText, difficultyNameText;
    [SerializeField] private GameObject[] difficultyCrests;

    [Header("Game Over Stats")]
    [SerializeField] private TMP_Text chaptersCompletedText;
    [SerializeField] private TMP_Text goldEarnedText, goldDepositedText, goldDepositedOutcomeText;
    [SerializeField] private MemoriCanvasGroup completionMessageRow, heroNameRow, difficultyRow, backgroundRow;
    [SerializeField] private MemoriCanvasGroup chaptersCompletedRow, goldEarnedRow, goldDepositedRow;
    public MemoriButtonV2 mainMenuButton;
    [SerializeField] private MemoriCanvasGroup mainGameOverGroup, textGroup, fadeCanvasGroup;
    [SerializeField] private GameObject defeatObject, victoryObject;
    [SerializeField] private MMF_Player crestSpawnFeedback;

    [Header("Act Complete")]
    [SerializeField] private GameObject actCompleteObject;
    [SerializeField] private MemoriButtonV2 continueButton;
    [SerializeField] private TMP_Text actCompleteTextPart1, actCompleteTextPart2,  actCompleteTextPart3;

    [Header("Hero Unlock")]
    [SerializeField] private MemoriCanvasGroup heroUnlockRow;
    [SerializeField] private TMP_Text unlockedHeroNameText;
    [SerializeField] private Image unlockedHeroImage;
    [SerializeField] private MemoriButtonV2 heroUnlockContinueButton;

    TT_Difficulty _difficulty;
    bool _unlocksNewHero;
    int _unlockedHeroID;

    public void Start()
    {
        mainGameOverGroup.CGDisable();
        textGroup.CGDisable();
        fadeCanvasGroup.CGDisable();
        victoryObject.SetActive(false);
        defeatObject.SetActive(false);
        actCompleteObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        if (heroUnlockRow != null) heroUnlockRow.CGDisable();
        if (heroUnlockContinueButton != null) heroUnlockContinueButton.gameObject.SetActive(false);
    }

    public void RecordGameOver(bool _beatDemo)
    {
        CampaignSaveData saveData = CampaignManager.Instance.CampaignSaveManager.SaveData;
        RunStats runStats = saveData.RunStats;

        _unlocksNewHero = false;
        if (_beatDemo)
        {
            int currentHeroID = saveData.heroID;
            bool isFirstCompletion = SaveDataHandler.GetHeroDifficultiesCompleted(currentHeroID).Count == 0;
            if (isFirstCompletion)
            {
                Hero nextHero = HeroData.GetHeroByID(currentHeroID + 1);
                if (nextHero.HeroID == currentHeroID + 1 && nextHero.UnlockCondition == UnlockCondition.HeroCompletion)
                {
                    _unlocksNewHero = true;
                    _unlockedHeroID = nextHero.HeroID;
                    string unlockedLocalized = LocalizationManager.Instance.GetText("New Hero Unlocked");
                    string heroLocalized = LocalizationManager.Instance.GetText(nextHero.HeroName);
                    unlockedHeroNameText.text = $"<color={ColorData.Secondary}>{unlockedLocalized}</color> <color={ColorData.Primary}>{heroLocalized}</color>";
                }
            }
        }

        SaveDataHandler.RecordGameOver(_beatDemo);

        string heroNameLocalized = LocalizationManager.Instance.GetText(HeroData.GetHeroByID(saveData.heroID).HeroName);
        heroNameText.text = heroNameLocalized;
        _difficulty = saveData.difficultyLevel;

        //get selected difficulty data
        DifficultyLevel difficultyData = DifficultyData.GetDifficultyLevelData(_difficulty);
        string levelLocalized = LocalizationManager.Instance.GetText("Level");
        string difficultyNamestring = LocalizationManager.Instance.GetText(difficultyData.difficultyName);
        difficultyNameText.text = $"{levelLocalized} {(int)_difficulty}: {difficultyNamestring}";

        chaptersCompletedText.text = runStats.chaptersCompleted.ToString();
        goldEarnedText.text = runStats.goldEarned.ToString();

        int bonusGold = _beatDemo ? saveData.goldAmount : 0;
        string bonusColor = _beatDemo ? ColorData.Green : ColorData.Error;
        goldDepositedText.text = $"{runStats.goldDeposited} <color={bonusColor}>+ {bonusGold}</color> = {runStats.goldDeposited + bonusGold}";
        goldDepositedOutcomeText.text = $"<color={bonusColor}>{LocalizationManager.Instance.GetText(_beatDemo ? "Victory" : "Defeat")}</color>";

        // AnalyticsManager.Instance.LogRunEnd(
        //     saveData.runUUID.ToString(),
        //     _beatDemo ? RunResult.Win : RunResult.Loss,
        //     runStats.chaptersCompleted,
        //     saveData.goldAmount,
        //     runStats.enemiesSlain
        // );

        CampaignManager.Instance.CampaignSaveManager.DeleteCampaignSave();
    }
    public async void DisplayGameOver(bool beatDemo = false)
    {
        string demoCompletedLocalized = LocalizationManager.Instance.GetText("Demo Completed");
        string defeatedLocalized = LocalizationManager.Instance.GetText("Defeated");
        demoCompletionText.text = beatDemo ? demoCompletedLocalized : defeatedLocalized;
        IAudioRequester.Instance.SwitchToGameOverMusic(beatDemo);
        mainGameOverGroup.CGEnable();
        defeatObject.SetActive(!beatDemo);
        victoryObject.SetActive(beatDemo);

        mainMenuButton.Button.onClick.RemoveAllListeners();
        mainMenuButton.Button.onClick.AddListener(() => ExitAfterFadeOut());
        
        for (int i = 0; i < difficultyCrests.Length; i++)
        {
            difficultyCrests[i].SetActive(i == (int)_difficulty - 1);
        }
        
        await Task.Delay(500);
        if (_unlocksNewHero && heroUnlockRow != null)
            await ShowHeroUnlockScreen();
        await FadeInStatsSequentially();
    }

    private async Task FadeInStatsSequentially()
    {
        textGroup.CGEnable();

        MemoriCanvasGroup[] rows = { backgroundRow, difficultyRow, heroNameRow, chaptersCompletedRow, goldEarnedRow, goldDepositedRow, completionMessageRow };
        foreach (var row in rows) row.CGDisable();

        const float rowFade = 0.4f;
        const int rowGapMs = 200;

        foreach (var row in rows)
        {
            IAudioRequester.Instance.PlaySFX(SFXData.LightMouseOver);

            if(row == difficultyRow)
                crestSpawnFeedback.PlayFeedbacks();

            await row.FadeIn(rowFade);
            await Task.Delay(rowGapMs);
        }

    }

    private async Task ShowHeroUnlockScreen()
    {
        if (unlockedHeroImage != null)
        {
            Sprite sprite = await TabletopTavernData.Instance.LoadHeroSpriteAsync(_unlockedHeroID);
            if (sprite != null) unlockedHeroImage.sprite = sprite;
        }

        IAudioRequester.Instance.PlaySFX(SFXData.LightMouseOver);
        await heroUnlockRow.FadeIn(0.4f);

        await Task.Delay(300);
        heroUnlockContinueButton.gameObject.SetActive(true);

        var tcs = new TaskCompletionSource<bool>();
        heroUnlockContinueButton.Button.onClick.RemoveAllListeners();
        heroUnlockContinueButton.Button.onClick.AddListener(() => tcs.TrySetResult(true));
        await tcs.Task;

        heroUnlockContinueButton.gameObject.SetActive(false);
        await heroUnlockRow.FadeOut(0.3f);
    }

    public async void DisplayActComplete()
    {
        mainGameOverGroup.CGEnable();
        actCompleteObject.SetActive(true);

        IAudioRequester.Instance.PlaySFX(SFXData.Victory);
        IAudioRequester.Instance.PlaySFX(SFXData.Cheer);

        string actLocalized = LocalizationManager.Instance.GetText("Act");
        string completeLocalized = LocalizationManager.Instance.GetText("Complete");
        int actsCompleted = CampaignManager.Instance.CampaignSaveManager.SaveData.bookNumber;
        string totalText = $"<size=300><cspace=-20>{actLocalized[0]}<size=240><cspace=-4>{actLocalized[1..]} {MemoriUI.ConvertNumberToRomanNumeral(actsCompleted)} {completeLocalized}";
        actCompleteTextPart1.text = totalText;
        actCompleteTextPart2.text = totalText;
        actCompleteTextPart3.text = totalText;

        continueButton.Button.onClick.RemoveAllListeners();
        continueButton.Button.onClick.AddListener(CampaignManager.Instance.MapSceneUIManager.CompleteLayer);

        await Task.Delay(1000);
        continueButton.gameObject.SetActive(true);
    }
    public async void ExitAfterFadeOut()
    {
        await fadeCanvasGroup.FadeIn(1f);
        SceneHandler.Instance.SwitchGameState(GameStateEnum.MainMenu);
    }
}
}
