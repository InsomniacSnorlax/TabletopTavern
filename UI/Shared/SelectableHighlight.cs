using UnityEngine;
using UnityEngine.EventSystems;
using Memori.UI;
using Memori.Audio;
using UnityEngine.UI;

public class SelectableHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image selectionHighlightImage;
    [SerializeField] private Transform itemToScale;
    public void OnSelect(BaseEventData eventData)
    {
        IAudioRequester.Instance.PlaySFX(SFXData.ButtonHover);
        MemoriUI.BloomItemScale(itemToScale, 1.025f, 0.1f);
        if (selectionHighlightImage != null) selectionHighlightImage.enabled = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        MemoriUI.BloomItemScale(itemToScale, 1f, 0.1f);
        if (selectionHighlightImage != null) selectionHighlightImage.enabled = false;
    }
}
