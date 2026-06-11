using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace TJ.Event
{
    public class EventAquireRewardButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private Button button;
        [SerializeField] private Image rewardImage;
        public Button Button => button;
        public event Action onClick;
        
        public void SetUp(Sprite _rewardSprite, string _rewardText, Action action)
        {
            onClick = action;
            rewardImage.sprite = _rewardSprite;
            rewardText.text = _rewardText;
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
