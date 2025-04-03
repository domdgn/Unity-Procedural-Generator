using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonTiler : MonoBehaviour
{
    public GameObject masterPrefab;
    public int maxRooms = 10;
    
    private List<GameObject> placedRooms = new List<GameObject>();

    private void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        GameObject firstRoom = Instantiate(masterPrefab, Vector3.zero, Quaternion.identity);
        placedRooms.Add(firstRoom);

        for (int i = 1; i < maxRooms; i++)
        {
            PlaceNextRoom();
        }
    }

    void PlaceNextRoom()
    {
        GameObject sourceRoom = placedRooms[Random.Range(0, placedRooms.Count)];

        Transform[] allSourceChildren = sourceRoom.GetComponentsInChildren<Transform>();

        List<Transform> entryPoints = new List<Transform>();

        foreach (Transform child in allSourceChildren)
        {
            if (child.CompareTag("Entry"))
            {
                EntryPoint entryScript = child.GetComponent<EntryPoint>();
                if (entryScript != null && entryScript.isConnected)
                {
                    continue;
                }

                entryPoints.Add(child);
            }
        }

        Debug.Log("Total entry points found: " + entryPoints.Count);

        if (entryPoints.Count == 0)
        {
            Debug.LogWarning("No entry points found in the source room.");
            return;
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
        Debug.Log("Total next entry points found: " + nextEntryPoints.Count);

        if (nextEntryPoints.Count == 0)
        {
            Debug.LogWarning("No entry points found in the next room.");
            return;
        }

        Transform nextEntryPoint = nextEntryPoints[Random.Range(0, nextEntryPoints.Count)];

        nextRoom.transform.rotation = Quaternion.LookRotation(-sourceEntryPoint.forward, Vector3.up);

        Vector3 offset = sourceEntryPoint.position - nextEntryPoint.position;
        nextRoom.transform.position += offset;

        EntryPoint sourceEntryScript = sourceEntryPoint.GetComponent<EntryPoint>();
        EntryPoint nextEntryScript = nextEntryPoint.GetComponent<EntryPoint>();

        sourceEntryScript.isConnected = true;
        nextEntryScript.isConnected = true;

        placedRooms.Add(nextRoom);
    }
}
