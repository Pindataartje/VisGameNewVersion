using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    public KeyCode pickupKey = KeyCode.E; // Changeable in Inspector
    public float pickupRange = 2f;
    public Transform playerCamera;
    public Transform bag; // Assign the bag object in the Inspector

    void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            TryPickupItem();
        }
    }

    void TryPickupItem()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            GameObject item = hit.collider.gameObject;
            string itemTag = item.tag;

            // Find the correct item slot
            foreach (ItemSlot slot in bag.GetComponentsInChildren<ItemSlot>())
            {
                if (slot.itemTag == itemTag)
                {
                    if (slot.StoreItem(item))
                    {
                        Debug.Log($"Picked up: {itemTag}");
                    }
                    else
                    {
                        Debug.Log("Inventory Full!");
                    }
                    return;
                }
            }
        }
    }
}
