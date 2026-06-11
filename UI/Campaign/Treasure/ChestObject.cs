using UnityEngine;
using Memori.Audio;

namespace TJ.Treasure
{
    public class ChestObject : MonoBehaviour
    {
        [SerializeField] private GameObject _chestRays;
        [SerializeField] private TreasurePanel treasurePanel;
        public void ShowChestRays()
        {
            _chestRays.SetActive(true);
            treasurePanel.ChestOpen();
        }
        public void StopChestRays()
        {
            _chestRays.SetActive(false);
        }
        public void PlayPing()
        {
            IAudioRequester.Instance.PlaySFX(SFXData.ChestPing);
        }
        public void PlayRattle()
        {
            IAudioRequester.Instance.PlaySFX(SFXData.ChestRattle);
        }
    }
}