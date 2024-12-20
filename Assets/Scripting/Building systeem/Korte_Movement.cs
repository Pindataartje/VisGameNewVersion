using UnityEngine;

public class Korte_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f; // Renamed from moveSpeed
    public float jumpHeight = 5f; // Renamed from jumpForce

    [Header("Mouse Settings")]
    public float lookSensitivity = 100f;
    public Transform cameraTransform;

    [Header("Ground Settings")]
    public LayerMask groundLayer; // Layer mask for ground detection

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool onFloor; // Renamed from isGrounded

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Handle Mouse Look
        MouseLook();

        // Handle Movement
        Movement();
    }

    void FixedUpdate()
    {
        // Handle Jumping
        if (Input.GetButtonDown("Jump") && onFloor)
        {
            Jump();
        }
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D keys
        float vertical = Input.GetAxis("Vertical"); // W/S keys

        // Calculate movement direction
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Apply movement (preserve Y velocity for physics)
        Vector3 velocity = rb.velocity;
        rb.velocity = new Vector3(move.x * walkSpeed, velocity.y, move.z * walkSpeed);
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if player is on the floor (using layer)
        if (IsInLayerMask(collision.gameObject.layer, groundLayer))
        {
            onFloor = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Check if player left the floor (using layer)
        if (IsInLayerMask(collision.gameObject.layer, groundLayer))
        {
            onFloor = false;
        }
    }

    bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) > 0;
    }
}
