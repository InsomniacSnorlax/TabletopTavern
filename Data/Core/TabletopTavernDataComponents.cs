using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public struct SquadAssets
{
    public Race race;
    public Sprite unitIcon;
    public SquadIcon squadIcon;
    public AssetReferenceGameObject unitRecruitmentPrefab;
    public VoiceSFX voiceSFX;
    public MeleeAttackSFX meleeAttackSFX;
    public FireProjectileSFX fireProjectileSFX;
    public FormationDiscipline formationDiscipline;
    public AssetReferenceGameObject ArtilleryCrewPrefab;
}
