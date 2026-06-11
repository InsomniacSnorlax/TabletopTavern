using Memori.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TJ
{
public class GearModifierUI : MonoBehaviour
{
    [SerializeField] private Image modifierImage;
    [SerializeField] private TMP_Text modifierAmount;
    public void LoadGearModifierUI(Gear _gearAction)
    {
        // modifierImage.sprite = SpriteData.GetSprite(_gearAction.GearUnitModifier.ToString());
        // string modifierText = _gearAction.IsNegative ? "-" : "+";
        string modifierText = _gearAction.GearModifierValue.ToString() + " ";
        // modifierText += MemoriUI.AddSpacesToSentence(_gearAction.GearUnitModifier.ToString());
        modifierAmount.text = modifierText;
    }
}
}