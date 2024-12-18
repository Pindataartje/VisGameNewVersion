using UnityEngine;

public class ConstrainCameraRotation : MonoBehaviour
{
    public Transform boat; // Reference to the boat
    public Vector3 offset = new Vector3(0, 5, -10); // Offset relative to the boat (default: above and behind)

    private void LateUpdate()
    {
        if (boat == null) return;

        // Get the boat's position and rotation
        Vector3 targetPosition = boat.position + Quaternion.Euler(0, boat.eulerAngles.y, 0) * offset;

        // Update the camera position
        transform.position = targetPosition;

        // Make the camera look at the boat's center (adjust height if needed)
        transform.LookAt(boat.position + Vector3.up * 2f); // Adjust height as necessary
    }
}
