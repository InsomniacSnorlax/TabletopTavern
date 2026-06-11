using UnityEngine;
using Memori.Audio;
using Memori.Core;

[RequireComponent(typeof(AudioSource))]
public class FireplaceSFX : MonoBehaviour
{
    AudioSource fireplaceAudioSource;
    private void Start()
    {
        fireplaceAudioSource = GetComponent<AudioSource>();
        IAudioRequester.Instance.ambienceVolume.OnValueChanged += FireplaceSFXLevelChange;
        FireplaceSFXLevelChange(IAudioRequester.Instance.ambienceVolume.GetValue());
    }
    private void OnDestroy() 
    {
        if (IAudioRequester.Instance != null)
            IAudioRequester.Instance.ambienceVolume.OnValueChanged -= FireplaceSFXLevelChange;
    }
    private void FireplaceSFXLevelChange(float volume) 
    {
        fireplaceAudioSource.volume = 0.2f * volume;
    }
}
