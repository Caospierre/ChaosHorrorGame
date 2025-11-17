using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TVMonster : MonoBehaviour
{
    private CandyController cc;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Init(CandyController controller) =>
        cc = controller;

    public void InitiateAttack()
    {
        anim.SetTrigger("Attack");
        // Add audio here
    }

    public void AttackFinished() // Animation callback
    {
        //cc.RemoveCandy(1);
        UIManager.Instance.MonsterTvAttackFinished();
        HotelLayoutManager.Instance.RemoveCandy();
        Destroy(gameObject);
    }
}
