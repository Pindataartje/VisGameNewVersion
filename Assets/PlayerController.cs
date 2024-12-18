using Cinemachine;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float stamina = 5f;
    public float staminaRecoveryRate = 1f;

    [Header("Look Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float maxVerticalAngle = 89f; // Restrict vertical look
    private float verticalRotation = 0f;

    [Header("Interaction Settings")]
    public float interactionRange = 5f;
    public string interactableTag = "Controller";
    public CinemachineVirtualCamera playerCameraVirtual;
    public CinemachineVirtualCamera boatCamera;
    public MonoBehaviour boatController; // Reference to the BoatController script
    public GameObject playerController;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool isSprinting = false;
    private bool inBoat = false;

    private void Start()
    {
        // Initialize components
        controller = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;

        // Ensure correct initial states
        boatController.enabled = false; // Disable the boat controller script initially
        playerController.SetActive(true); // Player controller enabled initially
    }

    private void Update()
    {
        if (!inBoat)
        {
            HandleMovement();
            HandleLook();
            HandleInteraction();
        }
    }

    private void HandleMovement()
    {
        // Check if player is grounded
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // Movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Sprinting
        isSprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0;
        float speed = isSprinting ? sprintSpeed : moveSpeed;

        if (isSprinting)
        {
            stamina -= Time.deltaTime; // Decrease stamina while sprinting
        }
        else if (stamina < 5f)
        {
            stamina += staminaRecoveryRate * Time.deltaTime; // Recover stamina
        }

        controller.Move(move * speed * Time.deltaTime);

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            playerVelocity.y = jumpForce;
        }

        // Apply gravity
        playerVelocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Horizontal rotation
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

        // Apply rotation to camera
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Shoot a ray from the camera
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
            {
                if (hit.collider.CompareTag(interactableTag))
                {
                    ToggleControlMode();
                }
            }
        }
    }

    private void ToggleControlMode()
    {
        inBoat = !inBoat;

        if (inBoat)
        {
            // Switch to boat control
            playerCameraVirtual.Priority = 0;
            boatCamera.Priority = 10;
            playerController.SetActive(false); // Disable player movement
            boatController.enabled = true;    // Enable the boat controller script
        }
        else
        {
            // Switch to player control
            playerCameraVirtual.Priority = 10;
            boatCamera.Priority = 0;
            playerController.SetActive(true); // Enable player movement
            boatController.enabled = false;   // Disable the boat controller script
        }
    }
}
