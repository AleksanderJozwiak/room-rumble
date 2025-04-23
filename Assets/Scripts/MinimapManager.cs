using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    public GameObject roomIconPrefab;
    public Transform minimapParent;
    public Sprite sRoomSprite, lRoomSprite, iRoomSprite;

    //private Dictionary<Vector2, GameObject> roomIcons = new();

    public GameObject playerDotPrefab;
    private RectTransform playerDot;
    private Transform player;
    private Vector2 currentRoomGridPos;
    private float worldRoomSize = 30f; // same as grid size in DungeonGenerator

    private float roomSize = 30f;

    void Update()
    {
        if (playerDot == null || player == null) return;

        // Local offset from center of room, normalized
        Vector3 playerWorldPos = player.position;
        Vector2 roomWorldCenter = new Vector2(currentRoomGridPos.x * worldRoomSize, currentRoomGridPos.y * worldRoomSize);
        Vector2 localOffset = new Vector2(
            playerWorldPos.x - roomWorldCenter.x,
            playerWorldPos.z - roomWorldCenter.y
        );

        // Normalize offset based on room size (so it’s -0.5 to 0.5 range)
        Vector2 normalizedOffset = localOffset / worldRoomSize;

        // Convert to UI offset
        Vector2 iconOffset = normalizedOffset * roomSize;

        // Final anchored position
        playerDot.anchoredPosition = currentRoomGridPos * roomSize + iconOffset;
    }


    public void TrackPlayer(Transform playerTransform, Vector2 startingGridPos)
    {
        player = playerTransform;
        currentRoomGridPos = startingGridPos;
    }

    public void InitializeMinimap(Dictionary<Vector2, GameObject> rooms)
    {
        foreach (var kvp in rooms)
        {
            Vector2 gridPos = kvp.Key;
            Room room = kvp.Value.GetComponent<Room>();

            GameObject icon = Instantiate(roomIconPrefab, minimapParent);
            RectTransform rect = icon.GetComponent<RectTransform>();
            //rect.anchoredPosition = gridPos * roomSize;
            Sprite roomSprite = sRoomSprite;
            if (room.roomType == RoomType.L)
            {
                roomSprite = lRoomSprite;
            }
            else if (room.roomType == RoomType.I)
            {
                roomSprite = iRoomSprite;
            }
            icon.GetComponent<Image>().sprite = roomSprite;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, roomSize);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, roomSize);
            rect.SetLocalPositionAndRotation(
                new Vector3(gridPos.x * roomSize, gridPos.y * roomSize, 0),
                Quaternion.Euler(0, 0, 360-room.gameObject.transform.rotation.eulerAngles.y)
            );

            //icon.SetActive(false); // Hidden until visited

            //roomIcons[gridPos] = icon;

            // Optional: special icon for portal
            //if (room is PortalRoom)
            //    icon.GetComponent<Image>().sprite = portalRoomSprite;
        }
        playerDot = Instantiate(playerDotPrefab, minimapParent).GetComponent<RectTransform>();
        TrackPlayer(GameObject.FindGameObjectWithTag("Player").transform, new Vector2(0, 0));
    }

    public void RevealRoom(Vector2 position, bool isPlayerHere)
    {
        //if (roomIcons.TryGetValue(position, out GameObject icon))
        //{
        //    icon.SetActive(true);
        //    icon.GetComponent<Image>().sprite = isPlayerHere ? playerRoomSprite : defaultRoomSprite;
        //}

        //// Reset other rooms if player moved
        //foreach (var kvp in roomIcons)
        //{
        //    if (kvp.Key != position && kvp.Value.activeSelf)
        //        kvp.Value.GetComponent<Image>().sprite = defaultRoomSprite;
        //}
    }
}
