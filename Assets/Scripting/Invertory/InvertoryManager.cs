using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI; // Assign your inventory panel
    public Transform dropPosition; // Empty object in front of camera where dropped items spawn
    public TextMeshProUGUI itemInfoText; // UI text to show item count
    private ItemSlot hoveredSlot; // Track hovered slot

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
        // Raycast from cursor position to detect inventory items
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Cursor is over: {hit.collider.gameObject.name} (Tag: {hit.collider.gameObject.tag})");

            // First, check if the hit object has an ItemSlot
            hoveredSlot = hit.collider.GetComponent<ItemSlot>();

            // If not, check its parent (because items are stored inside empty slots)
            if (hoveredSlot == null && hit.collider.transform.parent != null)
            {
                hoveredSlot = hit.collider.transform.parent.GetComponent<ItemSlot>();
            }

            if (hoveredSlot != null)
            {
                itemInfoText.text = $"{hoveredSlot.itemTag}: {hoveredSlot.GetItemCount()}";
                Debug.Log($"✅ Detected Inventory Item: {hoveredSlot.itemTag} - Count: {hoveredSlot.GetItemCount()}");
            }
            else
            {
                itemInfoText.text = ""; // Clear text if not hovering an item
                Debug.Log("❌ Cursor is over something, but it's NOT an inventory item.");
            }
        }
        else
        {
            hoveredSlot = null;
            itemInfoText.text = "";
            Debug.Log("❌ Cursor is not over any object.");
        }
    }


    void TryDropHoveredItem()
    {
        if (hoveredSlot != null)
        {
            Debug.Log($"Dropping item from slot: {hoveredSlot.itemTag}");
            DropItem(hoveredSlot);
        }
        else
        {
            Debug.Log("No inventory item detected when trying to drop.");
        }
    }

    void DropItem(ItemSlot slot)
    {
        if (slot != null)
        {
            GameObject droppedItem = slot.RemoveItem();
            if (droppedItem)
            {
                droppedItem.SetActive(true);
                droppedItem.transform.position = dropPosition.position;
                droppedItem.transform.SetParent(null);

                // Re-enable physics and floater for dropped item
                if (droppedItem.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = false;
                    rb.detectCollisions = true;
                }

                if (droppedItem.TryGetComponent(out Floater floater))
                {
                    floater.enabled = true;
                }

                if (droppedItem.TryGetComponent(out Collider col))
                {
                    col.enabled = true;
                }

                Debug.Log($"Dropped: {slot.itemTag}");
            }
            else
            {
                Debug.Log("Tried to drop an item, but inventory slot is empty.");
            }
        }
    }
}
