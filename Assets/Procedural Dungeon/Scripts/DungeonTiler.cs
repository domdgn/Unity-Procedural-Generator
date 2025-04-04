using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTiler : MonoBehaviour
{
    public GameObject masterPrefab;
    public int maxRooms = 10;
    public int maxFailedAttempts = 25;
    int failedAttempts = 0;
    
    private List<GameObject> placedRooms = new List<GameObject>();

    private void Start()
    {
        Debug.Log("=== STARTING DUNGEON GENERATION ===");
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        Debug.Log("Creating first room...");
        GameObject firstRoom = Instantiate(masterPrefab, Vector3.zero, Quaternion.identity);
        placedRooms.Add(firstRoom);
        Debug.Log($"First room placed. Starting generation of {maxRooms - 1} additional rooms");

        int currentRooms = 1;

        while(currentRooms < maxRooms && failedAttempts < maxFailedAttempts)
        {
            if (PlaceNextRoom())
            {
                currentRooms++;
                Debug.Log($"Placed room {currentRooms}/{maxRooms}");
            }
            else
            {
                failedAttempts++;
                Debug.LogWarning($"Failed attempt {failedAttempts}/{maxFailedAttempts}");
            }
        }
    }

    bool PlaceNextRoom()
    {
        int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            GameObject sourceRoom = placedRooms[Random.Range(0, placedRooms.Count)];
            Transform[] allSourceChildren = sourceRoom.GetComponentsInChildren<Transform>();

            List<Transform> entryPoints = new List<Transform>();

            foreach (Transform child in allSourceChildren)
            {
                if (child.CompareTag("Entry"))
                {
                    EntryPoint entryScript = child.GetComponent<EntryPoint>();
                    if (entryScript != null && !entryScript.isConnected)
                    {
                        entryPoints.Add(child);
                    }          
                }
            }

            if (entryPoints.Count == 0)
            {
                Debug.LogWarning("No entry points found in the source room.");
                continue;
            }

            Transform sourceEntryPoint = entryPoints[Random.Range(0, entryPoints.Count)];
            GameObject nextRoom = Instantiate(masterPrefab);

            Transform[] allNextChildren = nextRoom.GetComponentsInChildren<Transform>();

            List<Transform> nextEntryPoints = new List<Transform>();

            foreach (Transform child in allNextChildren)
            {
                if (child.CompareTag("Entry"))
                {
                    nextEntryPoints.Add(child);
                }
            }

            if (nextEntryPoints.Count == 0)
            {
                Destroy(nextRoom);
                continue;
            }

            Transform nextEntryPoint = nextEntryPoints[Random.Range(0, nextEntryPoints.Count)];

            nextRoom.transform.localRotation = Quaternion.LookRotation(-sourceEntryPoint.localPosition, Vector3.up);

            Vector3 offset = sourceEntryPoint.position - nextEntryPoint.position;
            nextRoom.transform.position += offset;

            if (IsRoomOverlapping(nextRoom, sourceRoom))
            {
                Destroy(nextRoom);
                Debug.LogWarning($"Room overlap detected at {nextRoom.transform.position}");
                continue;
            }

            Debug.Log($"Successfully placed room at {nextRoom.transform.position}");
            sourceEntryPoint.GetComponent<EntryPoint>().isConnected = true;
            nextEntryPoint.GetComponent<EntryPoint>().isConnected = true;
            placedRooms.Add(nextRoom);
            return true;
        }
        Debug.LogWarning("Failed to place room after maximum attempts");
        return false;
    }

    private bool IsRoomOverlapping(GameObject nextRoom, GameObject sourceRoom)
    {
        foreach (GameObject placedRoom in placedRooms)
        {
            if (nextRoom.GetComponent<BoxCollider>().bounds.Intersects(placedRoom.GetComponent<BoxCollider>().bounds))
            {
                return true;
            }
        }
        return false;
    }
}
