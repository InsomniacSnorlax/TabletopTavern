

using Memori.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TJ.MainMenu
{
    public class StartingGearDoubleClickHandler : DoubleClickHandler//, IPointerEnterHandler, IPointerExitHandler
    {
        private StartingArmyManager startingArmyManager;

        public void SetStartingArmyManager(StartingArmyManager _startingArmyManager)
        {
            startingArmyManager = _startingArmyManager;
        }

        protected override void OnDoubleClick()
        {
            startingArmyManager.SelectGearCard(GearID.None);
        }
        // public void OnPointerEnter(PointerEventData eventData)
        // {
        //     if(startingArmyManager != null)
        //         startingArmyManager.gearCanvasGroup.FadeInAsync(0.2f);
        // }
        // public void OnPointerExit(PointerEventData eventData)
        // {
        //     if(startingArmyManager != null)
        //         startingArmyManager.gearCanvasGroup.FadeOutAsync(0.2f);
        // }
    }
}