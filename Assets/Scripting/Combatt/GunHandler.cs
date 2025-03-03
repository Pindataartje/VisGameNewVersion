using UnityEngine;

public class GunHandler : MonoBehaviour
{
    [Header("Gun Objects")]
    public GameObject flareGun;

    // Define the types of guns supported.
    public enum GunType { Crossbow, FlareGun }
    public GunType gunType = GunType.Crossbow;

   

    [Header("Gun Settings")]
    [Tooltip("Amount of damage this gun deals")]
    public float damage = 0f;

    [Header("Sound Settings")]
    public AudioClip crossbowSound;

    [Header("Animation")]
    private Animator animController; // Reference to the Animator component

    [Header("Spawn Settings")]
    [Tooltip("Transform that marks the spawn point for the projectile")]
    public Transform bulletSpawnPoint;  // Transform for the spawn location

    void Start()
    {
        // Get the Animator component attached to this GameObject
        animController = GetComponent<Animator>();

        if (animController == null)
        {
            Debug.LogWarning("No Animator found on " + gameObject.name);
        }

        if (bulletSpawnPoint == null)
        {
            Debug.LogWarning("Bullet spawn point not assigned!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    // Call this method to trigger a shooting action and instantiate the projectile at the specified spawn point.
    public void Shoot()
    {
        AudioSource.PlayClipAtPoint(crossbowSound, transform.position);

        if (animController != null)
        {
            animController.SetTrigger("Shoot");
        }

       
        // Ensure the bulletSpawnPoint is assigned and use it for the spawn position
        
    }
}
