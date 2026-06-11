using TJ;
using UnityEngine;

[CreateAssetMenu(fileName = "RaceData", menuName = "GameData/RaceData", order = 1)]
public class RaceData : ScriptableObject
{
    public Race Race;
    public Color PrimaryColor;
    public Color SecondaryColor;
    public Color AccentColor;
    public RaceBasePrefab RaceBasePrefab;
    public MapRegion MapRegion;
}