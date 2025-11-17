using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Room")]
public class RoomSO : ScriptableObject
{
    [System.Serializable]
    private class RoomContentEntry
    {
        public RoomInteractionType type;
        public int amount;
    }

    [SerializeField] private RoomType type;
    [SerializeField, Min(0.0001f)] private float spawnWeight = 1f;

    [SerializeField] private Room prefab;
    [SerializeField] private RoomContentEntry[] interactionEntries;

    private List<(RoomInteractionType type, int amount)> interactions;

    private void OnEnable()
    {
        // Build once
        interactions = interactionEntries != null
            ? interactionEntries.Select(e => (e.type, e.amount)).ToList()
            : new List<(RoomInteractionType type, int amount)>();
    }

    // Getters
    public RoomType Type => type;
    public float SpawnWeight => spawnWeight;
    public Room Prefab => prefab;
    public List<(RoomInteractionType type, int amount)> Interactions => interactions;
    public bool HasTVClue => interactions.Any(interaction => interaction.type == RoomInteractionType.TelevisionClue);
}
