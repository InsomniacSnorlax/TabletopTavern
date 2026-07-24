using Memori.Tooltip;
using UnityEngine;
using TMPro;
using Memori.SaveData;
using Memori.Metaprogression;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Memori.Notifications;
using Memori.Audio;
using Memori.Localization;
using Memori.Scenes;
using System.Threading.Tasks;
using UnityEngine.Serialization;


namespace TJ.MainMenu
{
    public class UpgradesPanel : MainMenuPanel
    {
        [Header("Main Menu")]
        [SerializeField] private GameObject upgradesAvailableIndicator;

        [Header("Metaprogression Scene")]
        [SerializeField] private Camera _metaprogressionCamera;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Transform cameraSceneParent;

        [Header("UI")]
        [FormerlySerializedAs("_depositedGoldText")]
        [SerializeField] private TMP_Text _renownText;
        [SerializeField] private MetaprogressionManager _metaprogressionManager;
        [SerializeField] private Button _resetButton, _depositButton;
        [SerializeField] private TooltipDropdown _tavernThemeDropdown;
        [SerializeField] private List<TavernThemeData> _tavernThemes;
        [SerializeField] private MemoriTooltipTrigger _tavernTooltipTrigger;

        List<MetaprogressionModel> _unlockedNodes = new();
        MetaprogressionPresenter _selectedNode;
        // Index 0 is always "None"; subsequent entries map 1:1 to _tavernThemes
        int _lastValidThemeIndex = 0;
        int _renownAvailable = 0;
        bool _isOpen = false;

