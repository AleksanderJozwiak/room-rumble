using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Stat Boost Values")]
    public float healthBoost;
    public float staminaBoost;
    public int ammoBoost;
    public float damageBoost;
    public float armorBoost;
    public float fireRateBoost;
    public float criticalChanceBoost;
    public float reloadSpeedBoost;
    public float movementSpeedBoost;

    [Header("Pickup Effects")]
    public ParticleSystem pickupEffect;

    [Header("SoundFX")]
    [SerializeField] private AudioClip powerUpSoundClip;
    [SerializeField] private float powerUpSoundVolume = 1f;

    private PlayerStats playerStats;
    private PlayerUIManager playerUi;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStats = other.GetComponent<PlayerMovement>().playerStats;
            playerUi = other.GetComponentInChildren<PlayerUIManager>();
            if (playerStats != null)
            {
                ApplyStatBoosts(playerUi);
            }

            if (pickupEffect != null)
            {
                ParticleSystem effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            gameObject.SetActive(false);
            SoundFXManager.instance.PlaySoundFXClip(powerUpSoundClip, transform, powerUpSoundVolume);
        }
    }

    private void ApplyStatBoosts(PlayerUIManager playerUi)
    {
        playerStats.currentHealth += healthBoost;
        playerStats.currentStamina += staminaBoost;
        playerStats.maxAmmo += ammoBoost;
        playerStats.damage += damageBoost;
        playerStats.armor += armorBoost;
        playerStats.fireRate += fireRateBoost;
        playerStats.criticalChance += criticalChanceBoost;
        playerStats.reloadSpeed += reloadSpeedBoost;
        playerStats.movementSpeed += movementSpeedBoost;

        if (playerUi != null)
        {
            playerUi.UpdateStatsText();
            playerUi.UpdateArmorBar();
            playerUi.UpdateHealthBar();
        }
    }
}