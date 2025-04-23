using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerStats playerStats;

    private int currentLevel = 0;

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

    public void SetLevel(int level)
    {
        if (currentLevel == 0)
        {
            ResetStats();
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
}
