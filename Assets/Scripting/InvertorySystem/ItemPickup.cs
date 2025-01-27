using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string[] allowedTags; // Tags this script can pick up (e.g., "Stick", "Stone")

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered with: {other.name}"); // Debug message

        foreach (string tag in allowedTags)
        {
            if (other.CompareTag(tag))
            {
                InventorySlot slot = FindSlotForItem(tag);
                if (slot != null && !slot.IsFull())
                {
                    Debug.Log($"Adding {tag} to inventory."); // Debug message
                    slot.AddItem(other.gameObject);
                    other.gameObject.SetActive(false); // Disable the item after pickup
                }
                else
                {
                    Debug.Log($"{tag} slot is full!"); // Debug message
                }
                break;
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            foreach (string tag in allowedTags)
            {
                if (other.CompareTag(tag))
                {
                    InventorySlot slot = FindSlotForItem(tag);
                    if (slot != null && !slot.IsFull())
                    {
                        slot.AddItem(other.gameObject);
                        other.gameObject.SetActive(false); // Disable the item after pickup
                    }
                    else
                    {
                        Debug.Log($"{tag} slot is full!");
                    }
                    break;
                }
            }
        }
    }


    private InventorySlot FindSlotForItem(string tag)
    {
        InventorySlot[] slots = FindObjectsOfType<InventorySlot>();
        foreach (InventorySlot slot in slots)
        {
            if (slot.slotTag == tag && !slot.IsFull())
            {
                return slot;
            }
        }
        Debug.LogWarning($"No slot found for tag: {tag}"); // Debug message
        return null;
    }
}
