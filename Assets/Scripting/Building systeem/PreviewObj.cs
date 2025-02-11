using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewObj : MonoBehaviour
{
    public bool isFoundation;
    public List<Collider> colliders = new List<Collider>();
    public Material green;
    public Material red;
    public bool isBuildAble;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7 && isFoundation)
            colliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7 && isFoundation)
            colliders.Remove(other);
    }

    public void Update()
    {
        ChangeColor();
    }

    public void ChangeColor()
    {
        if (colliders.Count == 0)
            isBuildAble = true;
        else
            isBuildAble = false;

        if (isBuildAble)
        {
            foreach (Transform child in this.transform)
            {
                child.GetComponent<Renderer>().material = green;
            }
        }
        if (!isBuildAble)
        {
            foreach (Transform child in this.transform)
            {
                child.GetComponent<Renderer>().material = red;
            }
        }

    }
}
