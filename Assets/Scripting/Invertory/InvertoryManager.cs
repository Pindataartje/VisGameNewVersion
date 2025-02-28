using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI; // Assign your inventory panel
    public Transform dropPosition; // Empty object in front of camera where dropped items spawn
    public TextMeshProUGUI itemInfoText; // UI text to show item count
    private ItemSlot hoveredSlot; // Track hovered slot

    [Header("Equipment Settings")]
    public Transform handPoint; // Assign this to the HandPoint in the player
    private GameObject equippedItem = null; // Stores the currently equipped item
    private ItemSlot equippedSlot = null; // Tracks which slot the item came from

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        if (inventoryUI.activeSelf)
        {
            DetectHoveredItem();

            if (Input.GetMouseButtonDown(1)) // Right Click to drop
            {
                TryDropHoveredItem();
            }

            if (Input.GetMouseButtonDown(0)) // Left Click to equip
            {
                TryEquipHoveredItem();
            }
        }

        if (Input.GetKeyDown(KeyCode.X)) // Unequip item
        {
            UnequipItem();
        }

        if (Input.GetKeyDown(KeyCode.Q)) // Drop equipped item
        {
            DropEquippedItem();
        }
    }

    void ToggleInventory()
    {
        inventoryUI.SetActive(!inventoryUI.activeSelf);
        Cursor.lockState = inventoryUI.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryUI.activeSelf;
    }

    void DetectHoveredItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            ItemSlot newHoveredSlot = hit.collider.GetComponent<ItemSlot>();

            if (newHoveredSlot == null && hit.collider.transform.parent != null)
                newHoveredSlot = hit.collider.transform.parent.GetComponent<ItemSlot>();

            if (newHoveredSlot != hoveredSlot)
            {
                hoveredSlot = newHoveredSlot;
            }

            UpdateItemText();
        }
        else if (hoveredSlot != null)
        {
            hoveredSlot = null;
            itemInfoText.text = "";
        }
    }

    void TryEquipHoveredItem()
    {
        if (hoveredSlot != null && hoveredSlot.GetItemCount() > 0 && equippedItem == null)
        {
            GameObject itemToEquip = hoveredSlot.RemoveItem();
            if (itemToEquip)
            {
                equippedItem = itemToEquip;
                equippedSlot = hoveredSlot;

                // Set item in hand
                itemToEquip.transform.SetParent(handPoint);
                itemToEquip.transform.localPosition = Vector3.zero;
                itemToEquip.transform.localRotation = Quaternion.identity;
                itemToEquip.SetActive(true);

                // Disable physics
                if (itemToEquip.TryGetComponent(out Rigidbody rb))
                    rb.isKinematic = true;

                if (itemToEquip.TryGetComponent(out Collider col))
                    col.enabled = false;

                Debug.Log($"✅ Equipped: {equippedSlot.itemTag}");
                UpdateItemText();
            }
        }
    }

    void UnequipItem()
    {
        if (equippedItem != null && equippedSlot != null)
        {
            // Check if inventory has space
            if (!equippedSlot.IsFull())
            {
                equippedSlot.StoreItem(equippedItem);
                equippedItem = null;
                equippedSlot = null;
                Debug.Log("✅ Unequipped item back into inventory.");
            }
            else
            {
                Debug.Log("❌ Inventory full, dropping unequipped item.");
                DropEquippedItem();
            }

            UpdateItemText();
        }
    }

    void DropEquippedItem()
    {
        if (equippedItem != null)
        {
            equippedItem.SetActive(true);
            equippedItem.transform.position = dropPosition.position;
            equippedItem.transform.SetParent(null);

            // Re-enable physics
            if (equippedItem.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }

            if (equippedItem.TryGetComponent(out Collider col))
            {
                col.enabled = true;
            }

            Debug.Log($"✅ Dropped equipped item: {equippedItem.name}");
            equippedItem = null;
            equippedSlot = null;

            UpdateItemText();
        }
    }

    void TryDropHoveredItem()
    {
        if (hoveredSlot != null)
        {
            DropItem(hoveredSlot);
        }
        else
        {
            Debug.Log("❌ No inventory item detected when trying to drop.");
        }
    }

    void DropItem(ItemSlot slot)
    {
        if (slot == null || slot.GetItemCount() <= 0)
        {
            Debug.Log("❌ Tried to drop an item, but inventory slot is empty.");
            return;
        }

        GameObject droppedItem = slot.RemoveItem();
        if (droppedItem)
        {
            droppedItem.SetActive(true);
            droppedItem.transform.position = dropPosition.position;
            droppedItem.transform.SetParent(null);

            if (droppedItem.TryGetComponent(out Rigidbody rb))
                rb.isKinematic = false;

            if (droppedItem.TryGetComponent(out Collider col))
                col.enabled = true;

            Debug.Log($"✅ Dropped: {slot.itemTag}");

            UpdateItemText();
        }
        else
        {
            Debug.Log("❌ Tried to drop an item, but no object was returned.");
        }
    }

    void UpdateItemText()
    {
        if (hoveredSlot != null)
        {
            int count = hoveredSlot.GetItemCount();
            itemInfoText.text = count > 0 ? $"{hoveredSlot.itemTag}: {count}" : "";
        }
        else
        {
            itemInfoText.text = "";
        }
    }
}
