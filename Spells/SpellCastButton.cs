using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading;

namespace TJ.Spells
{
// [RequireComponent(typeof(ToolTipTrigger))]
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
    private float currentCooldown;
    public bool OnCooldown => currentCooldown > 0;
    private SpellManager spellManager;
    private SpellData spell;
    private bool selected;
    private float modifiedCooldownRate;
    // ToolTipTrigger toolTipTrigger;


    Color cachedCooldownColor;
    Color cachedSpellIconColor;

    private void Awake()
    {
        // toolTipTrigger = GetComponent<ToolTipTrigger>();
        // toolTipTrigger.SetUpToolTip(_description:"Locked", _delay:0.5f);
        cachedCooldownColor = cooldownImage.color;
        cachedSpellIconColor = spellIcon.color;
    }

    public void LoadSpellUI(SpellData _spell, SpellManager _spellManager, int _spellIndex)
    {
        spell = _spell;
        spellManager = _spellManager;

        spellIcon.sprite = SpriteData.GetSprite($"{spell.SpellIcon}");


        cooldownImage.fillAmount = 0;
        selectSpellButton.onClick.RemoveAllListeners();
        selectSpellButton.onClick.AddListener(AttemptSelectSpell);
        onCooldownGem.gameObject.SetActive(false);
        availableGem.gameObject.SetActive(true);
        selectedGem.gameObject.SetActive(false);
        // toolTipTrigger.SetUpToolTip(spell.spellName, spell.spellDescription + "\nCooldown: " +spell.spellCooldown.ToString()+"s", $"Hotkey [{_spellIndex}]", 0.25f);
    }
    public void ApplyCooldown()
    {
        // modifiedCooldownRate = spell.spellCooldown * GameManager.Instance.GlobalModifierManager.GetGlobalModifier(ModifierType.SpellsCooldown, TowerType.Global);
        modifiedCooldownRate = spell.SpellCooldown;
        currentCooldown = modifiedCooldownRate;
        StartCoroutine(StartCooldown());
    }
    public void ReduceCooldown(float _time)
    {
        currentCooldown -= _time;
    }
    private IEnumerator StartCooldown()
    {
        selectedGem.gameObject.SetActive(false);
        availableGem.gameObject.SetActive(false);
        onCooldownGem.gameObject.SetActive(true);
        while (currentCooldown > 0) {
            cooldownImage.fillAmount = currentCooldown / modifiedCooldownRate;
            currentCooldown -= Time.deltaTime;
            yield return null;
        }
        cooldownImage.fillAmount = 100;
        FlashCooldownImage(Color.white);
        yield return new WaitForSecondsRealtime(0.1f);
        spellIcon.color = Color.white;
        cooldownImage.fillAmount = 0;
        availableGem.gameObject.SetActive(true);
        onCooldownGem.gameObject.SetActive(false);
        
        // GameManager.Instance.IAudioRequester.PlaySFX("spell-ready");
    }
    public async void FlashCooldownImage(Color _color)
    {
        _color.a = 0.75f;
        cooldownImage.color = _color;
        await Task.Delay(100);
        cooldownImage.color = cachedCooldownColor;
    }
    public void AttemptSelectSpell() 
    {
        spellManager.SelectSpell(spell.SpellName);
    }
    public void SelectSpell()
    {
        // GameManager.Instance.IAudioRequester.PlaySFX("spell-selected");
        selected = true;
        outlineImage.color = selectedOutlineColor;
        selectedGem.gameObject.SetActive(true);
        availableGem.gameObject.SetActive(false);
    }
    public void DeselectSpell()
    {
        outlineImage.color = outlineDefaultColor;
        selected = false;
        selectedGem.gameObject.SetActive(false);
        availableGem.gameObject.SetActive(true);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // GameManager.Instance.IAudioRequester.PlaySFX("tiny-click");

        if(!OnCooldown && !selected)
            outlineImage.color = mouseOverOutlineColor;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if(!OnCooldown && !selected)
            outlineImage.color = outlineDefaultColor;
    }
}
}