        [ContextMenu("Display Nodes")]
        private void Start()
        {
            _metaprogressionCamera.enabled = false;
            cameraSceneParent.gameObject.SetActive(false);
            _resetButton.onClick.AddListener(ResetMetaprogression);
            _depositButton.onClick.AddListener(OverrideAddRenown);
            _tavernThemeDropdown.onValueChanged.AddListener(OnTavernThemeChanged);

            List<int> unlockedNodeIds = SaveDataHandler.GetUnlockedMetaprogressionNodes();
            _unlockedNodes = GetUnlockedNodesFromIds(unlockedNodeIds);
            CheckForAvailableUpgrades();

            _tavernTooltipTrigger.SetUpToolTip(
                LocalizationManager.Instance.GetText("TavernThemeTooltipTitle"),
                LocalizationManager.Instance.GetText("TavernThemeTooltipDescription")
            );
        }
        void Update()
        {
            if(!_isOpen) return;

            if (EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = _metaprogressionCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject objectHit = hit.transform.gameObject;
                MetaprogressionPresenter presenter = objectHit.GetComponentInParent<MetaprogressionPresenter>();

                if (presenter != null)
                {
                    bool parentIsUnlocked = true;
                    if(presenter.ParentPresenter != null) parentIsUnlocked = presenter.ParentPresenter.IsUnlocked;

                    string costText = parentIsUnlocked ? "" : $"<color {ColorData.Error}>{LocalizationManager.Instance.GetText("upgradesLockedRequiresPreviousNode")}</color>\n";
                    costText += LocalizationManager.Instance.GetText("Cost") + ": " + presenter.MetaprogressionModel.NodeCost.ToString();

                    if(_selectedNode != presenter)
                    {
                        _selectedNode = presenter;
                        IAudioRequester.Instance.PlaySFX(SFXData.ButtonHover);

                        int nodeValue = presenter.MetaprogressionModel.NodeValue;
                        string nodeName = LocalizationManager.Instance.GetText($"metaprogressionModel{presenter.MetaprogressionModel.NodeId}") + (nodeValue != 0 ? $" {nodeValue}" : "") + (presenter.MetaprogressionModel.AddGoldSprite ? " <sprite name=GoldSprite>" : "");
                        
                        _selectedNode.MouseOverHighlight(true);
                        TooltipManager.Instance.LoadToolTip(
                            nodeName,
                            costText,
                            ""
                        );
                    }
                }
                else
                {
                    if(_selectedNode != null)
                        _selectedNode.MouseOverHighlight(false);
                        
                    _selectedNode = null;
                    TooltipManager.Instance.HideTooltip();
                }
            }
            else
            {
                if(_selectedNode != null)
                    _selectedNode.MouseOverHighlight(false);

                _selectedNode = null;
                TooltipManager.Instance.HideTooltip();
            }

            // On left mouse click, attempt to purchase unlock
            if (Input.GetMouseButtonDown(0))
            {
                PurchaseUnlockNode();
            }
        }
        public void PurchaseUnlockNode()
        {
            if(_selectedNode == null) return;

            if(_unlockedNodes.Contains(_selectedNode.MetaprogressionModel))
            {
                NotificationManager.Instance.DisplayNotification(LocalizationManager.Instance.GetText("upgradesNodeAlreadyUnlocked"));
                return;
            }

            if(_selectedNode.ParentPresenter != null && !_unlockedNodes.Contains(_selectedNode.ParentPresenter.MetaprogressionModel))
            {
                NotificationManager.Instance.DisplayNotification(LocalizationManager.Instance.GetText("upgradesRequiresPreviousNodeUnlock"));
                return;
            }

            if(_renownAvailable < _selectedNode.MetaprogressionModel.NodeCost)
            {
                NotificationManager.Instance.DisplayNotification(LocalizationManager.Instance.GetText("upgradesInsufficientRenown"));
                return;
            }

            SaveDataHandler.UnlockMetaprogressionNode(_selectedNode.MetaprogressionModel);
            List<int> unlockedNodeIds = SaveDataHandler.GetUnlockedMetaprogressionNodes();
            _unlockedNodes = GetUnlockedNodesFromIds(unlockedNodeIds);
            _selectedNode.Unlock(false);
            IAudioRequester.Instance.PlaySFX(SFXData.SelectHero);
            CalculateRenownSpent();
            _metaprogressionManager.HighlightAvailableUpgrades(_unlockedNodes, _renownAvailable);
        }
        private void CalculateRenownSpent()
        {
            List<int> unlockedNodeIds = SaveDataHandler.GetUnlockedMetaprogressionNodes();
            _unlockedNodes = GetUnlockedNodesFromIds(unlockedNodeIds);
            int totalSpent = 0;
            foreach(MetaprogressionModel node in _unlockedNodes)
            {
                if(node == null) continue;
                totalSpent += node.NodeCost;
            }
            int renown = SaveDataHandler.GetRenown();
            _renownAvailable = renown - totalSpent;
            _renownText.text =  $"{_renownAvailable}/{renown}";
        }
        public override async void OpenPanel()
        {
            SceneHandler.Instance.TranstionCameras(_mainCamera, _metaprogressionCamera);
            await Task.Delay(500);
            cameraSceneParent.gameObject.SetActive(true);
            this.gameObject.SetActive(true);
            _isOpen = true;
            SetUpTavernThemeDropdown();
            DisplayNodes();
            base.OpenPanel();
        }
        [ContextMenu("Display Nodes")]
        private void DisplayNodes() 
        {
            List<int> unlockedNodeIds = SaveDataHandler.GetUnlockedMetaprogressionNodes();
            _unlockedNodes = GetUnlockedNodesFromIds(unlockedNodeIds);
            _metaprogressionManager.DisplayNodes(_unlockedNodes);
            _metaprogressionManager.HighlightAvailableUpgrades(_unlockedNodes, SaveDataHandler.GetRenown());
            CalculateRenownSpent();
        }
        public override async void ClosePanel()
        {
            TooltipManager.Instance.HideTooltip();
            SceneHandler.Instance.TranstionCameras(_metaprogressionCamera, _mainCamera);
            await Task.Delay(500);
            cameraSceneParent.gameObject.SetActive(false);
            _isOpen = false;
            CheckForAvailableUpgrades();
            base.ClosePanel();
            this.gameObject.SetActive(false);
        }
        private void ResetMetaprogression()
        {
            SaveDataHandler.ResetMetaprogression();
            DisplayNodes();
        }
        private void OverrideAddRenown()
        {
            PlayerSaveData playerSaveData = SaveDataHandler.LoadPlayerSaveData();
            playerSaveData.renown += 100;
            SaveDataHandler.SavePlayerSaveData(playerSaveData);
            CalculateRenownSpent();
        }
        private void CheckForAvailableUpgrades()
        {
            List<int> unlockedNodeIds = SaveDataHandler.GetUnlockedMetaprogressionNodes();
            List<MetaprogressionModel> unlockedNodes = GetUnlockedNodesFromIds(unlockedNodeIds);
            MetaprogressionTreeModel treeModel = _metaprogressionManager.MetaprogressionTreeModel;

            int renown = SaveDataHandler.GetRenown();
            int totalSpent = 0;
            foreach(MetaprogressionModel node in _unlockedNodes)
            {
                if(node == null) continue;
                totalSpent += node.NodeCost;
            }

            foreach(ChildParentPair pair in treeModel.MetaProgressionTree)
            {
                MetaprogressionModel node = pair.Child;
                if(unlockedNodes.Contains(node)) continue;

                //check if parent is unlocked
                if(pair.Parent != null && !unlockedNodes.Contains(pair.Parent)) continue;

                //check if enough renown to unlock
                if(renown - totalSpent >= node.NodeCost)
                {
                    // Debug.Log($"Upgrade available: {node.NodeId} with cost {node.NodeCost}");
                    upgradesAvailableIndicator.SetActive(true);
                    return;
                }
            }
            upgradesAvailableIndicator.SetActive(false);
            // Debug.Log("No upgrades available");
        }
        private List<MetaprogressionModel> GetUnlockedNodesFromIds(List<int> unlockedNodeIds)
        {
            List<MetaprogressionModel> unlockedNodes = new List<MetaprogressionModel>();
            MetaprogressionTreeModel treeModel = _metaprogressionManager.MetaprogressionTreeModel;

            foreach(ChildParentPair pair in treeModel.MetaProgressionTree)
            {
                if(unlockedNodeIds.Contains(pair.Child.NodeId))
                {
                    unlockedNodes.Add(pair.Child);
                }
            }

            return unlockedNodes;
        }
        private void SetUpTavernThemeDropdown()
        {
            // Re-evaluated on every open so a Godking run finished this session is reflected here
            // without needing a game restart, and so already-affected saves repair themselves.
            SaveDataHandler.RefreshTavernThemeUnlocks();

            _tavernThemeDropdown.options.Clear();

            foreach (TavernThemeData theme in _tavernThemes)
            {
                // if(theme.Race == Race.Special) 
                //     continue;

                bool unlocked = SaveDataHandler.IsTavernThemeUnlocked(theme.Race);
                string localizedRace = theme.Race == Race.Special
                    ? LocalizationManager.Instance.GetText("None")
                    : LocalizationManager.Instance.GetText(theme.Race.ToString());
                string label = unlocked ? localizedRace : $"<color=red>{localizedRace}</color>";
                _tavernThemeDropdown.options.Add(new TMP_Dropdown.OptionData(label));
            }

            int startIndex = _tavernThemes.FindIndex(t => t.Race == Race.Special);
            if (startIndex < 0) startIndex = 0;
            if (SaveDataHandler.TryGetActiveTavernTheme(out Race savedRace))
            {
                int themeIndex = _tavernThemes.FindIndex(t => t.Race == savedRace);
                if (themeIndex >= 0)
                    startIndex = themeIndex;
            }

            _lastValidThemeIndex = startIndex;
            _tavernThemeDropdown.SetValueWithoutNotify(startIndex);
            _tavernThemeDropdown.RefreshShownValue();
        }
        private void OnTavernThemeChanged(int _index)
        {
            TavernThemeData selected = _tavernThemes[_index];
            if (!SaveDataHandler.IsTavernThemeUnlocked(selected.Race))
            {
                NotificationManager.Instance.DisplayNotification(
                    LocalizationManager.Instance.GetText("TavernThemeTooltipDescription")
                );
                _tavernThemeDropdown.SetValueWithoutNotify(_lastValidThemeIndex);
                _tavernThemeDropdown.RefreshShownValue();
                return;
            }

            _lastValidThemeIndex = _index;
            if (selected.Race == Race.Special)
                SaveDataHandler.ClearActiveTavernTheme();
            else
                SaveDataHandler.SetActiveTavernTheme(selected.Race);

            TavernThemeManager.Instance.ApplyTheme(selected);
        }
    }
}
