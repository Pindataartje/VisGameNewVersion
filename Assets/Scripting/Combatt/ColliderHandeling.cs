using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderHandeling : MonoBehaviour
{
    private WeaponHandler weaponHandler;

    private void Start()
    {
        // Get the WeaponHandler from the parent or another GameObject
        weaponHandler = GetComponentInParent<WeaponHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debugging to check what collider is triggering the weapon
        Debug.Log("Collider Triggered by: " + other.gameObject.name);

        // Check if the collider belongs to an enemy and it's not already in the hit list
        if (other.CompareTag("Enemy") && !weaponHandler.IsEnemyHit(other))
        {
            AnimalAI animalAI = other.GetComponent<AnimalAI>();
            if (animalAI != null)
            {
                animalAI.TakeDamage(10f);  // Example damage value
                Debug.Log("Damage applied to " + animalAI.gameObject.name);

                // Add the enemy to the hit list to prevent re-hitting until the cooldown
                weaponHandler.AddHitEnemy(other);
            }
        }
       
    }
}
