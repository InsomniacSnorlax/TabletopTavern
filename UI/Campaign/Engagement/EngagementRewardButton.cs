using UnityEngine;
using UnityEngine.EventSystems;

namespace TJ.Engagement
{
public class EngagementRewardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool conscriptionButton = false;
    EngagementPanel engagementPanel;
    bool isHover = false;
    public void SetUp(EngagementPanel _engagementPanel)
    {
        engagementPanel = _engagementPanel;
        isHover = false;
    }
    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && isHover) {
            isHover = false;
                if (conscriptionButton)
                {
                    engagementPanel.ConscriptSurvivorsButtonClicked();
                }
                else
                {
                    engagementPanel.RaiseDeadButtonClicked();
                }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }
}
}