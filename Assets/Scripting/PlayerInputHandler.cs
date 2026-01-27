using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public float MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool DropDownPressed { get; private set; }
    public bool ActionPressed { get; private set; }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx =>
            MoveInput = ctx.ReadValue<float>();

        inputActions.Player.Move.canceled += ctx =>
            MoveInput = 0f;

        inputActions.Player.Jump.performed += _ =>
            JumpPressed = true;

        inputActions.Player.DropDown.performed += _ =>
            DropDownPressed = true;

        inputActions.Player.Action.performed += _ =>
            ActionPressed = true;
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void LateUpdate()
    {
        // reset input one-frame
        JumpPressed = false;
        DropDownPressed = false;
        ActionPressed = false;
    }
}
