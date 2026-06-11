using UnityEngine;
using Memori.Utilities;
using System.Threading.Tasks;

namespace TJ.Map
{
    public class AutoSavingIndicator : MonoBehaviour
    {
        [SerializeField] private MemoriCanvasGroup canvasGroup;
        private void Awake()
        {
            canvasGroup.CGDisable();
        }
        public async void OnGameSaved()
        {
            await canvasGroup.FadeIn(2f);
            canvasGroup.FadeOutAsync(1f);
        }
    }
}