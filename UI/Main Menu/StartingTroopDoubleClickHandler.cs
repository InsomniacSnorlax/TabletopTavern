

using Memori.Core;
using Memori.SaveData;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TJ.MainMenu
{
    public class StartingTroopDoubleClickHandler : DoubleClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private StartingArmyManager startingArmyManager;
        private int troopIndex;

        public void SetUp(int index, StartingArmyManager _startingArmyManager)
        {
            startingArmyManager = _startingArmyManager;
            troopIndex = index;
        }

        protected override void OnDoubleClick()
        {
            if(troopIndex == -1)
            {
                SquadToLoad squadToAdd = GetComponent<SquadDisplayCardMenu>().GetSquadToLoad();
                startingArmyManager.AddTroop(squadToAdd);
            }
            else
            {
                startingArmyManager.RemoveTroop(troopIndex);
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            
        }
    }
}