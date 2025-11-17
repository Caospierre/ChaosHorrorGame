using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HotelLayoutManager : MonoBehaviour
{
    public static HotelLayoutManager Instance { get; private set; }
    public int CurrentFloor { get; private set; } = 0;
    public int MaxFloor => hotelData.TotalFloors;

    [SerializeField] private Transform[] roomSpawnPoints;
    [SerializeField] private HotelSO hotelData;

    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private string lobbySpawnId = "FloorToLobby";
    [SerializeField] private string hallwaySceneName = "Floor";

    [field: SerializeField] public string Keycode { get; private set; }
    private int keycodeHintsGiven = 0;
    private HashSet<string> usedClues = new();

    [Tooltip("Set Seed to 0 to generate a random seed.")]
    [SerializeField] private int seed = 0;
    private Dictionary<int, Dictionary<int, RoomState>> floorRoomMap = new();
    private Dictionary<Room, int> roomNumberByInstance = new();

    private int currentCandy = 0;
    private int keypadFails = 0;

    public int CurrentCandy => currentCandy;
    public int KeypadFails => keypadFails;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate HotelLayoutManager deleted.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;

        if (seed == 0) seed = System.DateTime.Now.ToString().GetHashCode();
        Random.InitState(seed);

        Keycode = string.Concat(
            Enumerable.Range(0, hotelData.RequiredKeypadParts)
            .Select(_ => Random.Range(0, 10).ToString())
        );

        InitializeHotelFloorMap();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    [ContextMenu("Restart Game")]
    public void RestartWithNewSeed()
    {
        seed = System.DateTime.Now.ToString().GetHashCode();
        Random.InitState(seed);

        Keycode = string.Concat(
            Enumerable.Range(0, hotelData.RequiredKeypadParts)
            .Select(_ => Random.Range(0, 10).ToString())
        );

        CurrentFloor= 0;
        keypadFails = 0;
        keycodeHintsGiven = 0;
        currentCandy = 0;
        usedClues.Clear();
        InitializeHotelFloorMap();
        SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
    }

    public void GoToFloor(int floor)
    {
        if (floor == CurrentFloor) return;

        CurrentFloor = floor;
        // Floor 0 is the lobby, otherwise we want to load a hallway.
        if(floor == 0)
        {
            SceneLoader.Instance.LoadScene(lobbySceneName, lobbySpawnId);
        }
        else
        {
            SceneLoader.Instance.LoadScene(hallwaySceneName);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        roomNumberByInstance.Clear();
        roomSpawnPoints = new Transform[0];
        if (roomSpawnPoints.Length == 0)
        {
            roomSpawnPoints = GameObject.FindGameObjectsWithTag("RoomSpawnPoint")
                .Select(go => go.transform)
                .OrderBy(transform => transform.name)
                .ToArray();
        }

        InstantiateRooms();
        SpawnRoomContent();
    }

    private void InitializeHotelFloorMap()
    {
        int totalRooms = hotelData.RoomsPerFloor;
        if (totalRooms <= 0) return;

        RoomSO[] pool;
        for (int floorIdx = 1; floorIdx < MaxFloor; floorIdx++)
        {
            if (!floorRoomMap.ContainsKey(floorIdx))
            {
                floorRoomMap[floorIdx] = new Dictionary<int, RoomState>();
            }

            for (int roomIdx = 0; roomIdx < totalRooms; roomIdx++)
            {
                RoomState roomState = new RoomState();

                roomState.IsGood = Random.value < hotelData.GoodRoomRatio;
                pool = roomState.IsGood ? hotelData.GoodRoomPool : hotelData.BadRoomPool;
                roomState.SO = WeightedPick(pool, roomState.IsGood);

                roomState.Floor = floorIdx;
                roomState.Index = roomIdx;
                roomState.prefabSeed = Random.value;
                roomState.spawnSeed = Random.value;

                floorRoomMap[floorIdx][roomIdx] = roomState;
            }
        }

        // Try to guarantee enough TV rooms, MakeClue will handle keycode clues.
        var goodRooms = floorRoomMap.SelectMany(kv => kv.Value.Values).Where(rs => rs.IsGood);
        int keycodeLen = hotelData.RequiredKeypadParts;
        int tvRoomCount = goodRooms.Count(rs => rs.HasTVClueSO);
        // If there's enough good rooms to hold all the keycode clues, but not enough tv rooms yet
        if (keycodeLen <= goodRooms.Count() && tvRoomCount < keycodeLen)
        {
            // While we have less tv rooms than good rooms, and haven't placed enough tv rooms yet
            RoomState roomStateToReplace;
            RoomSO newTVRoomSO;
            while (tvRoomCount < goodRooms.Count() && tvRoomCount < keycodeLen)
            {
                // Replace a non-tv room with a tv room
                roomStateToReplace = floorRoomMap.SelectMany(kv => kv.Value.Values)
                    .Where(rs => rs.IsGood && !rs.HasTVClueSO)
                    .OrderBy(_ => Random.value)
                    .First();

                newTVRoomSO = hotelData.GoodRoomPool.Where(roomSO => roomSO.HasTVClue)
                    .OrderBy(_ => Random.value)
                    .First();

                // Because we're only replacing good rooms with other good rooms, the good room ratio stays the same
                floorRoomMap[roomStateToReplace.Floor][roomStateToReplace.Index].SO = newTVRoomSO;
                goodRooms = floorRoomMap.SelectMany(kv => kv.Value.Values).Where(rs => rs.IsGood);
                tvRoomCount = goodRooms.Count(rs => rs.HasTVClueSO);
            }
        }

        // Randomize rooms
        var allRoomState = Enumerable.Range(1, MaxFloor - 1)
            .SelectMany(floor => floorRoomMap[floor].Values)
            .OrderBy(_ => Random.value)
            .ToList();

        // Apply clues
        foreach(RoomState rs in allRoomState)
        {
            foreach (var (type, amount) in rs.SO.Interactions)
            {
                if (type == RoomInteractionType.TelevisionClue)
                {
                    ClueData clueData = MakeClue(rs);
                    rs.clue = TryRegisterClue(clueData) ? clueData : default;
                    break;
                }
            }
        }
    }

    private void InstantiateRooms()
    {
        int totalRooms = roomSpawnPoints.Length;
        if (totalRooms <= 0) return;

        for (int i = 0; i < totalRooms; i++)
        {
            var spawnPoint = roomSpawnPoints[i];
            foreach (Transform child in spawnPoint) Destroy(child.gameObject);

            RoomState roomState = floorRoomMap[CurrentFloor][i];
            Room room = Instantiate(roomState.SO.Prefab, spawnPoint);
            room.ScriptableObject = roomState.SO;

            string roomNumber = $"{CurrentFloor}{i:D2}";
            room.name = $"Room: {roomNumber}";

            roomNumberByInstance.Add(room, i);
            room.DebugAsignFloatingRoomNumber(roomNumber);
        }
    }

    private RoomSO WeightedPick(RoomSO[] pool, bool isGood)
    {
        if (pool == null || pool.Length == 0) return null;

        // Sum weights (guarding against tiny/zero values)
        float total = 0f;
        foreach (var r in pool)
        {
            total += Mathf.Max(0.0001f, r.SpawnWeight);
        }

        float roll = Random.value * total;
        foreach (var r in pool)
        {
            roll -= Mathf.Max(0.0001f, r.SpawnWeight);
            if (roll <= 0f) return r;
        }

        return pool[pool.Length - 1];
    }

    private void SpawnRoomContent()
    {
        List<Room> currentFloorRooms = roomNumberByInstance.Keys.ToList();
        foreach (var room in currentFloorRooms)
        {
            RoomSO so = room.ScriptableObject;
            if (so == null) continue;

            foreach (var (type, amount) in so.Interactions)
            {
                RoomState roomState = floorRoomMap[CurrentFloor][roomNumberByInstance[room]];
                if (type == RoomInteractionType.TelevisionClue)
                {
                    for (int i = 0; i < amount; i++) // Unlikely to have multiple clues in one room
                    {
                        // Currently not handling clues in bad rooms
                        if (!IsRoomGood(room)) continue;

                        GameObject obj = room.SpawnOne(type, roomState.prefabSeed, roomState.spawnSeed);

                        // Ran out of things to spawn
                        if (!obj) break;

                        ClueData clue = roomState.clue;
                        if (obj.TryGetComponent<IClueOwner>(out var owner)) owner.SetClue(clue);
                    }
                }
                else
                {
                    room.SpawnRoomContent(type, amount, roomState.prefabSeed, roomState.spawnSeed);
                }
            }
        }
    }

    private ClueData MakeClue(RoomState rs)
    {
        if(keycodeHintsGiven < hotelData.RequiredKeypadParts)
        {
            return MakeCodePositionsClue(rs.Index);
        }

        float totalWeight =
            hotelData.AssertClueRatio +
            hotelData.AdjacentClueRatio +
            hotelData.AmongSetClueRatio +
            hotelData.RangeClueRatio +
            hotelData.XorClueRatio +
            hotelData.CodeSumClueRatio;
        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        cumulative += hotelData.AssertClueRatio;
        if (roll < cumulative)
        {
            return MakeAssertClue(rs.Index);    
        }

        cumulative += hotelData.AdjacentClueRatio;
        if (roll < cumulative)
        {
            return MakeAdjacentClue(rs.Index);
        } 

        cumulative += hotelData.XorClueRatio;
        if (roll < cumulative)
        {
            return MakeXorClue(rs.Index);
        }

        cumulative += hotelData.AmongSetClueRatio;
        if (roll < cumulative)
        {
            return MakeAmongSetClue(rs.Index);
        }
        
        cumulative += hotelData.RangeClueRatio;
        if (roll < cumulative)
        {
            return MakeRangeClue(rs.Index);
        }

        cumulative += hotelData.CodeSumClueRatio;
        if (roll < cumulative)
        {
            return MakeCodeSumClue(rs.Index);
        }

        return default;
    }

    private ClueData MakeAssertClue(int roomIdx)
    {
        if (hotelData.RoomsPerFloor <= 0) return default;

        // Range starts at 1 to avoid the lobby floor.
        int floor = Random.Range(1, MaxFloor);
        int index = Random.Range(0, hotelData.RoomsPerFloor);
        bool good = IsRoomGood(floor, index);

        var clue = new ClueData(good ? ClueType.AssertGood : ClueType.AssertBad, floor, index);
        return clue;
    }

    private ClueData MakeAdjacentClue(int roomIdx)
    {
        if (hotelData.RoomsPerFloor <= 1) return default;

        // Range starts at 1 to avoid the lobby floor.
        int floor = Random.Range(1, MaxFloor);

        // choose a source index that has a neighbor
        int src = Random.Range(0, hotelData.RoomsPerFloor);

        // Adjacent rooms are +/- 2 in index. Eg. the left side goes 00, 02, 04, ...
        int left = src - 2;
        int right = src + 2;
        var adjacentRooms = new List<int>();
        if (left >= 0) adjacentRooms.Add(left);
        if (right < hotelData.RoomsPerFloor) adjacentRooms.Add(right);
        if (adjacentRooms.Count == 0) return default;

        int tgt = adjacentRooms[Random.Range(0, adjacentRooms.Count)];
        bool isGood = IsRoomGood(floor, tgt);

        var clue = new ClueData(isGood ? ClueType.AdjacentGood : ClueType.AdjacentBad, floor, src, tgt);
        return clue;
    }

    private ClueData MakeAmongSetClue(int roomIdx)
    {
        if (hotelData.RoomsPerFloor < 3) return default;
        int floor = Random.Range(1, MaxFloor);

        // If there's less than 2 rooms of either type, this assertion fails. There is certainly a better way to do this, but this is good enough.
        if(Enumerable.Range(0, hotelData.RoomsPerFloor).Where(i => IsRoomGood(floor, i)).Count() < 2 ||
            Enumerable.Range(0, hotelData.RoomsPerFloor).Where(i => !IsRoomGood(floor, i)).Count() < 2)
        {
            return default;
        }

        // Good clue means 1 good and 2 bad. Bad clue means 1 bad and 2 good.
        bool goodClue = Random.value < 0.5f;
        List<int> threeRooms = new();
        if (goodClue)
        {
            int goodRoom = Enumerable.Range(0, hotelData.RoomsPerFloor).Where(i => IsRoomGood(floor, i)).OrderBy(_ => Random.value).First();
            List<int> badRooms = Enumerable.Range(0, hotelData.RoomsPerFloor).Where(i => !IsRoomGood(floor, i)).OrderBy(_ => Random.value).Take(2).ToList();
            threeRooms = badRooms.Append(goodRoom).OrderBy(_ => Random.value).ToList();

            if (threeRooms.Count < 3)
            {
                Debug.Log($"MakeAmongSetClue could not resolve, defaulting to normal assertion.");
                return default;
            }
        }
        else
        {
            int badRoom = Enumerable.Range(0, hotelData.RoomsPerFloor).Where(i => !IsRoomGood(floor, i)).OrderBy(_ => Random.value).First();
            List<int> goodRooms = Enumerable.Range(0, hotelData.RoomsPerFloor).Where(i => IsRoomGood(floor, i)).OrderBy(_ => Random.value).Take(2).ToList();
            threeRooms = goodRooms.Append(badRoom).OrderBy(_ => Random.value).ToList();

            if (threeRooms.Count < 3)
            {
                Debug.Log($"MakeAmongSetClue could not resolve, defaulting to normal assertion.");
                return default;
            }
        }

        ClueData clue = new ClueData(goodClue ? ClueType.AmongSetGood : ClueType.AmongSetBad, floor, threeRooms[0], threeRooms[1], threeRooms[2]);
        return clue;
    }

    private ClueData MakeRangeClue(int roomIdx)
    {
        if (hotelData.RoomsPerFloor <= 0) return default;
        
        int floor = Random.Range(1, MaxFloor);
        int goodCount = floorRoomMap[floor].Values.Count(rs => rs.IsGood);
        int badCount = hotelData.RoomsPerFloor - goodCount;
        bool isGoodClue = Random.value < 0.5f;

        var clue = new ClueData(isGoodClue ? ClueType.RangeGood : ClueType.RangeBad, floor, isGoodClue ? goodCount : badCount);
        return clue;
    }

    private ClueData MakeXorClue(int roomIdx)
    {
        int floor = Random.Range(1, MaxFloor);

        var goodIdx = Enumerable.Range(0, hotelData.RoomsPerFloor).Where(i => IsRoomGood(floor, roomIdx)).ToList();
        var badIdx = Enumerable.Range(0, hotelData.RoomsPerFloor).Where(i => !IsRoomGood(floor, roomIdx)).ToList();
        if (goodIdx.Count == 0 || badIdx.Count == 0) return default;

        int a = goodIdx[Random.Range(0, goodIdx.Count)];
        int b = badIdx[Random.Range(0, badIdx.Count)];

        bool positiveForm = Random.value < 0.5f;
        bool flip = Random.value < 0.5f;
        int X = flip ? a : b;
        int Y = flip ? b : a;

        var clue = new ClueData(positiveForm ? ClueType.XorGood : ClueType.XorBad, floor, X, Y);
        return clue;
    }

    private ClueData MakeCodePositionsClue(int roomIdx)
    {
        int position = keycodeHintsGiven + 1; // 1-based indexing
        int digit = int.Parse(Keycode[keycodeHintsGiven++].ToString());

        return new ClueData
        {
            Type = ClueType.CodePosition,
            floor = CurrentFloor,
            A = digit,
            B = position
        };
    }

    private ClueData MakeCodeSumClue(int roomIdx)
    {
        // Safety check – ensure we have a valid keycode
        if (string.IsNullOrEmpty(Keycode))
        {
            Debug.LogWarning("[MakeCodeSumClue] Keycode is missing or empty. Returning fallback clue.");
            return default;
        }

        // Sum all digits in the Keycode string
        int sum = 0;
        foreach (char c in Keycode)
        {
            if (char.IsDigit(c))
                sum += c - '0'; // Convert character digit to int
        }

        // Return the clue data
        return new ClueData(
            ClueType.CodeSum,
            CurrentFloor,
            sum
        );
    }

    public void AddCandy(int amount = 1)
    {
        currentCandy += amount;
        UIManager.Instance.UpdateCandyDisplay(currentCandy);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayCandyAdd();
    }

    public void RemoveCandy(int amount = 1)
    {
        currentCandy -= amount;

        if (currentCandy < 0)
            UIManager.Instance.ShowCandyDefeat();

        if (SoundManager.Instance != null)
        {
            Debug.Log("Candy negative test");
            SoundManager.Instance.PlayCandyRemove();
        }

        UIManager.Instance.UpdateCandyDisplay(currentCandy);
    }

    private bool TryRegisterClue(ClueData c)
    {
        if (c.Type == ClueType.Default) return true;
        return usedClues.Add($"{c.Type}|{c.floor}|{c.A}|{c.B}|{c.C}");
    }

    public void AddKeypadFail() => keypadFails++;

    // Helper for clarity
    private bool IsRoomGood(Room r) => r.ScriptableObject.Type == RoomType.Good;
    private bool IsRoomGood(int floor, Room r) => floorRoomMap[floor][roomNumberByInstance[r]].IsGood;
    private bool IsRoomGood(int floor, int roomIdx) => floorRoomMap[floor][roomIdx].IsGood;
}
