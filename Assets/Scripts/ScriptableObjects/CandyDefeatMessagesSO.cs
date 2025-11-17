using UnityEngine;

[CreateAssetMenu(fileName = "CandyDefeatMessages", menuName = "Game/Candy Defeat Messages")]
public class CandyDefeatMessagesSO : ScriptableObject
{
    [TextArea(2, 4)]
    public string[] messages;

    public string GetRandomMessage()
    {
        if (messages == null || messages.Length == 0)
        {
            return "You ran out of candy!";
        }

        // Filter out null or empty messages
        var validMessages = new System.Collections.Generic.List<string>();
        foreach (var msg in messages)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                validMessages.Add(msg);
            }
        }

        // If no valid messages, return default
        if (validMessages.Count == 0)
        {
            return "You ran out of candy!";
        }

        return validMessages[Random.Range(0, validMessages.Count)];
    }
}

