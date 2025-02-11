using UnityEngine;
using TMPro; // Add the TextMeshPro namespace

public class InventoryHover : MonoBehaviour
{
    public Camera inventoryCamera; // Assign the inventory camera
    public TextMeshProUGUI hoverText; // Assign the TextMeshProUGUI element

    void Update()
    {
        Ray ray = inventoryCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            InventorySlot slot = hit.transform.GetComponent<InventorySlot>();
            if (slot != null)
            {
                hoverText.text = $"{slot.slotTag}: {slot.transform.childCount}/{slot.maxItems}";
                hoverText.gameObject.SetActive(true);
            }
            else
            {
                hoverText.gameObject.SetActive(false);
            }
        }
        else
        {
            hoverText.gameObject.SetActive(false);
        }
    }
}
