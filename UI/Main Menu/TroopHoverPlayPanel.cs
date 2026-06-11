using Memori.SaveData;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TJ.MainMenu
{
public class TroopHoverPlayPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private int index;
    PlayPanel playPanel;
    public void SetUp(int _index, PlayPanel _playPanel)
    {
        index = _index;
        playPanel = _playPanel;
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if(index == -1)
        {
            SquadToLoad squadToAdd = GetComponent<SquadDisplayCardMenu>().GetSquadToLoad();
            playPanel.StartingArmySection.PointerOverTroop(squadToAdd);
        }
        else
        {
            playPanel.StartingArmySection.PointerOverTroop(index);
        }
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        playPanel.StartingArmySection.PointerOffTroop();
    }
}
}