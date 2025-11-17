public enum RoomType
{
    Good,
    Bad
}

public enum RoomInteractionType
{
    // Candy
    Searchable,
    Pickup,

    // Can be good or bad
    TelevisionKey, // Good gives keypad part | Bad gives monster attack upon interaction
    TelevisionClue, // Good gives truthful clue | (not implemented) Bad gives untruthful clue

    GoodMonster, // If we are interacting with monsters directly

    ForcedMonster, // Collider onenter triggers encounter
    MonsterTV,
}