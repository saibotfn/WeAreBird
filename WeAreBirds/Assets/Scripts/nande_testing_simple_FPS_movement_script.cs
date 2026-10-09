using UnityEngine;
using UnityEngine.InputSystem;

public class nande_testing_simple_FPS_movement_script : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Camera playerCamera;
    public float mouseSensitivity = 0.15f;

    [Header("Interaction")]
    public float interactDistance = 3f;
    public LayerMask interactLayers = ~0;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraRotationX;
    private bool cursorLocked = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        LockCursor(true);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard == null)
            return;

        // Toggle cursor lock
        if (keyboard.escapeKey.wasPressedThisFrame)
            LockCursor(!cursorLocked);

        if (!cursorLocked &&
            mouse != null &&
            mouse.leftButton.wasPressedThisFrame)
        {
            LockCursor(true);
        }

        if (!cursorLocked)
            return;

        HandleMovement(keyboard);
        HandleMouseLook(mouse);
        HandleInteraction(keyboard);
    }

    void HandleMovement(Keyboard keyboard)
    {
        // WASD movement
        float x = 0f;
        float z = 0f;

        if (keyboard.wKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;
        if (keyboard.aKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;

        Vector3 direction =
            transform.right * x +
            transform.forward * z;

        // Prevent faster diagonal movement
        direction = Vector3.ClampMagnitude(direction, 1f);

        // Sprint
        float speed = keyboard.leftShiftKey.isPressed
            ? sprintSpeed
            : walkSpeed;

        controller.Move(direction * speed * Time.deltaTime);

        // Ground check
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Jump
        if (keyboard.spaceKey.wasPressedThisFrame &&
            controller.isGrounded)
        {
            velocity.y =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook(Mouse mouse)
    {
        if (mouse == null || playerCamera == null)
            return;

        Vector2 mouseDelta = mouse.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // Vertical camera rotation
        cameraRotationX -= mouseY;
        cameraRotationX =
            Mathf.Clamp(cameraRotationX, -90f, 90f);

        playerCamera.transform.localRotation =
            Quaternion.Euler(cameraRotationX, 0f, 0f);

        // Horizontal player rotation
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleInteraction(Keyboard keyboard)
    {
        if (playerCamera == null)
            return;

        if (keyboard.eKey.wasPressedThisFrame)
        {
            Ray ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );

            if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactDistance,
                interactLayers))
            {
                nande_testing_interactable_object_script interactable =
                    hit.collider.GetComponentInParent<nande_testing_interactable_object_script>();

                if (interactable != null)
                    interactable.Interact();
            }
        }
    }

    void LockCursor(bool locked)
    {
        cursorLocked = locked;

        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !locked;
    }
}
