using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class Room : MonoBehaviour
{
    [System.Serializable]
    private class RoomContentEntry
    {
        public RoomInteractionType type;
        public Transform[] spawnPoints;
        public GameObject[] prefabs;
    }

    [Header("Static Components")]
    [SerializeField] private InteractableDoor door;
    [SerializeField] private GameObject interior; // Gets enabled/disabled so the door stays visible
    
    // For debug purposes
    [SerializeField] private TextMeshPro roomNumberDisplay;

    [Header("Room Content")]
    [Tooltip("Will not spawn an entry more than once")]
    [SerializeField] private List<RoomContentEntry> roomContent = new();

    private Dictionary<RoomInteractionType, Transform[]> contentSpawns = new();
    private Dictionary<RoomInteractionType, GameObject[]> contentPrefabs = new();

    public RoomSO ScriptableObject { get; set; }

    private void Awake()
    {
        // Build dictionaries from content entries -- Ignores duplicate entries
        foreach (RoomContentEntry entry in roomContent)
        {
            if (entry.spawnPoints != null && entry.spawnPoints.Length > 0 && !contentSpawns.ContainsKey(entry.type))
                contentSpawns.Add(entry.type, entry.spawnPoints);

            if (entry.prefabs != null && entry.prefabs.Length > 0 && !contentPrefabs.ContainsKey(entry.type))
                contentPrefabs.Add(entry.type, entry.prefabs);
        }
    }

    private void Start()
    {
        interior.SetActive(false);
    }

    private void OnEnable()
    {
        door.OnDoorOpened += HandleDoorOpen;
        door.OnDoorClosed += HandleDoorClose;
    }

    private void OnDisable()
    {
        door.OnDoorOpened -= HandleDoorOpen;
        door.OnDoorClosed -= HandleDoorClose;
    }

    private void HandleDoorOpen()
    {
        // Run any other init
        interior.SetActive(true);
    }

    private void HandleDoorClose()
    {
        interior.SetActive(false);
        // Run any other cleanup
    }

    public bool SpawnRoomContent(RoomInteractionType type, int amount)
    {
        return SpawnRoomContent(type, amount, Random.value, Random.value);
    }

    public bool SpawnRoomContent(RoomInteractionType type, int amount, float prefabSeed, float spawnSeed)
    {
        if (!contentPrefabs.TryGetValue(type, out GameObject[] prefabs) || 
            !contentSpawns.TryGetValue(type, out Transform[] spawns))
            return false;


        int toCreate = Mathf.Min(amount, prefabs.Length, spawns.Length);
        if (toCreate == 0)
            return false;

        // Shuffle both lists
        prefabs = prefabs.OrderBy(_ => prefabSeed).ToArray();
        spawns = spawns.OrderBy(_ => spawnSeed).ToArray();

        // Spawn one prefab per slot
        for (int i = 0; i < toCreate; i++)
        {
            Instantiate(prefabs[i], spawns[i]);
        }

        return true;
    }

    // Spawn content and return so caller can configure -- ie. Set clue data
    public GameObject SpawnOne(RoomInteractionType type)
    {
        return SpawnOne(type, Random.value, Random.value);
    }

    // Spawn content and return so caller can configure -- ie. Set clue data
    public GameObject SpawnOne(RoomInteractionType type, float prefabSeed, float spawnSeed)
    {
        if (!contentPrefabs.TryGetValue(type, out GameObject[] prefabs) || !contentSpawns.TryGetValue(type, out var spawns))
            return null;

        GameObject prefab = prefabs.OrderBy(_ => prefabSeed).FirstOrDefault();
        Transform spawn = spawns.OrderBy(_ => spawnSeed).FirstOrDefault();

        if (prefab == null || spawn == null)
            return null;

        return Instantiate(prefab, spawn);
    }

    // For debug purposes
    public void DebugAsignFloatingRoomNumber(int num)
    {
        DebugAsignFloatingRoomNumber(num.ToString());
    }

    public void DebugAsignFloatingRoomNumber(string num)
    {
        if (roomNumberDisplay != null)
        {
            roomNumberDisplay.text = num;

            //if (gameObject.transform.position.x < 0)
            //    roomNumberDisplay.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 180);
        }
    }
}
