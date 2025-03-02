using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 5f;
    public float airControlMultiplier = 0.3f;
    public float slopeAcceleration = 3f;
    public float maxSlopeAngle = 45f;

    [Header("Sprinting Settings")]
    public float sprintMultiplier = 1.5f;
    public float staminaConsumptionRate = 20f;
    public float staminaRecoveryRate = 10f;
    public float sprintCooldownThreshold = 20f;
    public float jumpStaminaCost = 15f;

    [Header("Step Bobbing Settings")]
    public float stepBobbingSpeed = 10f;
    public float stepBobbingAmount = 0.1f;

    [Header("Crouch Settings")]
    public Transform crouchCameraTarget;
    public float crouchSpeedMultiplier = 0.5f; // Slow movement while crouching
    public float defaultPlayerScale = 1.5f; // Default player scale
    public float crouchPlayerScale = 0.8f;  // Scale when crouching
    public float scaleSpeed = 8f; // How fast scaling transitions

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float maxStamina = 100f;
    public float currentStamina = 100f;
    public Slider healthSlider;
    public Slider staminaSlider;

    [Header("Fall Damage Settings")]
    public float fallDamageThreshold = 3f;
    public float fallDamageMultiplier = 10f; // Damage per unit of fall distance beyond threshold

    [Header("References")]
    public CinemachineVirtualCamera virtualCamera;
    public CapsuleCollider capsuleCollider;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Transform camTransform;
    private Vector3 camStandLocalPos;
    private bool isCrouching = false;
    private float stepTimer = 0f;
    private bool isGrounded = false;
    private bool isSliding = false;
    private bool canSprint = true;
    private bool isMoving = false;
    private Vector3 slopeNormal;

    // Variables for fall damage
    private bool wasGrounded = true;
    private float fallStartHeight;

    // Variable to store jump direction
    private Vector3 jumpDirection = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (virtualCamera != null)
        {
            camTransform = virtualCamera.transform;
            camStandLocalPos = camTransform.localPosition;
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
        // --- Ground & Fall Damage Check ---
        // Use a raycast from near the bottom of the capsule
        Vector3 rayOrigin = new Vector3(transform.position.x, capsuleCollider.bounds.min.y + 0.05f, transform.position.z);
        RaycastHit hit;
        float rayLength = 0.2f; // A short distance from the base
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, groundLayer))
        {
            isGrounded = true;
            slopeNormal = hit.normal;
            float angle = Vector3.Angle(Vector3.up, slopeNormal);
            isSliding = angle > maxSlopeAngle;
        }
        else
        {
            isGrounded = false;
            isSliding = false;
        }

        // --- Fall Damage ---
        // If we just left the ground, record the height.
        if (wasGrounded && !isGrounded)
        {
            fallStartHeight = transform.position.y;
        }
        // If we just landed, calculate fall damage.
        if (!wasGrounded && isGrounded)
        {
            float fallDistance = fallStartHeight - transform.position.y;
            if (fallDistance > fallDamageThreshold)
            {
                float damage = (fallDistance - fallDamageThreshold) * fallDamageMultiplier;
                currentHealth -= damage;
                Debug.Log("Fall damage taken: " + damage);
                if (currentHealth < 0) currentHealth = 0;
            }
        }
        wasGrounded = isGrounded;

        // --- Input & Movement ---
        HandleInput();

        if (!isCrouching)
            HandleStepBobbing();
        else if (crouchCameraTarget != null)
            camTransform.localPosition = crouchCameraTarget.localPosition;

        UpdateUI();
    }

    void FixedUpdate()
    {
        if (isSliding)
        {
            HandleSlopeSliding();
        }
    }

    void HandleInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        isMoving = (moveX != 0 || moveZ != 0);

        // Calculate desired move direction based on input.
        Vector3 moveDir = (transform.right * moveX + transform.forward * moveZ).normalized;

        float currentSpeed = isCrouching ? crouchSpeed : walkSpeed;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && moveDir.magnitude > 0.1f && canSprint;

        if (isSprinting)
        {
            if (currentStamina > 0)
            {
                currentSpeed *= sprintMultiplier;
                currentStamina -= staminaConsumptionRate * Time.deltaTime;
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    canSprint = false;
                }
            }
        }
        else
        {
            currentStamina += staminaRecoveryRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
            if (currentStamina >= sprintCooldownThreshold) canSprint = true;
        }

        if (isGrounded)
        {
            // Reset jumpDirection when grounded.
            jumpDirection = Vector3.zero;
            Vector3 moveVector = AdjustVelocityForSlope(moveDir * currentSpeed);
            rb.velocity = new Vector3(moveVector.x, rb.velocity.y, moveVector.z);
        }
        else
        {
            // In air, restrict input to mostly the original jump direction.
            if (jumpDirection != Vector3.zero)
            {
                float dot = Vector3.Dot(jumpDirection, moveDir);
                // Only allow air control if input is in roughly the same direction as the jump.
                if (dot < 0.1f)
                {
                    moveDir = Vector3.zero;
                }
            }
            Vector3 airMove = moveDir * (currentSpeed * airControlMultiplier);
            // Here we add a small correction to the horizontal velocity.
            rb.velocity = new Vector3(rb.velocity.x + airMove.x * Time.deltaTime, rb.velocity.y, rb.velocity.z + airMove.z * Time.deltaTime);
        }

        // Jumping: only allow if grounded.
        if (Input.GetButtonDown("Jump") && isGrounded && currentStamina >= jumpStaminaCost)
        {
            // Record the input move direction as jump direction.
            jumpDirection = moveDir;
            // If no input, use the forward direction.
            if (jumpDirection == Vector3.zero)
                jumpDirection = transform.forward;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            currentStamina -= jumpStaminaCost;
        }

        // Crouching
        if (Input.GetKey(KeyCode.LeftControl))
            Crouch();
        else
            StandUp();
    }

    Vector3 AdjustVelocityForSlope(Vector3 moveDir)
    {
        if (Vector3.Angle(Vector3.up, slopeNormal) <= maxSlopeAngle)
        {
            return Vector3.ProjectOnPlane(moveDir, slopeNormal);
        }
        return moveDir;
    }

    void HandleSlopeSliding()
    {
        Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, slopeNormal).normalized;
        rb.velocity += slideDirection * slopeAcceleration * Time.deltaTime;
    }

    void HandleStepBobbing()
    {
        if (!isGrounded || !isMoving) return;

        stepTimer += Time.deltaTime * stepBobbingSpeed;
        float bobbingOffset = Mathf.Sin(stepTimer) * stepBobbingAmount;
        camTransform.localPosition = camStandLocalPos + new Vector3(0, bobbingOffset, 0);
    }

    void Crouch()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            StartCoroutine(ScalePlayer(crouchPlayerScale));
        }
    }

    void StandUp()
    {
        if (isCrouching)
        {
            isCrouching = false;
            StartCoroutine(ScalePlayer(defaultPlayerScale));
        }
    }

    System.Collections.IEnumerator ScalePlayer(float targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(startScale.x, targetScale, startScale.z);
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * scaleSpeed;
            transform.localScale = Vector3.Lerp(startScale, endScale, time);
            yield return null;
        }
    }

    void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (staminaSlider != null) staminaSlider.value = currentStamina;
    }
}
