using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    public AudioMixerGroup mixerGroup;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        //spawn in gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //assing the audioClip
        audioSource.clip = audioClip;

        //assign group of sound
        audioSource.outputAudioMixerGroup = mixerGroup;

        //assign volume
        audioSource.volume = volume;

        //play sound
        audioSource.Play();

        //get length of sound FX clip
        float clipLength = audioSource.clip.length;

        //destroy the clip after it is done playing
        Destroy(audioSource.gameObject, clipLength);

        Debug.Log($"[SFX] Odtwarzam: {audioClip.name} na {spawnTransform.name}");
    }

    public void PlaySoundFXClip3D(AudioClip audioClip, Transform spawnTransform, float volume, float minDistance, float maxDistance)
    {
        //spawn in gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //assing the audioClip
        audioSource.clip = audioClip;

        //assign group of sound
        audioSource.outputAudioMixerGroup = mixerGroup;

        //assign volume
        audioSource.volume = volume;

        //made audio source 3D
        audioSource.spatialBlend = 1f;

        //turn off Doppler effect
        audioSource.dopplerLevel = 0f;

        //change rolloff mode to custom
        audioSource.rolloffMode = AudioRolloffMode.Custom;

        //assign key frames to custom curve
        AnimationCurve customCurve = new AnimationCurve(
            new Keyframe(0f, volume),
            new Keyframe(1f, volume * 0.75f),
            new Keyframe(2f, volume * 0.5f),
            new Keyframe(3f, 0f)
        );

        //assign min disctacne from audio source
        audioSource.minDistance = minDistance;

        //assign max distance from audio source
        audioSource.maxDistance = maxDistance;

        //play sound
        audioSource.Play();

        //get length of sound FX clip
        float clipLength = audioSource.clip.length;

        //destroy the clip after it is done playing
        Destroy(audioSource.gameObject, clipLength);

        Debug.Log($"[SFX] Odtwarzam: {audioClip.name} na {spawnTransform.name}");
    }
}
