using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    public GameObject playerUI;

    [Header("UI Elements")]
    public Slider healthBar;
    public Slider staminaBar;
    public Slider armorBar;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI statsText;

    [Header("Player Stats")]
    public PlayerStats playerStats;

    public GameObject playerDeathUI;
    
    [Header("Endless info")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText; 
    public TextMeshProUGUI currentTimeText;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI bestTimeText;


    private void Start()
    {
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdateArmorBar();
        UpdateAmmoText(0);
        UpdateStatsText();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.currentGameMode == GameManager.GameMode.Endless)
        {
            float time = GameManager.Instance.gameTime;
            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time % 60F);

            timeText.text = $"{minutes:00}:{seconds:00}";
            scoreText.text = $"Score: {GameManager.Instance.score}";
        }
        else
        {
            timeText.gameObject.SetActive(false);
            scoreText.gameObject.SetActive(false);
        }
        
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

    public void UpdateHighScoreUI()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        int minutes = Mathf.FloorToInt(bestTime / 60F);
        int seconds = Mathf.FloorToInt(bestTime % 60F);

        highScoreText.text = $"High Score: {highScore}";
        bestTimeText.text = $"High Score Time: {minutes:00}:{seconds:00}";
    }

    public void PlayerDies()
    {
        if (GameManager.Instance.currentGameMode == GameManager.GameMode.Endless)
        {
            UpdateHighScoreUI();
            currentTimeText.text = timeText.text;
            currentScoreText.text = scoreText.text;
            GameManager.Instance.CheckAndSaveHighScore();
        }
        playerUI.SetActive(false);
        playerDeathUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
    }
}
