using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    public KeyCode pickupKey = KeyCode.E; // Pickup key
    public KeyCode unequipKey = KeyCode.X; // Unequip key
    public KeyCode dropKey = KeyCode.Q; // Drop equipped item key

    public float pickupRange = 2f;
    public Transform playerCamera;
    public Transform bag; // Assign the bag object in the Inspector
    public Transform dropPosition; // Assign the drop position in front of the player
    public Transform handPoint; // Assign the HandPoint where equipped items go

    void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            TryPickupItem();
        }

        if (Input.GetKeyDown(unequipKey))
        {
            UnequipItem();
        }

        if (Input.GetKeyDown(dropKey))
        {
            DropEquippedItem();
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

    public void EquipItem(GameObject item)
    {
        // ✅ Allow equipping ONLY if handPoint is empty
        if (handPoint.childCount > 0)
        {
            Debug.Log("❌ Cannot equip - Already holding an item!");
            return;
        }

        // Move item to handPoint
        item.transform.SetParent(handPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.SetActive(true);

        // Disable physics
        if (item.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        if (item.TryGetComponent(out Collider col))
            col.enabled = false;

        Debug.Log($"✅ Equipped: {item.name}");
    }

    void UnequipItem()
    {
        if (handPoint.childCount > 0) // ✅ Check if an item is equipped
        {
            GameObject item = handPoint.GetChild(0).gameObject;
            string itemTag = item.tag;
            ItemSlot correctSlot = FindCorrectSlot(itemTag);

            if (correctSlot != null && !correctSlot.IsFull())
            {
                correctSlot.StoreItem(item);
                Debug.Log($"✅ Unequipped {item.name} back into inventory.");
            }
            else
            {
                Debug.Log("❌ Inventory full, dropping unequipped item.");
                DropEquippedItem();
            }
        }
    }

    void DropEquippedItem()
    {
        if (handPoint.childCount > 0) // ✅ Check if an item is equipped
        {
            GameObject item = handPoint.GetChild(0).gameObject;
            item.SetActive(true);
            item.transform.position = dropPosition.position;
            item.transform.SetParent(null);

            // Re-enable physics
            if (item.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }

            if (item.TryGetComponent(out Collider col))
            {
                col.enabled = true;
            }

            Debug.Log($"✅ Dropped: {item.name}");
        }
    }

    ItemSlot FindCorrectSlot(string itemTag)
    {
        foreach (ItemSlot slot in bag.GetComponentsInChildren<ItemSlot>())
        {
            if (slot.itemTag == itemTag)
            {
                return slot;
            }
        }
        return null;
    }
}
