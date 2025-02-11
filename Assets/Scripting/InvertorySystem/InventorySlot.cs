using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    public string slotTag;      // The tag this slot accepts (e.g., "StickI")
    public int maxItems = 10;   // Maximum number of items allowed in this slot

    public bool IsFull()
    {
        return transform.childCount >= maxItems;
    }

    public void AddItem(GameObject item)
    {
        if (!IsFull())
        {
            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Collider>().enabled = false;
        }
    }

    public GameObject RemoveItem()
    {
        if (transform.childCount > 0)
        {
            GameObject item = transform.GetChild(0).gameObject;
            item.transform.SetParent(null);
            item.GetComponent<Rigidbody>().isKinematic = false;
            item.GetComponent<Collider>().enabled = true;
            return item;
        }
        return null;
    }
}
