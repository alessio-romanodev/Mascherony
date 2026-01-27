using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public float MoveInput { get; private set; }

    // one–frame inputs
    public bool JumpPressed { get; private set; }
    public bool DropDownPressed { get; private set; }
    public bool ActionPressed { get; private set; }

    // held input
    public bool JumpHeld { get; private set; }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx =>
            MoveInput = ctx.ReadValue<float>();

        inputActions.Player.Move.canceled += _ =>
            MoveInput = 0f;

        // JUMP
        inputActions.Player.Jump.started += _ =>
            JumpPressed = true;

        inputActions.Player.Jump.performed += _ =>
            JumpHeld = true;

        inputActions.Player.Jump.canceled += _ =>
            JumpHeld = false;

        // DROP DOWN
        inputActions.Player.DropDown.performed += _ =>
            DropDownPressed = true;

        // ACTION
        inputActions.Player.Action.performed += _ =>
            ActionPressed = true;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void LateUpdate()
    {
        // reset one-frame inputs
        JumpPressed = false;
        DropDownPressed = false;
        ActionPressed = false;
    }
}
