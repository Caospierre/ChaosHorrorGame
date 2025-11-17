using UnityEngine;

[CreateAssetMenu(menuName = "SO/HotelSO")]
public class HotelSO : ScriptableObject
{
    [SerializeField] private int totalFloors = 3;
    [SerializeField] private int roomsPerFloor = 6;
    [Range(0, 1)]
    [SerializeField] private float goodRoomRatio = 0.7f;

    [Header("Required Counts")]
    [SerializeField] private int requiredKeypadParts = 6;
    [SerializeField] private Vector2 candyRange = new Vector2(3, 5);

    [Header("Pools")]
    [SerializeField] private RoomSO[] goodRoomPool;
    [SerializeField] private RoomSO[] badRoomPool;

    [Header("Clues")]

    [Range(0, 1)]
    [SerializeField] private float assertClueRatio = 0.15f;
    [Range(0, 1)]
    [SerializeField] private float adjacentClueRatio = 0.15f;
    [Range(0, 1)]
    [SerializeField] private float amongSetClueRatio = 0.15f;
    [Range(0, 1)]
    [SerializeField] private float rangeClueRatio = 0.15f;
    [Range(0, 1)]
    [SerializeField] private float xorClueRatio = 0.3f;
    // Keycode clues can't be left up to chance.
    //[SerializeField, Range(0, 1)] private float codePositionClueRatio = 0.15f;
    [Range(0, 1)]
    [SerializeField] private float codeSumClueRatio = 0.10f;

    public int TotalFloors => totalFloors;
    public int RoomsPerFloor => roomsPerFloor;
    public float GoodRoomRatio => goodRoomRatio;
    public int RequiredKeypadParts => requiredKeypadParts;
    public Vector2 CandyRange => candyRange;
    public RoomSO[] GoodRoomPool => goodRoomPool;
    public RoomSO[] BadRoomPool => badRoomPool;
    public float AssertClueRatio => assertClueRatio;
    public float AdjacentClueRatio => adjacentClueRatio;
    public float AmongSetClueRatio => amongSetClueRatio;
    public float RangeClueRatio => rangeClueRatio;
    public float XorClueRatio => xorClueRatio;
    public float CodeSumClueRatio => codeSumClueRatio;
}
