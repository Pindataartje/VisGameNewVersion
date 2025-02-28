using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Floater : MonoBehaviour
{
    public Rigidbody rb;
    public float depthBeforeSubmersion = 1f; // Depth at which the floater is fully submerged
    public float displacementAmount = 3f;   // Buoyancy force multiplier
    public int floaters = 4;                // Number of floaters

    public float springForce = 10f;         // Spring strength for buoyancy
    public float dampingForce = 5f;         // Damping to reduce oscillations
    public float waterDrag = 0.99f;         // Linear drag
    public float waterAngularDrag = 0.5f;   // Angular drag

    public WaterSurface water;              // HDRP Water Surface

    private WaterSearchParameters searchParams;
    private WaterSearchResult searchResult;

    private void Start()
    {
        // If the water surface is not assigned, try to find it automatically
        if (water == null)
        {
            water = FindObjectOfType<WaterSurface>();

            if (water == null)
            {
                Debug.LogError("❌ No WaterSurface object found in the scene! Please add an HDRP Water Surface.");
                enabled = false; // Disable the script to prevent further errors
                return;
            }
            else
            {
                Debug.Log("🌊 WaterSurface automatically assigned: " + water.gameObject.name);
            }
        }
    }

    private void FixedUpdate()
    {
        if (water == null) return; // Stop if no water is assigned

        // Apply gravity divided by the number of floaters
        rb.AddForceAtPosition(Physics.gravity / floaters, transform.position, ForceMode.Acceleration);

        // Find the water surface height at the floater's position
        searchParams.startPositionWS = transform.position;
        water.ProjectPointOnWaterSurface(searchParams, out searchResult);

        float waterHeight = searchResult.projectedPositionWS.y;
        float floaterHeight = transform.position.y;

        // If the floater is below the water surface
        if (floaterHeight < waterHeight)
        {
            float submersionDepth = waterHeight - floaterHeight;
            float displacementMultiplier = Mathf.Clamp01(submersionDepth / depthBeforeSubmersion);

            // Apply buoyancy force with spring physics
            float buoyancyForce = displacementMultiplier * displacementAmount * Mathf.Abs(Physics.gravity.y);
            float spring = springForce * submersionDepth;
            float damping = dampingForce * rb.velocity.y;

            rb.AddForceAtPosition(new Vector3(0f, buoyancyForce + spring - damping, 0f), transform.position, ForceMode.Acceleration);

            // Apply linear drag to smooth motion
            rb.AddForce(displacementMultiplier * -rb.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rb.AddTorque(displacementMultiplier * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}
