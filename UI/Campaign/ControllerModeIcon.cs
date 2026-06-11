using UnityEngine;
using Memori.Input;
using UnityEngine.UI;

public class ControllerModeIcon : MonoBehaviour
{
    [SerializeField] private GameObject controllerIcon;
    void Start()
    {
        InputHandler.Instance.onControlModeToggle += ControllerModeIconToggled;
    }
    void OnEnable()
    {
        controllerIcon.SetActive(InputHandler.Instance.ControllerMode);
    }

    private void ControllerModeIconToggled()
    {
        controllerIcon.SetActive(InputHandler.Instance.ControllerMode);
    }
    private void OnDestroy()
    {
        if (InputHandler.HasInstance) {
            InputHandler.Instance.onControlModeToggle -= ControllerModeIconToggled;
        }
    }
}
