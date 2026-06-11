using Memori.UI;
using TMPro;
using UnityEngine;
using Memori.SaveData;
using System.Threading.Tasks;
using Memori.Audio;
using System.Collections;
using Memori.Localization;

namespace TJ.Map
{
public class TutorialManager : Memori.Utilities.Singleton<TutorialManager>
{
    [Header("Active Tutorial Steps")]
    [SerializeField] private TutorialStep[] activeTutorialSteps;
    [SerializeField] private TutorialStepEnum activeStepEnum;
    [SerializeField] private int activeStepNumber;

    [Header("Main Tutorial Panel")]
    [SerializeField] private TMP_Text stepDescription;
    [SerializeField] private Animator tutorialPanelAnimator;
    [SerializeField] private MemoriButtonV2 completeStepButton;
    // [SerializeField] private ImageHighlighter closeButtonHighlighter;

    [Header("Tooltip")]
    [SerializeField] private Animator tutorialTooltipAnimator;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private MemoriButtonV2 completeTooltipButton;
    [SerializeField] private Transform tutorialTooltipPlacementTransform;

    [Header("Scene")]
    [SerializeField] private GameObject sceneGameObjects;

    private void Start()
    {
        tutorialPanelAnimator.gameObject.SetActive(false);
        tutorialTooltipAnimator.gameObject.SetActive(false);

        completeStepButton.Button.onClick.AddListener(LocalCompleteStep);
        completeTooltipButton.Button.onClick.AddListener(LocalCompleteTooltip);
    }
    public async void DelayedStart()
    {
        PlayerSaveData saveData = SaveDataHandler.LoadPlayerSaveData();
        if(saveData.tutorialStepCompleted.Contains(TutorialData.MoveCamera.stepID)) return;

        await Task.Delay(3000);

        LoadStepsFromRandomSpot(new TutorialStep[3] { TutorialData.MoveCamera, TutorialData.RotateCamera, TutorialData.SelectNode });
        bool firstStepCompleted = false;
        while(firstStepCompleted == false){
            await Task.Delay(500);

            if(UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.D)){
                firstStepCompleted = true;
            }
        }
        CompleteStepCheck(TutorialStepEnum.MoveCamera);
        firstStepCompleted = false;
        while(firstStepCompleted == false){
            await Task.Delay(500);

            if(UnityEngine.Input.GetMouseButton(2)){
                firstStepCompleted = true;
            }
        }
        CompleteStepCheck(TutorialStepEnum.RotateCamera);
    }
    public void LoadStepsFromRandomSpot(TutorialStep[] _tutorialSteps)
    {
        PlayerSaveData saveData = SaveDataHandler.LoadPlayerSaveData();
        if(saveData.tutorialStepCompleted.Contains(_tutorialSteps[^1].stepID)) return;

        stepDescription.text = "";
        completeStepButton.gameObject.SetActive(false);

        activeTutorialSteps = _tutorialSteps;
        tutorialPanelAnimator.gameObject.SetActive(true);
        tutorialPanelAnimator.SetBool("Active", true);

        activeStepNumber = 0;
        StartCoroutine(DelayedLoadText());
    }
    public IEnumerator DelayedLoadText()
    {
        yield return new WaitForSeconds(0.5f);
        LoadStep();
    }
    public void LoadStep()
    {
        stepDescription.text = LocalizationManager.Instance.GetText($"tutorialStep{activeTutorialSteps[activeStepNumber].stepID}Desc");
        activeStepEnum = activeTutorialSteps[activeStepNumber].tutorialStepEnum;

        completeStepButton.gameObject.SetActive(true);
        // closeButtonHighlighter.FlashHighlightImage();
 
        sceneGameObjects.SetActive(true);
    }
    public void LoadTooltip(TutorialStep _tutorialStep, Transform _tooltipPlacementTransform)
    {
        PlayerSaveData saveData = SaveDataHandler.LoadPlayerSaveData();
        if(saveData.tutorialStepCompleted.Contains(_tutorialStep.stepID)) return;

        saveData.tutorialStepCompleted.Add(_tutorialStep.stepID);
        SaveDataHandler.SavePlayerSaveData(saveData);

        tooltipText.text = LocalizationManager.Instance.GetText($"tutorialStep{_tutorialStep.stepID}Desc");

        tutorialTooltipAnimator.SetBool("Active", true);
        tutorialTooltipAnimator.gameObject.SetActive(true);
        tutorialTooltipPlacementTransform.transform.position = _tooltipPlacementTransform.position;
        completeTooltipButton.gameObject.SetActive(true);
        sceneGameObjects.SetActive(true);
    }
    public void CompleteStepCheck(TutorialStepEnum _tutorialStepEnum)
    {
        if(_tutorialStepEnum != activeStepEnum) return;

        LocalCompleteStep();
    }
    private void LocalCompleteStep()
    {
        PlayerSaveData saveData = SaveDataHandler.LoadPlayerSaveData();
        saveData.tutorialStepCompleted.Add(activeTutorialSteps[activeStepNumber].stepID);
        SaveDataHandler.SavePlayerSaveData(saveData);

        activeStepNumber++;
        completeStepButton.gameObject.SetActive(false);
        stepDescription.text = "";

        if(activeStepNumber == activeTutorialSteps.Length) {
            tutorialPanelAnimator.SetBool("Active", false);
            activeStepEnum = TutorialStepEnum.Blank;
            return;
        }

        tutorialPanelAnimator.SetBool("Next", true);
        LoadStep();
    }
    private void LocalCompleteTooltip()
    {
        tutorialTooltipAnimator.SetBool("Active", false);
        completeTooltipButton.gameObject.SetActive(false);
    }
    public void TurnOff()
    {
        tutorialPanelAnimator.SetBool("Active", false);
        tutorialTooltipAnimator.SetBool("Active", false);
        sceneGameObjects.SetActive(false);
        completeTooltipButton.gameObject.SetActive(false);
        completeStepButton.gameObject.SetActive(false);
    }
}
}
