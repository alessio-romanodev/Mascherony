using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public float MoveInput { get; private set; }

    // one–frame inputs
    public bool JumpPressed { get; private set; }
    public bool DropDownPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool DashPressed { get; private set; }

    // held input
    public bool JumpHeld { get; private set; }

    public bool DropDown => DropDownPressed;

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

        inputActions.Player.Jump.started += _ =>
            JumpPressed = true;

        inputActions.Player.Jump.performed += _ =>
            JumpHeld = true;

        inputActions.Player.Jump.canceled += _ =>
            JumpHeld = false;

        inputActions.Player.DropDown.performed += _ =>
            DropDownPressed = true;

        inputActions.Player.Attack.performed += _ =>
            AttackPressed = true;

        inputActions.Player.Dash.performed += _ =>
            DashPressed = true;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void LateUpdate()
    {
        JumpPressed = false;
        DropDownPressed = false;
        AttackPressed = false;
        DashPressed = false;
    }
}
