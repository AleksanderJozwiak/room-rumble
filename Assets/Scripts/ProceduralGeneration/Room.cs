using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Door[] Doors;
    public List<Transform> spawnPoints = new();
    public List<GameObject> powerUps = new();
    public Transform powerUpSpawnPoint;
    public List<GameObject> objectsToSpawn = new();

    void Start()
    {

    }

    void Update()
    {

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
            if (Random.value <= 0.75f) // 75% chance to spawn
            {
                GameObject objToSpawn = objectsToSpawn[Random.Range(0, objectsToSpawn.Count)];
                Vector3 spawnPosition = new Vector3(spawnPoint.position.x, objToSpawn.transform.position.y, spawnPoint.position.z);

                // Generate a random Y-axis rotation
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                Instantiate(objToSpawn, spawnPosition, randomRotation);
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

}
