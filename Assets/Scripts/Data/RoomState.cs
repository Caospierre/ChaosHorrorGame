using System.Linq;

public class RoomState
{
    public RoomSO SO;
    public ClueData clue;
    public float prefabSeed;
    public float spawnSeed;
    public int Floor;
    public int Index;
    public bool IsGood;

    public bool HasClue => clue.Type != ClueType.Default;
    public bool HasKeyCodeClue => clue.Type == ClueType.CodePosition || clue.Type == ClueType.CodeSum;
    public bool HasTVClueSO => SO.HasTVClue;
}