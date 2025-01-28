using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider healthBar;
    public Slider staminaBar;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI statsText; // New UI for displaying fire rate, crit chance, etc.

    [Header("Player Stats")]
    public PlayerStats playerStats;

    private void Start()
    {
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdateAmmoText(0);
        UpdateStatsText();
    }

    public void UpdateHealthBar()
    {
        healthBar.value = playerStats.currentHealth / playerStats.maxHealth;
    }

    public void UpdateStaminaBar()
    {
        staminaBar.value = playerStats.currentStamina / playerStats.maxStamina;
    }

    public void UpdateAmmoText(int currentAmmo)
    {
        ammoText.text = "Ammo: " + currentAmmo + "/" + playerStats.maxAmmo;
    }

    public void UpdateStatsText()
    {
        statsText.text = $"Fire Rate: {playerStats.fireRate}s\n" +
                         $"Crit Chance: {playerStats.criticalChance}%\n" +
                         $"Reload Speed: {playerStats.reloadSpeed}s";
    }
}
