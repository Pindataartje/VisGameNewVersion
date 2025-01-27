using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryCamera; // Assign the inventory camera in the Inspector
    public GameObject playerCamera;    // Assign the main camera in the Inspector
    public GameObject inventoryUI;     // The 3D inventory model (e.g., bag or table)

    private bool isInventoryOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryCamera.SetActive(isInventoryOpen);
        playerCamera.SetActive(!isInventoryOpen);
        inventoryUI.SetActive(isInventoryOpen);

        // Optionally lock/unlock the cursor
        Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isInventoryOpen;
    }
}
