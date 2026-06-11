using Memori.Utilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TJ.MainMenu
{
    [RequireComponent(typeof(MemoriCanvasGroup))]
    public class MainMenuPanel : MonoBehaviour
    {
        private MemoriCanvasGroup panelCanvasGroup;
        [SerializeField] protected Button returnToMainMenuButton;
        protected MainMenu mainMenu;
        private void Awake()
        {
            panelCanvasGroup = GetComponent<MemoriCanvasGroup>();
        }
        public virtual void SetUp(MainMenu _mainMenu)
        {
            mainMenu = _mainMenu;
            if(returnToMainMenuButton == null) return;

            returnToMainMenuButton.onClick.RemoveAllListeners();
            returnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        public virtual void OpenPanel()
        {
            //get first button on the new panel
            if(returnToMainMenuButton != null) EventSystem.current.SetSelectedGameObject(returnToMainMenuButton.gameObject);
            panelCanvasGroup.CGEnable();
        }
        public virtual void ClosePanel()
        {
            EventSystem.current.SetSelectedGameObject(null);
            panelCanvasGroup.CGDisable();
        }
        public void ReturnToMainMenu()
        {
            mainMenu.ReturnToMainMenu();
        }
    }
}
