using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimEventRelay : MonoBehaviour
{
    [SerializeField] private MonoBehaviour target;

    public void AttackFinished()
    {
        (target as LurkingMonster)?.AnimationCallback();
    }

    public void OpenFinished() =>
        (target as InteractableDoor)?.OpenFinished();

    public void CloseFinished() =>
        (target as InteractableDoor)?.CloseFinished();
}
