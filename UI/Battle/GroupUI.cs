using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TJ.Battle
{
public class GroupUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private TMP_Text groupNumberText;
    [SerializeField] private RectTransform groupBackgroundImage;
    [SerializeField] private Image backgroundImage, mainImage;
    [SerializeField] private TJ.BattleButton lockButton;
    [SerializeField] [Range(0f, 1f)] private float deselectedBrightness = 0.4f;
    GroupManager groupManager;
    int groupID;
    Color _color;
    public int GroupID => groupID;
    public void SetUpGroupUI(int _groupNumber, int _squadCount, GroupManager _groupManager, Color _color, bool isLocked)
    {
        groupManager = _groupManager;
        groupID = _groupNumber;
        this._color = _color;

        groupNumberText.text = _groupNumber.ToString();
        groupBackgroundImage.sizeDelta = new Vector2((_squadCount*60) + ((_squadCount-1) * 5), 25);
        SetSelected(false);

        if (lockButton != null)
        {
            lockButton.SetUp("Lock Formation", "Lock this group's formation shape",
                onClickBoolToggleAction: (_) => groupManager.ToggleLockGroup(groupID));
            lockButton.SetOnOrOff(isLocked);
        }
    }
    public void SetSelected(bool selected)
    {
        Color c = selected ? _color : _color * deselectedBrightness;
        c.a = _color.a;
        backgroundImage.color = c;
        mainImage.color = c;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("Pointer Enter group " + groupID);
        BattleManager.Instance.UnitSelectionManager.HoverSquad(0, true);
        groupManager.HoverGroup(groupID);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("Pointer Exit group " + groupID);
        groupManager.UnhoverGroup();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log("Pointer Down");
        groupManager.SelectGroup(groupID);
    }
}
}