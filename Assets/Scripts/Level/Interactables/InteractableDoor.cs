using System;
using UnityEngine;

public enum DoorState
{
    Open,
    Closed,
    Locked, // For exit door

    Opening,
    Closing
}

// Class for other doors to listen to, so they can close and unload before the new one opens
public static class DoorListener
{
    public static event Action<InteractableDoor> OnDoorOpened;
    public static event Action<InteractableDoor> OnDoorOpening;
    public static void RaiseOpened(InteractableDoor d) => OnDoorOpened?.Invoke(d);
    public static void RaiseOpening(InteractableDoor d) => OnDoorOpening?.Invoke(d);
}

[RequireComponent(typeof(Collider))]
public class InteractableDoor : MonoBehaviour, IInteractable
{
    [Header("Feedback")]
    [SerializeField] private Animator animator;

    // Just adding source and clips here over having an audio manager
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSfx;
    [SerializeField] private AudioClip closeSfx;

    [SerializeField] private Transform tempDoorHingeRef;

    private DoorState state = DoorState.Closed;
    private Collider col;

    private bool waiting = false;

    // Events
    public event Action OnDoorOpening;
    public event Action OnDoorOpened;
    public event Action OnDoorClosed;

    private void Start()
    {
        col = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        DoorListener.OnDoorOpened += HandleOtherDoorOpened;
        DoorListener.OnDoorOpening += HandleDoorOpening;
    }

    private void OnDisable()
    {
        DoorListener.OnDoorOpened -= HandleOtherDoorOpened;
        DoorListener.OnDoorOpening -= HandleDoorOpening;
    }

    private void HandleOtherDoorOpened(InteractableDoor d)
    {
        waiting = false;
    }

    private void HandleDoorOpening(InteractableDoor d)
    {
        if (d != this)
        {
            waiting = true;
            TryClose();
        } 
    }

    public string GetPrompt()
    {
        return state switch
        {
            //DoorState.Open => "Press (e) to close",
            DoorState.Closed => "Press (e) to open",
            DoorState.Locked => "Locked",
            _ => "" // Default return empty -- Opening/Closing
        };
    }

    public void Interact(GameObject interactor)
    {
        if (waiting)
            return;

        switch (state)
        {
            case DoorState.Open:
                //TryClose(); -- Could add a collider that enables when triggered to block the door but more effort than its worth
                break;

            case DoorState.Closed:
                Open();
                break;
        }
    }

    public void Open()
    {
        if (state != DoorState.Closed)
            return;

        DoorListener.RaiseOpening(this);

        col.enabled = false;

        state = DoorState.Opening;
        OnDoorOpened?.Invoke();

        animator.SetTrigger("Open");
        SoundManager.Instance.PlaySFXSingle(openSfx);
    }

    public void TryClose()
    {
        if (state == DoorState.Closed || state == DoorState.Closing)
            return;

        Close();
    }

    private void Close()
    {
        state = DoorState.Closing;
        animator.SetTrigger("Close");
        //PlaySfx(closeSfx);
    }

    public void OpenFinished()
    {
        state = DoorState.Open;
        DoorListener.RaiseOpened(this);
    }

    public void CloseFinished()
    {
        OnDoorClosed?.Invoke();
        state = DoorState.Closed;
        col.enabled = true;
    }
    public void Unlock() => state = DoorState.Closed;

    private void PlaySfx(AudioClip clip)
    {
        if (clip && audioSource)
            audioSource.PlayOneShot(clip);
    }
}
