using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySFX : MonoBehaviour
{
    [SerializeField] private AudioClip ExampleSoundClip;
    [SerializeField] private float ExmapleSoundVolume = 1f;

    private void Start()
    {
        SoundFXManager.instance.PlaySoundFXClip3D(ExampleSoundClip, transform, ExmapleSoundVolume, 0f, 100f);
    }
}
