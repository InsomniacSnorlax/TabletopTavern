using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TJ
{
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(MemoriCanvasGroup))]
public class GoldNotification : MonoBehaviour
{
    [SerializeField] TMP_Text amountText;

    RectTransform _rect;
    MemoriCanvasGroup _canvasGroup;

    public void Initialize(int delta, string localizedString)
    {
        if (amountText != null)
        {
            amountText.text = (delta >= 0 ? $"+{delta}" : $"{delta}") + $" {TabletopTavernConstants.GOLD_SPRITE_STRING} {localizedString}";
            amountText.color = delta >= 0
                ? (Color)ColorData.HexToRgba(ColorData.Primary)
                : (Color)ColorData.HexToRgba(ColorData.Error);
        }
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<MemoriCanvasGroup>();
        _canvasGroup.FadeInAsync();
        _ = Animate();
    }

    private async Task Animate()
    {
        await Task.Delay(3000);

        await _canvasGroup.FadeOut(0.2f);

        // float startHeight = _rect.sizeDelta.y;
        // float elapsed = 0f;
        // while (elapsed < 0.1f)
        // {
        //     elapsed += Time.deltaTime;
        //     _rect.sizeDelta = new Vector2(_rect.sizeDelta.x, startHeight * (1f - elapsed / 0.1f));
        //     await Task.Yield();
        // }

        Destroy(gameObject);
    }
}
}
