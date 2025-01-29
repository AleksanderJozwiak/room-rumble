using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5f;
    public int damage = 10;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var playerHit = collision.gameObject.TryGetComponent<PlayerMovement>(out var playerMovement);
        if (playerHit)
        {
            playerMovement.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
