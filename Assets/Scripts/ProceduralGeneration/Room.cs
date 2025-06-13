using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RoomType
{
    S,
    L,
    I,
    Portal,
    Final
}

public class Room : MonoBehaviour
{
    public Door[] Doors;
    public List<Transform> spawnPoints = new();
    public List<GameObject> powerUps = new();
    public Transform powerUpSpawnPoint;
    public List<GameObject> objectsToSpawn = new();

    // Wave system enemies
    public GameObject fastEnemyPrefab;
    public GameObject flyingEnemyPrefab;
    public GameObject heavyEnemyPrefab;

    public List<GameObject> gatesToControl = new();
    private bool hasBeenEntered = false;
    private bool roomCleared = false;
    private MinimapManager minimapManager;
    private List<GameObject> activeEnemies = new();

    // Wave system variables
    private bool waveSystemActive = false;
    private int currentWave = 0;
    private float waveTimer = 0f;
    private float[] waveDurations = { 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f };

    public Vector2 GridPosition { get; private set; }

    public RoomType roomType;

    [Header("Victory Screen")]
    public GameObject victoryScreen; // Assign this in the inspector
    private int totalWaves = 10; // Total waves for final room
    private bool victoryAchieved = false;

    [Header("SoundFX")]
    [SerializeField] private AudioClip portalSoundClip;
    [SerializeField] private float portalSoundVolume = 1f;

    private void Start()
    {
        minimapManager = FindObjectOfType<MinimapManager>();
    }

    private void Update()
    {
        if (waveSystemActive && !roomCleared && !victoryAchieved)
        {
            waveTimer += Time.deltaTime;

            // Handle wave progression for all 10 waves
            for (int i = 0; i < totalWaves; i++)
            {
                if (currentWave == i && waveTimer >= waveDurations[i])
                {
                    if (i < totalWaves - 1)
                    {
                        StartNextWave();
                    }
                    else
                    {
                        StartFinalWave();
                    }
                    break;
                }
            }
        }
    }

    public void SetGridPosition(Vector2 pos)
    {
        GridPosition = pos;
    }

    public Vector2 GetGridPosition()
    {
        return GridPosition;
    }

    public void OpenAllPossibleDoors()
    {
        foreach (Door door in Doors)
        {
            door.OpenDoor();
        }
    }

    public Door GetDoorAtPosition(Transform doorTransform) =>
        Doors.FirstOrDefault(e => e.transform.position.x == doorTransform.transform.position.x && e.transform.position.z == doorTransform.transform.position.z);

