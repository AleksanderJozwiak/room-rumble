using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField] private AudioClip spikesSoundClip;
    [SerializeField] private float spikesSoundVolume = 1f;
    
    public int damage = 10;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMovement>(out var playerMovement))
        {
            playerMovement.TakeDamage(damage);
            SoundFXManager.instance.PlaySoundFXClip(spikesSoundClip, transform, spikesSoundVolume);
        }
    }

}
