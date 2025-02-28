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
        CheckGround();
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
            Vector3 moveVector = AdjustVelocityForSlope(moveDir * currentSpeed);
            rb.velocity = new Vector3(moveVector.x, rb.velocity.y, moveVector.z);
        }
        else
        {
            Vector3 airMove = moveDir * (currentSpeed * airControlMultiplier);
            rb.velocity += new Vector3(airMove.x, 0, airMove.z) * Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && isGrounded && currentStamina >= jumpStaminaCost)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            currentStamina -= jumpStaminaCost;
        }

        if (Input.GetKey(KeyCode.LeftControl))
            Crouch();
        else
            StandUp();
    }

    void CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, capsuleCollider.bounds.extents.y + 0.2f, groundLayer))
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