    public void GenerateObjects()
    {
        if (objectsToSpawn.Count == 0 || spawnPoints.Count == 0)
        {
            Debug.LogWarning("No objects to spawn or spawn points available.");
            return;
        }

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (Random.value <= 0.75f)
            {
                GameObject objToSpawn = objectsToSpawn[Random.Range(0, objectsToSpawn.Count)];
                Vector3 spawnPosition = new Vector3(spawnPoint.position.x, objToSpawn.transform.position.y, spawnPoint.position.z);
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                GameObject spawned = Instantiate(objToSpawn, spawnPosition, randomRotation);

                if (spawned.CompareTag("Enemy"))
                {
                    activeEnemies.Add(spawned);
                    Health enemyHealthScript = spawned.GetComponent<Health>();
                    if (enemyHealthScript != null)
                    {
                        enemyHealthScript.SetRoom(this);
                    }
                }
            }
        }
    }

    public void GeneratePowerUp()
    {
        if (powerUps.Count == 0 || powerUpSpawnPoint == null)
        {
            return;
        }

        if (Random.value <= 0.33f)
        {
            GameObject objToSpawn = powerUps[Random.Range(0, powerUps.Count)];
            Vector3 spawnPosition = new Vector3(powerUpSpawnPoint.position.x, objToSpawn.transform.position.y, powerUpSpawnPoint.position.z);
            Instantiate(objToSpawn, spawnPosition, objToSpawn.transform.rotation);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnteredRoom();
        }
    }

    void OnPlayerEnteredRoom()
    {
        if (hasBeenEntered) return;
        hasBeenEntered = true;

        if (minimapManager != null)
            minimapManager.RoomEntered(GridPosition);

        // Handle final room differently
        if (roomType == RoomType.Final)
        {
            StartFinalRoomWaveSystem();
        }
        else if (activeEnemies.Count > 0) // Normal room behavior
        {
            foreach (GameObject gate in gatesToControl)
            {
                LowerGates();
            }
        }

        //SoundFXManager.instance.PlaySoundFXClip(portalSoundClip, transform, portalSoundVolume);
    }

    private void StartFinalRoomWaveSystem()
    {
        waveSystemActive = true;
        currentWave = 0;
        waveTimer = 0f;
        victoryAchieved = false;

        // Always lower gates in final room
        foreach (GameObject gate in gatesToControl)
        {
            LowerGates();
        }

        // Spawn first wave immediately
        SpawnWave(fastEnemyPrefab);
    }

    private void SpawnWave(GameObject enemyPrefab)
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            Vector3 spawnPosition = new Vector3(spawnPoint.position.x, enemyPrefab.transform.position.y, spawnPoint.position.z);
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            GameObject spawned = Instantiate(enemyPrefab, spawnPosition, randomRotation);

            activeEnemies.Add(spawned);
            Health enemyHealthScript = spawned.GetComponent<Health>();
            if (enemyHealthScript != null)
            {
                enemyHealthScript.SetRoom(this);
            }
        }
    }

    private void SpawnMixedWave()
    {
        StartCoroutine(SpawnMixedWaveWithDelay());
    }

    private IEnumerator SpawnMixedWaveWithDelay()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            // Randomly select enemy type
            int enemyType = Random.Range(0, 3);
            GameObject enemyPrefab = enemyType switch
            {
                0 => fastEnemyPrefab,
                1 => flyingEnemyPrefab,
                2 => heavyEnemyPrefab,
                _ => fastEnemyPrefab
            };

            Vector3 spawnPosition = new Vector3(spawnPoint.position.x, enemyPrefab.transform.position.y, spawnPoint.position.z);
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            GameObject spawned = Instantiate(enemyPrefab, spawnPosition, randomRotation);

            activeEnemies.Add(spawned);
            Health enemyHealthScript = spawned.GetComponent<Health>();
            if (enemyHealthScript != null)
            {
                enemyHealthScript.SetRoom(this);
            }

            yield return new WaitForSeconds(3f); // Delay between spawns
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        waveTimer = 0f;

        switch (currentWave % 3) // Cycle through 3 enemy types
        {
            case 0:
                SpawnWave(fastEnemyPrefab);
                break;
            case 1:
                SpawnWave(flyingEnemyPrefab);
                break;
            case 2:
                SpawnWave(heavyEnemyPrefab);
                break;
        }
    }

    private void StartFinalWave()
    {
        SpawnMixedWave();
        waveSystemActive = false; // No more waves after this
    }

    public void NotifyEnemyDefeated(GameObject enemy)
    {
        activeEnemies.Remove(enemy);

        if (!roomCleared && activeEnemies.Count == 0)
        {
            // In final room, check if all waves are complete
            if (roomType == RoomType.Final)
            {
                if (currentWave >= totalWaves - 1 && !waveSystemActive)
                {
                    roomCleared = true;
                    victoryAchieved = true;
                    ShowVictoryScreen();
                    OpenGates();
                }
            }
            else // Normal room behavior
            {
                roomCleared = true;
                OpenGates();
            }
        }
    }

    private void ShowVictoryScreen()
    {
        if (victoryScreen != null)
        {
            victoryScreen.SetActive(true);
            Time.timeScale = 0f; // Pause the game
            Cursor.lockState = CursorLockMode.None; // Unlock cursor
            Cursor.visible = true; // Show cursor
        }
        else
        {
            Debug.LogWarning("Victory screen not assigned!");
        }
    }

    public void LowerGates()
    {
        foreach (GameObject gate in gatesToControl)
        {
            StartCoroutine(MoveGate(gate, 0f));
        }
    }

    public void OpenGates()
    {
        foreach (GameObject gate in gatesToControl)
        {
            StartCoroutine(MoveGate(gate, 4.5f));
        }
    }

    private IEnumerator MoveGate(GameObject gate, float targetY)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 startPos = gate.transform.position;
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        while (elapsed < duration)
        {
            gate.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        gate.transform.position = endPos;
    }
}