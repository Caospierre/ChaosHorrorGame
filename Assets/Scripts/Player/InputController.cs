using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    // Gameplay
    public Vector2 Move { get; private set; }

    public event Action InteractPressed;
    public event Action CancelPressed;
    public event Action ClickPressed;
    // UI
    public event Action ClosePressed; // E or Escape
    public event Action EscapePressed;

    // Keypad
    public event Action<string> KeypadPressed;

    private PlayerInputs input;

    private void Awake()
    {
        input = new PlayerInputs();
    }

    private void OnEnable()
    {
        if (input != null)
        {
            input.Player.Enable();
            input.Player.Interact.performed += OnInteract;
            input.Player.Cancel.performed += OnCancel;
            input.Player.Click.performed += OnClick;
            input.Player.Escape.performed += OnEscape;
            
            input.UI.Close.performed += OnClose;
            input.UI.Escape.performed += OnEscape;
            input.UI.Submit.performed += HandleOnSubmit;
            input.UI.Backspace.performed += OnBackspace;
            input.UI.Clear.performed += OnClear;

            // There is a better way to do this but I'm tired and can't think of it
            input.UI._1.performed += OnNum1;
            input.UI._2.performed += OnNum2;
            input.UI._3.performed += OnNum3;
            input.UI._4.performed += OnNum4;
            input.UI._5.performed += OnNum5;
            input.UI._6.performed += OnNum6;
            input.UI._7.performed += OnNum7;
            input.UI._8.performed += OnNum8;
            input.UI._9.performed += OnNum9;
            input.UI._0.performed += OnNum0;
        }
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.Player.Interact.performed -= OnInteract;
            input.Player.Cancel.performed -= OnCancel;
            input.Player.Click.performed -= OnClick;
            input.Player.Escape.performed -= OnEscape;

            input.UI.Close.performed -= OnClose;
            input.UI.Escape.performed -= OnEscape;
            input.UI.Submit.performed -= HandleOnSubmit;
            input.UI.Backspace.performed -= OnBackspace;
            input.UI.Clear.performed -= OnClear;

            input.UI._1.performed -= OnNum1;
            input.UI._2.performed -= OnNum2;
            input.UI._3.performed -= OnNum3;
            input.UI._4.performed -= OnNum4;
            input.UI._5.performed -= OnNum5;
            input.UI._6.performed -= OnNum6;
            input.UI._7.performed -= OnNum7;
            input.UI._8.performed -= OnNum8;
            input.UI._9.performed -= OnNum9;
            input.UI._0.performed -= OnNum0;

            input.Disable();
        }
    }

    private void Update()
    {
        if (input != null)
            Move = input.Player.Move.ReadValue<Vector2>();
    }

    private void OnInteract(InputAction.CallbackContext ctx) => InteractPressed?.Invoke();
    private void OnClose(InputAction.CallbackContext ctx) => ClosePressed?.Invoke();
    private void OnCancel(InputAction.CallbackContext ctx) => CancelPressed?.Invoke();
    private void OnClick(InputAction.CallbackContext ctx) => ClickPressed?.Invoke();
    private void OnEscape(InputAction.CallbackContext ctx) => EscapePressed?.Invoke();
    #region Keypad Input Handlers
    private void HandleOnSubmit(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("Submit");
    private void OnBackspace(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("Backspace");
    private void OnClear(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("Clear");
    private void OnNum1(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("1");
    private void OnNum2(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("2");
    private void OnNum3(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("3");
    private void OnNum4(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("4");
    private void OnNum5(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("5");
    private void OnNum6(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("6");
    private void OnNum7(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("7");
    private void OnNum8(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("8");
    private void OnNum9(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("9");
    private void OnNum0(InputAction.CallbackContext ctx) => KeypadPressed?.Invoke("0");
    #endregion

    public void EnableGameplayInputs()
    {
        input.Player.Enable();
        input.UI.Disable();
    }

    public void EnableUIInputs()
    {
        input.Player.Disable();
        input.UI.Enable();
    }
   


}
