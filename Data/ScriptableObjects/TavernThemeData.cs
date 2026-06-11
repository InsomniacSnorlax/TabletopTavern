using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "TavernThemeData", menuName = "GameData/TavernThemeData", order = 2)]
public class TavernThemeData : ScriptableObject
{
    public string ThemeName;
    public Race Race;
    public AssetReferenceGameObject ThemeObjects;
    public RaceData RaceData;
}
