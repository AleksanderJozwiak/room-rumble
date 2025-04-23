using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider healthBar;
    public Slider staminaBar;
    public Slider armorBar;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI statsText;

    [Header("Player Stats")]
    public PlayerStats playerStats;

    public GameObject playerDeathUI;

    private void Start()
    {
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdateArmorBar();
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
    
    public void UpdateArmorBar()
    {
        armorBar.value = playerStats.armor / 100;
    }

    public void UpdateAmmoText(int currentAmmo)
    {
        ammoText.text = "Ammo: " + currentAmmo + "/" + playerStats.maxAmmo;
    }

    public void UpdateStatsText()
    {
        statsText.text = $"Move speed: {playerStats.movementSpeed}\n" +
                         $"Fire Rate: {playerStats.fireRate}s\n" +
                         $"Crit Chance: {playerStats.criticalChance}%\n" +
                         $"Reload Speed: {playerStats.reloadSpeed}s";
    }

    public void PlayerDies()
    {
        playerDeathUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
    }
}
