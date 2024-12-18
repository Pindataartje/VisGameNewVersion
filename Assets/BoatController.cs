using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    public Rigidbody rb;

    // Movement parameters
    public float enginePower = 2000f;        // Force applied to move the boat forward
    public float maxSpeed = 20f;            // Maximum forward speed
    public float reverseSpeed = 5f;         // Maximum reverse speed
    public float turnSpeed = 5f;            // Turning speed factor
    public float drag = 0.99f;              // Drag to slow down the boat gradually

    // Steering parameters
    public float rudderAngle = 15f;         // Max angle the boat can turn (in degrees)
    public float turnInfluence = 0.1f;      // How much speed affects turning

    // Input variables
    private float forwardInput = 0f;        // Forward/backward input
    private float turnInput = 0f;           // Turning input

    private void FixedUpdate()
    {
        // Get player input
        forwardInput = Input.GetAxis("Vertical");   // W/S or Up/Down arrow keys
        turnInput = Input.GetAxis("Horizontal");   // A/D or Left/Right arrow keys

        // Move the boat forward/backward
        HandleMovement();

        // Turn the boat realistically
        HandleTurning();

        // Apply drag to slow the boat naturally
        ApplyDrag();
    }

    private void HandleMovement()
    {
        // Calculate force based on input and engine power
        float force = forwardInput * enginePower;

        // Forward motion
        if (forwardInput > 0)
        {
            // Limit max forward speed
            if (rb.velocity.magnitude < maxSpeed)
            {
                rb.AddForce(transform.forward * force * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
        // Reverse motion
        else if (forwardInput < 0)
        {
            // Limit max reverse speed
            if (rb.velocity.magnitude < reverseSpeed)
            {
                rb.AddForce(transform.forward * force * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
    }

    private void HandleTurning()
    {
        if (rb.velocity.magnitude > 0.1f) // Allow turning only if the boat is moving
        {
            // Adjust turn based on forward speed
            float turn = turnInput * turnSpeed * (1 + rb.velocity.magnitude * turnInfluence);

            // Apply turning torque
            rb.AddTorque(Vector3.up * turn * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }

    private void ApplyDrag()
    {
        // Apply linear drag
        rb.velocity *= drag;

        // Apply angular drag
        rb.angularVelocity *= drag;
    }
}
