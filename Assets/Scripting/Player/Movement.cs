using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float jumpForce = 5f;
    [Tooltip("Multiplier for horizontal control when in the air")]
    public float airControlMultiplier = 0.5f;

    [Header("Sprinting Settings")]
    public float sprintMultiplier = 1.5f;
    public float staminaConsumptionRate = 20f;
    public float staminaRecoveryRate = 10f;

    [Header("Step Bobbing Settings")]
    public float stepBobbingSpeed = 10f;
    public float stepBobbingAmount = 0.1f;

    [Header("Crouch Settings")]
    [Tooltip("Target transform for the camera when crouching")]
    public Transform crouchCameraTarget;
    [Tooltip("New collider height when crouching (e.g., 1.213064)")]
    public float crouchColliderHeight = 1.213064f;
    [Tooltip("New collider center Y when crouching (e.g., -0.3628699)")]
    public float crouchColliderCenterY = -0.3628699f;

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    [Tooltip("UI Slider for health")]
    public Slider healthSlider;
    [Tooltip("UI Slider for stamina")]
    public Slider staminaSlider;

    [Header("References")]
    [Tooltip("Assign your Cinemachine Virtual Camera here")]
    public CinemachineVirtualCamera virtualCamera;
    [Tooltip("Assign the Capsule Collider on your player")]
    public CapsuleCollider capsuleCollider;

    // Internal variables
    private Rigidbody rb;
    private Transform camTransform;
    private Vector3 camStandLocalPos;
    private float standColliderHeight;
    private Vector3 standColliderCenter;
    private bool isCrouching = false;
    private float stepTimer = 0f;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (virtualCamera != null)
        {
            camTransform = virtualCamera.transform;
            camStandLocalPos = camTransform.localPosition;
        }

        if (capsuleCollider != null)
        {
            standColliderHeight = capsuleCollider.height;
            standColliderCenter = capsuleCollider.center;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    void Update()
    {
        HandleInput();

        // Only run step bobbing when not crouching.
        if (!isCrouching)
            HandleStepBobbing();
        else if (crouchCameraTarget != null)
            camTransform.localPosition = crouchCameraTarget.localPosition;

        UpdateUI();
    }

    void FixedUpdate()
    {
        // Gravity and physics are handled by the Rigidbody.
    }

    void HandleInput()
    {
        // Use the capsule collider's bounds for a reliable ground check.
        float groundDistance = capsuleCollider.bounds.extents.y + 0.1f;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance);

        // Movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveDir = (transform.right * moveX + transform.forward * moveZ).normalized;

        // Determine speed – sprinting if LeftShift is held, moving, grounded, and stamina is available.
        float currentSpeed = 0f;
        if (isGrounded && Input.GetKey(KeyCode.LeftShift) && moveDir.magnitude > 0.1f && currentStamina > 0)
        {
            currentSpeed = walkSpeed * sprintMultiplier;
            currentStamina -= staminaConsumptionRate * Time.deltaTime;
            if (currentStamina < 0)
                currentStamina = 0;
        }
        else
        {
            currentSpeed = isGrounded ? walkSpeed : walkSpeed * airControlMultiplier;
            currentStamina += staminaRecoveryRate * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }

        // Preserve the vertical velocity while updating horizontal movement.
        Vector3 currentVelocity = rb.velocity;
        Vector3 desiredVelocity = moveDir * currentSpeed;
        currentVelocity.x = desiredVelocity.x;
        currentVelocity.z = desiredVelocity.z;
        rb.velocity = currentVelocity;

        // Jumping: if on ground and Jump is pressed, reset vertical velocity and add jump force.
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        // Crouching: hold LeftControl to stay crouched.
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Crouch();
        }
        else
        {
            StandUp();
        }
    }

    void HandleStepBobbing()
    {
        if (isGrounded && (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f))
        {
            stepTimer += Time.deltaTime * stepBobbingSpeed;
            float bobbingOffset = Mathf.Sin(stepTimer) * stepBobbingAmount;
            Vector3 newPos = camStandLocalPos;
            newPos.y += bobbingOffset;
            camTransform.localPosition = newPos;
        }
        else
        {
            stepTimer = 0f;
            camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, camStandLocalPos, Time.deltaTime * stepBobbingSpeed);
        }
    }

    void Crouch()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            if (virtualCamera != null && crouchCameraTarget != null)
                camTransform.localPosition = crouchCameraTarget.localPosition;

            if (capsuleCollider != null)
            {
                capsuleCollider.height = crouchColliderHeight;
                capsuleCollider.center = new Vector3(standColliderCenter.x, crouchColliderCenterY, standColliderCenter.z);
            }
        }
    }

    void StandUp()
    {
        if (isCrouching)
        {
            isCrouching = false;
            if (virtualCamera != null)
                camTransform.localPosition = camStandLocalPos;

            if (capsuleCollider != null)
            {
                capsuleCollider.height = standColliderHeight;
                capsuleCollider.center = standColliderCenter;
            }
        }
    }

    void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;
        if (staminaSlider != null)
            staminaSlider.value = currentStamina;
    }
}
