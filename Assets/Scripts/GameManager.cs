using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerStats playerStats;

    private int currentLevel = 0;

    // Tryb endless 
    public float gameTime = 0f; 
    public int score = 0;

    public GameMode currentGameMode;

    public enum GameMode
    {
        Endless,
        StoryMode
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        gameTime += Time.deltaTime;
    }

    public void AddScore(int points)
    {
        score += points;
    }

    public void SetLevel(int level)
    {
        if (currentLevel == 0)
        {
            ResetStats();
            if (currentGameMode == GameMode.Endless)
            {
                gameTime = 0f;
                score = 0;
            }
        }
        currentLevel = level;
    }
    public int GetLevel () => currentLevel;

    public void ResetStats()
    {
        playerStats.maxAmmo = 8;
        playerStats.damage = 20;
        playerStats.movementSpeed = 5;
        playerStats.fireRate = 1;
        playerStats.criticalChance = 10;
        playerStats.reloadSpeed = 3;
        playerStats.armor = 0;
        playerStats.currentHealth = playerStats.maxHealth;
        playerStats.currentStamina = playerStats.maxStamina;
    }

    public void CheckAndSaveHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);

        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetFloat("BestTime", gameTime);
            PlayerPrefs.Save();
        }
        else if (score == highScore && gameTime < bestTime)
        {
            PlayerPrefs.SetFloat("BestTime", gameTime);
            PlayerPrefs.Save();
        }
    }

}
