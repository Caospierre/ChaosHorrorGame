using System;
using System.Linq;
using Random = UnityEngine.Random;

public enum ClueType
{
    Default,

    // Room X is good/bad
    AssertGood,
    AssertBad,

    // Room next to X is good/bad
    AdjacentGood,
    AdjacentBad,

    // Among set of rooms
    AmongSetGood,
    AmongSetBad,

    // Number of good/bad rooms
    RangeGood,
    RangeBad,

    // Room X or Y is good/bad
    XorGood,
    XorBad,

    // Code value in X position
    CodePosition,
    CodeSum,
}

[Serializable]
public struct ClueData
{
    public ClueType Type;
    public int floor;
    public int A;
    public int B;
    public int C;

    public ClueData(ClueType type, int floor, string a, string b = "0", string c = "0")
    {
        Type = type;
        this.floor = floor;
        A = int.Parse(a);
        B = int.Parse(b);
        C = int.Parse(c);
    }

    public ClueData(ClueType type, int floor, int a, int b = 0, int c = 0)
    {
        Type = type;
        this.floor = floor;
        A = a;
        B = b;
        C = c;
    }

    public override string ToString()
    {
        return Type switch
        {
            // Default. We use this to prevent duplicate clues. These are just nonsense clues.
            ClueType.Default => NonsenseClue(),

            // Assert
            ClueType.AssertGood => $"Room {floor}{A:D2} is safe",
            ClueType.AssertBad => $"Room {floor}{A:D2} is not safe",
            // Adjacent
            ClueType.AdjacentGood => $"A room that is next to {floor}{A:D2} is safe",
            ClueType.AdjacentBad =>  $"A room that is next to {floor}{A:D2} is dangerous",
            //Among set of rooms
            ClueType.AmongSetGood => $"One of {floor}{A:D2}, {floor}{B:D2}, or {floor}{C:D2} rooms is safe",
            ClueType.AmongSetBad => $"One of {floor}{A:D2}, {floor}{B:D2}, or {floor}{C:D2} rooms is not safe",
            // Range of rooms is good/bad
            ClueType.RangeGood => $"Floor {floor} has {A} safe rooms",
            ClueType.RangeBad => $"Floor {floor} has {A} dangerous rooms",
            // X or
            ClueType.XorGood => $"Either room {floor}{A:D2} or room {floor}{B:D2} is safe",
            ClueType.XorBad => $"Either room {floor}{A:D2} or room {floor}{B:D2} is dangerous",
            // Keycode
            ClueType.CodePosition => CodePositionClue(A, B),
            ClueType.CodeSum => $"The sum of the passcode values equal {A}",
            _ => ""
        };
    }

    private string NonsenseClue()
    {
        string[] clues = { 
            "You can never escape", "No way out", "Be afraid", "They know", "Don't talk to strangers", 
            "Trust no one", "The walls are listening", "It's all a dream", "Wake up", "Follow the white rabbit",
            "Reality is an illusion", "Time is a flat circle", "The cake is a lie", "Everything is connected",
            "What you seek is seeking you", "The truth is out there", "I see dead people", "All your base are belong to us",
            "No one knows where the remote is", "How do you turn this thing off?", "It watches from the dark", "We shouldn't be here",
            "It knows your name", "They move when you blink", "It's closer than before", "Keep quiet", "The darkness will consume you"
        };
        return clues[Random.Range(0, clues.Length - 1)];
    }

    private string CodePositionClue(int A, int B)
    {
        // Make a string of code length underscores
        int codeLength = HotelLayoutManager.Instance.Keycode.Length;
        string[] clueDigits = Enumerable.Repeat("_", codeLength).ToArray();
        clueDigits[B - 1] = A.ToString();
        return string.Join(" ", clueDigits);
    }
}