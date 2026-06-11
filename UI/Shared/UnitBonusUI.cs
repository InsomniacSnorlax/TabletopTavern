using TMPro;
using UnityEngine;


namespace TJ.Map
{
public class UnitBonusUI : MonoBehaviour
{
    [SerializeField] private TMP_Text bonusNameText, bonusDescriptionText;
    public void LoadUnitBonusUI(string _bonusName, string _bonusDescription)
    {
        bonusNameText.text = _bonusName;
        bonusDescriptionText.text = _bonusDescription;
    }
}
}