using UnityEngine;

[CreateAssetMenu(fileName = "SquadData", menuName = "GameData/SquadData", order = 1)]
public class SquadData : ScriptableObject
{
    public SquadStats stats;
    public SquadAssets assets;
}