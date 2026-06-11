using UnityEngine;

[CreateAssetMenu(fileName = "VoiceSFX", menuName = "GameData/VoiceSFX", order = 1)]
public class VoiceSFX : ScriptableObject
{
    public AudioClip[] barkSFX;
    public AudioClip[] chargeSFX;
    public AudioClip[] retreatSFX;
    public AudioClip[] deathSFX;
    public AudioClip[] idleSFX;
}