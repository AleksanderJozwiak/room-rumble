using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    private Room room;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void SetRoom(Room roomReference)
    {
        room = roomReference;
    }


    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore((int)maxHealth);
        }

        if (room != null)
        {
            room.NotifyEnemyDefeated(gameObject);
        }

        Destroy(gameObject);
    }

}

