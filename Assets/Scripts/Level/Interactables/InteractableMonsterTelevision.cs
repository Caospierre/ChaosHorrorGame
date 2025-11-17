using System.Collections.Generic;
using UnityEngine;

public class MonsterTVInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private TVMonster monsterPrefab;
    [SerializeField] private Transform monsterSpawn;

    private bool isInteractable = true;
    private TVMonster spawnedMonster = null;

    [SerializeField]
    private List<string> messages = new List<string>
    {
        "You are already dead",
        "There is no escape",
        "Do you feel the breath on your neck?",
        "Behind you",
        "It likes when you pretend you don't see it",
        "You are no longer alone",
        "It's been following you",
        "If you are reading this,\nScream",
        "It's closer than your shadow"
    };

    public string GetPrompt()
    {
        return "Press (e) to watch";
    }

    public void Interact(GameObject interactor)
    {
        if (!isInteractable || !UIManager.Instance)
            return;

        if (interactor.TryGetComponent<CandyController>(out CandyController cc))
        {
            isInteractable = false;
            UIManager.Instance.OnMonsterTvClosed += UIClosed;
            UIManager.Instance.DisplayMonsterTv(PickRandomMessage());
            SpawnMonster(cc);
        }
    }

    private string PickRandomMessage()
    {
        int r = Random.Range(0, messages.Count);
        return messages[r];
    }

    private void SpawnMonster(CandyController cc)
    {
        spawnedMonster = Instantiate(monsterPrefab, monsterSpawn);
        spawnedMonster.Init(cc);
    }

    private void UIClosed()
    {
        spawnedMonster.InitiateAttack();
        UIManager.Instance.OnMonsterTvClosed -= UIClosed;
    }

    private void OnDisable()
    {
        UIManager.Instance.OnMonsterTvClosed -= UIClosed;
    }
}
