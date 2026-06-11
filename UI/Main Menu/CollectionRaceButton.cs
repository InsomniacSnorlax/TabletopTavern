using UnityEngine;
using TMPro;
using Memori.UI;
using System;
using Memori.Localization;

namespace TJ.MainMenu
{
    public class CollectionRaceButton : MonoBehaviour
    {
        public TMP_Text raceNameText;
        public TMP_Text UnitCountText;
        public MemoriButtonV2 RaceButton;
        public GameObject UnacknowledgedIndicator;
        public void SetUp(Action action, Race race)
        {
            raceNameText.text = LocalizationManager.Instance.GetText(race.ToString());
            RaceButton.Button.onClick.RemoveAllListeners();
            RaceButton.Button.onClick.AddListener(() => action());
        }
    }
}