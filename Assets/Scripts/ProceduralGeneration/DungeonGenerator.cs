using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs;
    public GameObject portalRoomPrefabs;
    [SerializeField]
    private int roomCount = 3;
    private const int gridSize = 30;
    private const int maxPlacementAttempts = 1000;

    private readonly List<GameObject> generatedRooms = new();
    private readonly HashSet<Vector2> occupiedPositions = new(); 
    private readonly Queue<Vector2> frontierPositions = new();
    private readonly List<Transform> unusedDoors = new(); 
    private readonly HashSet<Vector2> queuedPositions = new();

    private Dictionary<Vector2, GameObject> roomMap = new();

    public NavMeshSurface navMeshSurface;

    void Start()
    {
        roomCount = 3 + (GameManager.Instance.GetLevel() * 2);
        GenerateDungeon();
        BakeNavMesh();
        InitMinimap();
    }

    void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
    void GenerateDungeon()
    {
        Vector2 initialPosition = Vector2.zero;
        if (PlaceFirstRoom(0, initialPosition))
        {
            occupiedPositions.Add(initialPosition);
            frontierPositions.Enqueue(initialPosition);
        }

        int roomsGenerated = 1;
        int totalPlacementAttempts = 0;

        while (roomsGenerated < roomCount && totalPlacementAttempts < maxPlacementAttempts)
        {
            if (frontierPositions.Count > 0)
            {
                Vector2 nextPosition = frontierPositions.Dequeue();

                if (!IsNegativeInfinity(nextPosition) && !occupiedPositions.Contains(nextPosition))
                {
                    if (PlaceRoomWithConnection(Random.Range(0, roomPrefabs.Length), nextPosition))
                    {
                        roomsGenerated++;
                    }
                    else
                    {
                        bool foundValidPosition = false;

                        while (unusedDoors.Count > 0 && !foundValidPosition)
                        {
                            Transform door = unusedDoors[0];
                            unusedDoors.RemoveAt(0);

                            Vector2 doorPosition = GetPositionNearDoor(door);
                            if (doorPosition != Vector2.negativeInfinity && !occupiedPositions.Contains(doorPosition))
                            {
                                frontierPositions.Enqueue(doorPosition);
                                foundValidPosition = true;
                            }
                        }
                    }
                }

                AddAdjacentPositionsToFrontier(nextPosition);
            }
            else
            {
                Debug.LogWarning("No more valid frontier positions found, stopping early.");
                break;
            }

            totalPlacementAttempts++;
        }
        bool portalRoomPlaced = false;

        while (!portalRoomPlaced && frontierPositions.Count > 0)
        {
            Vector2 potentialPosition = frontierPositions.Dequeue();

            portalRoomPlaced = PlacePortalRoom(potentialPosition);
            if (portalRoomPlaced) break;    
        }



        bool isFirstRoom = true;
        BakeNavMesh();
        foreach (GameObject generatedRoom in generatedRooms)
        {
            Room room = generatedRoom.GetComponent<Room>();
            Transform[] doorsTransforms = GetDoors(generatedRoom);

            foreach (Transform doorTransform in doorsTransforms) {
                Door door = room.GetDoorAtPosition(doorTransform);
                if (ConnectsWithAnyOtherDoor(doorTransform))
                {
                    door.OpenDoor();
                    door.SetFrontier(false);
                }
                else
                {
                    door.CloseDoor();
                    door.SetFrontier(true);
                }
            }
            if (isFirstRoom)
            {
                isFirstRoom = false;
                continue;
            }
            room.GenerateObjects();
            room.GeneratePowerUp();
        }
    }
    void InitMinimap()
    {
        MinimapManager minimap = FindObjectOfType<MinimapManager>();
        minimap.InitializeMinimap(roomMap);

        //minimap.RevealRoom(Vector2.zero, true);
    }

    bool IsNegativeInfinity(Vector2 position)
    {
        return float.IsNegativeInfinity(position.x) && float.IsNegativeInfinity(position.y);
    }

    bool PlaceFirstRoom(int prefabIndex, Vector2 gridPosition)
    {
        GameObject roomPrefab = roomPrefabs[prefabIndex];

        if (occupiedPositions.Contains(gridPosition))
        {
            Debug.LogWarning("Position already occupied, cannot place the first room here.");
            return false;
        }

        GameObject newRoom = Instantiate(roomPrefab);
        newRoom.transform.position = new Vector3(gridPosition.x * gridSize, 0, gridPosition.y * gridSize);
        Room room = newRoom.GetComponent<Room>();
        room.OpenAllPossibleDoors();
        generatedRooms.Add(newRoom);
        occupiedPositions.Add(gridPosition);

        roomMap[gridPosition] = newRoom;
        room.SetGridPosition(gridPosition);

        return true;
    }
    
    bool PlacePortalRoom(Vector2 gridPosition)
    {
        if (gridPosition == Vector2.negativeInfinity || occupiedPositions.Contains(gridPosition))
        {
            Debug.LogWarning("Position already occupied, cannot place the first room here.");
            return false;
        }

        for (int rotation = 0; rotation < 4; rotation++)
        {
            Vector3 position = new Vector3(gridPosition.x * gridSize, 0, gridPosition.y * gridSize);
            Quaternion rotationAngle = Quaternion.Euler(0, rotation * 90, 0);

            if (ConnectsWithExistingRoom(portalRoomPrefabs, position, rotationAngle))
            {
                GameObject newRoom = Instantiate(portalRoomPrefabs);
                newRoom.transform.SetPositionAndRotation(position, rotationAngle);
                generatedRooms.Add(newRoom);
                occupiedPositions.Add(gridPosition);

                Room room = newRoom.GetComponent<Room>();
                roomMap[gridPosition] = newRoom;
                room.SetGridPosition(gridPosition);

                foreach (Transform doorTransform in GetDoors(newRoom))
                {
                    if (!ConnectsWithAnyOtherDoor(doorTransform))
                    {
                        unusedDoors.Add(doorTransform);
                    }
                }

                return true;
            }
        }

        return false;
    }

    bool PlaceRoomWithConnection(int prefabIndex, Vector2 gridPosition)
    {
        if (gridPosition == Vector2.negativeInfinity || occupiedPositions.Contains(gridPosition))
        {
            Debug.LogWarning($"Invalid or occupied position detected: {gridPosition}. Skipping this placement.");
            return false;
        }

        GameObject roomPrefab = roomPrefabs[prefabIndex];

        for (int rotation = 0; rotation < 4; rotation++)
        {
            Vector3 position = new Vector3(gridPosition.x * gridSize, 0, gridPosition.y * gridSize);
            Quaternion rotationAngle = Quaternion.Euler(0, rotation * 90, 0);

            if (ConnectsWithExistingRoom(roomPrefab, position, rotationAngle))
            {
                GameObject newRoom = Instantiate(roomPrefab);
                newRoom.transform.SetPositionAndRotation(position, rotationAngle);
                generatedRooms.Add(newRoom);
                occupiedPositions.Add(gridPosition);

                Room room = newRoom.GetComponent<Room>();
                roomMap[gridPosition] = newRoom;
                room.SetGridPosition(gridPosition);

                foreach (Transform doorTransform in GetDoors(newRoom))
                {
                    if (!ConnectsWithAnyOtherDoor(doorTransform))
                    {
                        unusedDoors.Add(doorTransform);
                    }
                }

                return true;
            }
        }

        return false;
    }

    void AddAdjacentPositionsToFrontier(Vector2 position)
    {
        Vector2[] adjacentPositions = {
        position + Vector2.up,
        position + Vector2.down,
        position + Vector2.left,
        position + Vector2.right
    };

        foreach (Vector2 pos in adjacentPositions)
        {
            if (!occupiedPositions.Contains(pos) && queuedPositions.Add(pos))
            {
                frontierPositions.Enqueue(pos);
            }
        }
    }

    bool ConnectsWithExistingRoom(GameObject roomPrefab, Vector3 position, Quaternion rotation)
    {
        roomPrefab.transform.SetPositionAndRotation(position, rotation);
        Transform[] newRoomDoors = GetDoors(roomPrefab);
        bool connects = false;

        foreach (GameObject existingRoom in generatedRooms)
        {
            Transform[] existingRoomDoors = GetDoors(existingRoom);

            foreach (Transform newDoor in newRoomDoors)
            {
                foreach (Transform existingDoor in existingRoomDoors)
                {
                    if (Vector3.Distance(newDoor.position, existingDoor.position) < 0.1f)
                    {
                        connects = true;
                        break;
                    }
                }
                if (connects) break;
            }
            if (connects) break;
        }

        return connects;
    }

    bool ConnectsWithAnyOtherDoor(Transform door)
    {
        foreach (GameObject existingRoom in generatedRooms)
        {
            foreach (Transform existingDoor in GetDoors(existingRoom))
            {
                if (existingDoor != door && Vector3.Distance(door.position, existingDoor.position) < 0.1f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    Transform[] GetDoors(GameObject room)
    {
        List<Transform> doors = new();
        foreach (Transform child in room.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("Door"))
            {
                doors.Add(child);
            }
        }
        return doors.ToArray();
    }

    Vector2 GetPositionNearDoor(Transform door)
    {
        Vector3 doorPos = door.position;
        Vector2 gridPos = new Vector2(Mathf.Round(doorPos.x / gridSize), Mathf.Round(doorPos.z / gridSize));

        Vector2[] adjacentPositions = new[]
        {
            gridPos + Vector2.up, gridPos + Vector2.down, gridPos + Vector2.left, gridPos + Vector2.right
        };

        foreach (Vector2 pos in adjacentPositions)
        {
            if (!occupiedPositions.Contains(pos))
            {
                return pos;
            }
        }

        return Vector2.negativeInfinity; 
    }
}
