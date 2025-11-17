using UnityEngine;

using UnityEngine.Windows;

[RequireComponent(typeof(CharacterController), typeof(InputController), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float turnSpeed = 2000f;
    [SerializeField] private float gravity = -40f;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 2.0f;
    [SerializeField] private float minVelocity = 0.2f;

    private CharacterController controller;
    private InputController inputController;
    private float verticalVelocity;
    private float stepCycle;
    private int lastStepIndex = -1;
    private Animator anim;


    
    // Giro 180° (S)
    bool isTurning = false;
    Quaternion targetTurnRotation;

    // Giro 90° (A / D)
    bool isSideTurning = false;
    Quaternion targetSideRotation;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputController = GetComponent<InputController>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        foreach (var clip in footstepClips)
        {
            if (clip != null)
                SoundManager.Instance.RegisterClip(clip.name, clip);
        }
    }

    private void Update()
    {
        if (UIManager.Instance != null)
            if (!UIManager.Instance.canPlayerMove)
                return;

        Vector2 mv = inputController.Move;

        if (isTurning || isSideTurning)
        {
            if (isTurning)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetTurnRotation,
                    turnSpeed * Time.deltaTime
                );

                if (Quaternion.Angle(transform.rotation, targetTurnRotation) < 1f)
                    isTurning = false;
            }

            if (isSideTurning)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetSideRotation,
                    turnSpeed * Time.deltaTime
                );

                if (Quaternion.Angle(transform.rotation, targetSideRotation) < 1f)
                    isSideTurning = false;
            }

            anim.SetBool("isMoving", false);
            return;
        }

        if (mv.y < -0.5f && !isTurning)
        {
            isTurning = true;
            targetTurnRotation = Quaternion.Euler(
                0f,
                transform.eulerAngles.y + 180f,
                0f
            );
            return;
        }

        if (mv.x < -0.5f && !isSideTurning) // A → -90°
        {
            isSideTurning = true;
            targetSideRotation = Quaternion.Euler(
                0f,
                transform.eulerAngles.y - 90f,
                0f
            );
            return;
        }

        if (mv.x > 0.5f && !isSideTurning) // D → +90°
        {
            isSideTurning = true;
            targetSideRotation = Quaternion.Euler(
                0f,
                transform.eulerAngles.y + 90f,
                0f
            );
            return;
        }

        // ============================================
        float forwardInput = Mathf.Max(0f, mv.y);
        Vector3 dir = transform.forward * forwardInput;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = dir * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        anim.SetBool("isMoving", forwardInput > 0.1f);
    }

    public void PlayFootstep()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayFootstep();
    }
}
