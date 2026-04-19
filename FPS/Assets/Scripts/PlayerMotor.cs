using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private PlayerController playerController;

    private CharacterController controller;
    private MovementControls controls;
    private Vector2 moveInput;
    private float weaponInput;
    private bool sprintHeld;
    private bool jumpPressed;
    private Vector3 velocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (controls == null)
            controls = new MovementControls();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        if (controls == null)
            controls = new MovementControls();

        controls.Enable();
        controls.PlayerMap.Move.performed += OnMove;
        controls.PlayerMap.Move.canceled += OnMove;
        controls.PlayerMap.Sprint.performed += OnSprint;
        controls.PlayerMap.Sprint.canceled += OnSprint;
        controls.PlayerMap.Jump.performed += OnJump;
        controls.PlayerMap.PrimaryWeapon.performed += OnPrimaryWeapon;
        controls.PlayerMap.SecondaryWeapon.performed += OnSecondaryWeapon;
    }

    private void OnDisable()
    {
        if (controls == null)
            return;

        controls.PlayerMap.Move.performed -= OnMove;
        controls.PlayerMap.Move.canceled -= OnMove;
        controls.PlayerMap.Sprint.performed -= OnSprint;
        controls.PlayerMap.Sprint.canceled -= OnSprint;
        controls.PlayerMap.Jump.performed -= OnJump;
        controls.PlayerMap.PrimaryWeapon.performed -= OnPrimaryWeapon;
        controls.PlayerMap.SecondaryWeapon.performed -= OnSecondaryWeapon;
        controls.Disable();
    }

    private void OnPrimaryWeapon(InputAction.CallbackContext context)
    {
        if (playerController != null)
            playerController.switchWeapon(1);
    }

    private void OnSecondaryWeapon(InputAction.CallbackContext context)
    {
        if (playerController != null)
            playerController.switchWeapon(2);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        sprintHeld = context.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    private void Update()
    {
        float currentSpeed = sprintHeld ? sprintSpeed : walkSpeed;

        bool isActuallySprinting = sprintHeld && moveInput.sqrMagnitude > 0.01f;
        if (playerController != null)
        {
            playerController.sprintAnim(isActuallySprinting);
        }

        float yaw = transform.eulerAngles.y;
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 move = yawRotation * new Vector3(moveInput.x, 0f, moveInput.y);
        move = Vector3.ClampMagnitude(move, 1f);

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (jumpPressed && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        jumpPressed = false;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}