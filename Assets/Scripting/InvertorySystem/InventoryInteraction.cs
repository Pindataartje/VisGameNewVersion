using UnityEngine;

public class InventoryInteraction : MonoBehaviour
{
    public Transform handSlot; // Reference to the player's hand (for equipping items)

    public void EquipItem(InventorySlot slot)
    {
        if (slot.transform.childCount > 0)
        {
            GameObject item = slot.RemoveItem();
            item.transform.SetParent(handSlot);
            item.transform.localPosition = Vector3.zero;
            item.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    public void DropItem(InventorySlot slot)
    {
        if (slot.transform.childCount > 0)
        {
            GameObject item = slot.RemoveItem();
            item.transform.position = transform.position + transform.forward; // Drop in front of the player
        }
    }
}
