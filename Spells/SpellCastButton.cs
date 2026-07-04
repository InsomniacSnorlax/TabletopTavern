using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TJ.Spells
{
public class SpellCastButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Icon")]
    [SerializeField] private Image spellIcon;
    [SerializeField] private Button selectSpellButton;

    [Header("Cooldown")]
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Image availableGem, selectedGem, onCooldownGem;

    [Header("Outline")]
    [SerializeField] private Image outlineImage;
    [SerializeField] private Color outlineDefaultColor;
    [SerializeField] private Color mouseOverOutlineColor, selectedOutlineColor;

    private Color cachedCooldownColor;
    private bool cachedSelected;
    private bool cachedOnCooldown;

    private void Awake()
    {
        cachedCooldownColor = cooldownImage.color;
    }

    public void LoadSpellUI(SpellData spellData, Action onSelectRequested)
    {
        spellIcon.sprite = spellData.SpellSprite;

        selectSpellButton.onClick.RemoveAllListeners();
        selectSpellButton.onClick.AddListener(() => onSelectRequested?.Invoke());

        SetSelected(false);
        RenderCooldown(0f, false);
    }

    public void SetSelected(bool selected)
    {
        cachedSelected = selected;
        outlineImage.color = selected ? selectedOutlineColor : outlineDefaultColor;
        RefreshGems();
    }

    public void RenderCooldown(float remainingFraction01, bool onCooldown)
    {
        cachedOnCooldown = onCooldown;
        cooldownImage.fillAmount = onCooldown ? Mathf.Clamp01(remainingFraction01) : 0f;
        RefreshGems();
    }

    private void RefreshGems()
    {
        onCooldownGem.gameObject.SetActive(cachedOnCooldown);
        selectedGem.gameObject.SetActive(cachedSelected && !cachedOnCooldown);
        availableGem.gameObject.SetActive(!cachedSelected && !cachedOnCooldown);
    }

    public async void FlashCooldownImage(Color _color)
    {
        _color.a = 0.75f;
        cooldownImage.color = _color;
        await Task.Delay(100);
        cooldownImage.color = cachedCooldownColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!cachedOnCooldown && !cachedSelected)
            outlineImage.color = mouseOverOutlineColor;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if(!cachedOnCooldown && !cachedSelected)
            outlineImage.color = outlineDefaultColor;
    }
}
}
