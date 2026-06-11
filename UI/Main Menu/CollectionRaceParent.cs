using UnityEngine;
using Memori.Utilities;
using System.Collections.Generic;
using UnityEngine.UI;

namespace TJ.MainMenu
{
    public class CollectionRaceParent : MonoBehaviour
    {
        public Transform RaceUnitsParent;
        public MemoriCanvasGroup RaceCanvasGroup;
        public HorizontalLayoutGroup grandparentRectTransform;
        public List<SquadDisplayCardCollection> RaceCardCollection = new();

        [Header("Sections")]
        public MemoriCanvasGroup unitsCanvasGroup;

        public void Clear()
        {
            RaceCardCollection.ForEach(card => Destroy(card.gameObject));
            RaceCardCollection.Clear();
        }
        public void ResetLayout()
        {
            DisplayUnitsOfRace();
            grandparentRectTransform.enabled = false;
            grandparentRectTransform.enabled = true;
        }
        public void DisplayUnitsOfRace()
        {
            unitsCanvasGroup.CGEnable();
        }
        public void HideUnitsOfRace()
        {
            unitsCanvasGroup.CGDisable();
        }
    }
}