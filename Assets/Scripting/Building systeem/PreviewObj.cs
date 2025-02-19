using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewObj : MonoBehaviour
{
    public List<Collider> colliders = new List<Collider>();
    public objectSoorts sort;
    public Material green;
    public Material red;
    public bool isBuildAble;

    public bool second;
    public PreviewObj childcol; // Only used in preview, not in actual object
    public Transform graphics;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
            colliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7)
            colliders.Remove(other);
    }

    public void Update()
    {
        if (!second)
        {
            ChangeColor();
        }
    }

    public void ChangeColor()
    {
        if (sort == objectSoorts.Foundation)
        {
            // Foundations can be placed if there's no collider
            isBuildAble = (colliders.Count == 0);
        }
        else
        {
            //  Only check childcol if it's a preview object (Fix)
            if (childcol != null)
            {
                isBuildAble = (colliders.Count == 0 && childcol.colliders.Count > 0);
            }
            else
            {
                //  If no childcol (e.g., actual build object), just check colliders
                isBuildAble = (colliders.Count == 0);
            }
        }

        // Change material color based on buildability
        foreach (Transform child in graphics)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = isBuildAble ? green : red;
            }
        }
    }
}


public enum objectSoorts
{
    normal,
    Foundation,
    Floor
}
