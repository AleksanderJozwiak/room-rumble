using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RoomType
{
    S,
    L,
    I,
    Portal
}

public class Room : MonoBehaviour
{
    public Door[] Doors;
    public List<Transform> spawnPoints = new();
    public List<GameObject> powerUps = new();
    public Transform powerUpSpawnPoint;
    public List<GameObject> objectsToSpawn = new();

    public List<GameObject> gatesToControl = new();
    private bool hasBeenEntered = false;
    private bool roomCleared = false;
    private MinimapManager minimapManager;
    private List<GameObject> activeEnemies = new();

    public Vector2 GridPosition { get; private set; }

    public RoomType roomType;

    [Header("SoundFX")]
    [SerializeField] private AudioClip portalSoundClip;
    [SerializeField] private float portalSoundVolume = 1f;

    private void Start()
    {
        minimapManager = FindObjectOfType<MinimapManager>();
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

        if (activeEnemies.Count > 0)
        {
            foreach (GameObject gate in gatesToControl)
            {
                LowerGates();
            }
        }

        if (minimapManager != null)
            minimapManager.RoomEntered(GridPosition);

        //SoundFXManager.instance.PlaySoundFXClip(portalSoundClip, transform, portalSoundVolume);
    }

    public void NotifyEnemyDefeated(GameObject enemy)
    {
        activeEnemies.Remove(enemy);

        if (!roomCleared && activeEnemies.Count == 0)
        {
            roomCleared = true;
            //minimapManager.RoomCleared(GridPosition, true);
            OpenGates();
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
