using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class NewBuildSysteem : MonoBehaviour
{
 public List<buildObjects> objects = new List<buildObjects>();
    public buildObjects currentOb;
    private Vector3 currentPos;
    public Transform currentPreview;
    public Transform cam;
    public RaycastHit hit;
    public LayerMask layer;

    public float offset = 1.0f;
    public float gridsize = 1.0f;
    public bool isBuilding;

    public void Start()
    {
        currentOb = objects[0];
        ChangeBuildable();

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Update()
    {
        if (isBuilding)
        {
            StartPreview();
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Build();
        }
    }

    public void ChangeBuildable()
    {
        GameObject curprev = Instantiate(currentOb.preview,currentPos  , Quaternion.identity) as GameObject;
        currentPreview = curprev.transform;
    }

    public void StartPreview()
    {
        if (Physics.Raycast(cam.position, cam.forward, out hit, 10f, layer))
        {
            if (hit.transform != this.transform)
            {
                ShowPreview(hit);
            }
        }
    }

    public void ShowPreview(RaycastHit hit2)
    {
        currentPos = hit2.point;
        currentPos -= Vector3.one * offset;
        currentPos /= gridsize;
        currentPos = new Vector3(Mathf.Round(currentPos.x), Mathf.Round(currentPos.y), Mathf.Round(currentPos.z));
        currentPos *= gridsize;
        currentPos += Vector3.one * offset;
        currentPreview.position = currentPos;
    }

    public void Build()
    {
        PreviewObj po = currentPreview.GetComponent<PreviewObj>();
        if (po.isBuildAble)
        {
            Instantiate(currentOb.prefab, currentPos, quaternion.identity);
        }

    }
}  
    
 [System.Serializable]
 public class buildObjects
 {
        public string name;
    public GameObject prefab;
        public GameObject preview;
        public int wood;

 }

