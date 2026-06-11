using Memori.Audio;
using Memori.SaveData;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TJ
{
    public class DepositGoldPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _depositAmountText, _totalDepositedText, _startAmountText;
        [SerializeField] private Button _confirmButton, _cancelButton, _minusButton, _plusButton;
        [SerializeField] private CanvasGroup _minusButtonCanvasGroup, _plusButtonCanvasGroup;

        private int _startAmount;
        private int _initialStartAmount;
        private int _depositAmount = 0;
        private int _initialDepositAmount = 0;
        private int _sessionDepositedAmount = 0; // gold already confirmed this town visit

        // Long-press settings
        [Header("Long Press Settings")]
        [SerializeField, Tooltip("Time before repeat starts (seconds)")] 
        private float initialHoldDelay = 0.4f;
        
        [SerializeField, Tooltip("Repeat interval while held (seconds)")] 
        private float repeatInterval = 0.5f;

        private Coroutine _plusHoldRoutine;
        private Coroutine _minusHoldRoutine;

        private void Awake()
        {
            // Clear previous listeners
            _confirmButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
            _minusButton.onClick.RemoveAllListeners();
            _plusButton.onClick.RemoveAllListeners();

            // Single clicks
            _confirmButton.onClick.AddListener(ConfirmDeposit);
            _cancelButton.onClick.AddListener(CancelDeposit);
            _minusButton.onClick.AddListener(() => AdjustDepositAmount(-1));
            _plusButton.onClick.AddListener(() => AdjustDepositAmount(1));

            // Long-press support
            AddHoldEvents(_plusButton, OnPlusPointerDown, OnPlusPointerUp);
            AddHoldEvents(_minusButton, OnMinusPointerDown, OnMinusPointerUp);
        }

        private void AddHoldEvents(Button button, UnityEngine.Events.UnityAction onDown, UnityEngine.Events.UnityAction onUp)
        {
            var trigger = button.gameObject.GetComponent<EventTrigger>() 
                ?? button.gameObject.AddComponent<EventTrigger>();

            // Pointer Down
            var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entryDown.callback.AddListener((data) => onDown?.Invoke());
            trigger.triggers.Add(entryDown);

            // Pointer Up
            var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entryUp.callback.AddListener((data) => onUp?.Invoke());
            trigger.triggers.Add(entryUp);

            // Pointer Exit (safety if finger/mouse leaves button)
            var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((data) => onUp?.Invoke());
            trigger.triggers.Add(entryExit);
        }

        private void OnPlusPointerDown()  => StartHoldRoutine(ref _plusHoldRoutine, 1);
        private void OnPlusPointerUp()    => StopHoldRoutine(ref _plusHoldRoutine);

        private void OnMinusPointerDown() => StartHoldRoutine(ref _minusHoldRoutine, -1);
        private void OnMinusPointerUp()   => StopHoldRoutine(ref _minusHoldRoutine);

        private void StartHoldRoutine(ref Coroutine routine, int direction)
        {
            StopHoldRoutine(ref routine);
            routine = StartCoroutine(HoldRepeatRoutine(direction));
        }

        private void StopHoldRoutine(ref Coroutine routine)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        private IEnumerator HoldRepeatRoutine(int direction)
        {
            // Initial delay before first repeat
            yield return new WaitForSeconds(initialHoldDelay);

            while (true)
            {
                AdjustDepositAmount(direction);
                yield return new WaitForSeconds(repeatInterval);
            }
        }

        private void AdjustDepositAmount(int amount)
        {
            int newDeposit = _depositAmount + amount;
            int newStart   = _startAmount - amount;

#if DEMO
            if(newDeposit + _initialDepositAmount > TabletopTavernConstants.MAX_DEMO_DEPOSITED_GOLD)
            {
                _plusButtonCanvasGroup.alpha = 0.1f;
                _plusButtonCanvasGroup.interactable = false;
                return;
            }
#endif

            if (newDeposit < 0 || newStart < 0)
                return;

            // Can't withdraw more than the gold we had when the popup first opened
            if (newStart > _initialStartAmount)
                return;

            _depositAmount = newDeposit;
            _startAmount   = newStart;

            UpdateDepositAmountText();
            UpdateStartAmountText();
            UpdateCanvasGroupInteractivity();
            IAudioRequester.Instance.PlaySFX(SFXData.AquireGold);
        }

        private void UpdateDepositAmountText()
        {
            _depositAmountText.text = _depositAmount.ToString();
#if DEMO
            _totalDepositedText.text = $"{_depositAmount + _initialDepositAmount} / {TabletopTavernConstants.MAX_DEMO_DEPOSITED_GOLD} (Demo MAX)";
#else
            _totalDepositedText.text = $"{_depositAmount + _initialDepositAmount}";
#endif

        }
        private void UpdateStartAmountText()   => _startAmountText.text   = _startAmount.ToString();

        public void ResetSession() => _sessionDepositedAmount = 0;

        public void Open()
        {
            gameObject.SetActive(true);
            _initialDepositAmount = SaveDataHandler.LoadPlayerSaveData().depositedGold;

            int currentGold = CampaignManager.Instance.EconomyManager.CurrentGoldAmount;
            // Budget = gold on hand now plus what was already confirmed this visit
            _initialStartAmount = currentGold + _sessionDepositedAmount;
            _startAmount = currentGold;
            // Pre-load the last confirmed amount so the popup matches what the player last set
            _depositAmount = _sessionDepositedAmount;

            UpdateDepositAmountText();
            UpdateStartAmountText();
            UpdateCanvasGroupInteractivity();
            CampaignManager.Instance.EconomyManager.OnGoldAmountChangedEconomyManager += UpdateInitialStartAmount;
        }

        private void ConfirmDeposit()
        {
            int delta = _depositAmount - _sessionDepositedAmount;
            if (delta > 0)
                CampaignManager.Instance.EconomyManager.SpendGold(delta);
            else if (delta < 0)
                CampaignManager.Instance.CampaignSaveManager.ModifyGold(-delta);

            CampaignSaveData campaignSaveData = CampaignManager.Instance.CampaignSaveManager.SaveData;
            campaignSaveData.RunStats.goldDeposited += delta;
            CampaignManager.Instance.CampaignSaveManager.SaveCampaign();

            PlayerSaveData playerSaveData = SaveDataHandler.LoadPlayerSaveData();
            playerSaveData.goldToDeposit += delta;
            SaveDataHandler.SavePlayerSaveData(playerSaveData);

            _sessionDepositedAmount = _depositAmount;

            CampaignManager.Instance.EconomyManager.OnGoldAmountChangedEconomyManager -= UpdateInitialStartAmount;
            gameObject.SetActive(false);
        }

        public void UpdateInitialStartAmount(int goldAmount)
        {
            _initialStartAmount = goldAmount + _sessionDepositedAmount;

            if (_depositAmount > goldAmount)
            {
                _depositAmount = goldAmount;
                UpdateDepositAmountText();
            }

            _startAmount = goldAmount - _depositAmount;
            UpdateStartAmountText();
            UpdateCanvasGroupInteractivity();
        }

        public void CancelDeposit()
        {
            CampaignManager.Instance.EconomyManager.OnGoldAmountChangedEconomyManager -= UpdateInitialStartAmount;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            StopHoldRoutine(ref _plusHoldRoutine);
            StopHoldRoutine(ref _minusHoldRoutine);
        }
        private void UpdateCanvasGroupInteractivity()
        {
            // Update plus button interactivity
#if DEMO
            if (_startAmount <= 0 || _depositAmount + _initialDepositAmount >= TabletopTavernConstants.MAX_DEMO_DEPOSITED_GOLD)
            {
                _plusButtonCanvasGroup.alpha = 0.1f;
                _plusButtonCanvasGroup.interactable = false;
            }
            else
            {
                _plusButtonCanvasGroup.alpha = 1f;
                _plusButtonCanvasGroup.interactable = true;
            }
#endif

            if (_depositAmount <= 0)
            {
                _minusButtonCanvasGroup.alpha = 0.1f;
                _minusButtonCanvasGroup.interactable = false;
            }
            else
            {
                _minusButtonCanvasGroup.alpha = 1f;
                _minusButtonCanvasGroup.interactable = true;
            }
        }
    }
}