using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine;

public class BuildSystem : MonoBehaviour
{
    public Vector3 place;
    private RaycastHit hit;
    private RaycastHit deHit;

    public GameObject tempObject;
    public List<GameObject> buildableObjects; // List of objects to build
    public List<GameObject> tempObjects; // List of temporary objects corresponding to buildable objects

    public int selectedObjectIndex = 0; // Current selected object index

    public bool placeNow;
    public bool tempObjectExist;

    public int range;
    public float distance;
    public bool objectInRange;
    public GameObject player;

    public bool inBuildMode;
    public GameObject buildCanvas;

    public float gridSize = 0.1f; // Size of the snapping grid
    private List<GameObject> placedObjects = new List<GameObject>(); // List of placed objects

    public bool inDestructionMode;

    void Start()
    {
        inBuildMode = false;
        inDestructionMode = false;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Z))
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        {
            
        }
        if (placeNow && !inDestructionMode) // Prevent raycasting when in destruction mode
        {
            HandleRaycast();
        }
        else if (inDestructionMode)
        {
            destructray();
        }

        HandleInput();
    }

    private void HandleInput()
    {
        // Block placing objects when in destruction mode
        if (Input.GetKeyDown(KeyCode.B) && !inDestructionMode)
        {
            PlaceSelectedObject();
        }

        if (Input.GetKeyDown(KeyCode.E) && placeNow && inBuildMode && !inDestructionMode)
        {
            RotateObject(10f);
        }
        if (Input.GetKeyDown(KeyCode.Q) && placeNow && inBuildMode && !inDestructionMode)
        {
            RotateObject(-10f);
        }

        if (distance < range)
        {
            objectInRange = true;
        }
        else
        {
            ResetBuild();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ToggleBuildMode();
        }
    }

    private void HandleRaycast()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            // Calculate the snapped position, considering both grid and nearby placed objects
            Vector3 rawPosition = hit.point;
            place = GetSnappedPosition(rawPosition);
            distance = Vector3.Distance(place, player.transform.position);

            if (hit.transform.CompareTag("buildingPlace") && objectInRange && inBuildMode && !inDestructionMode)
            {
                if (!tempObjectExist)
                {
                    CreateTempObject();
                }
                else
                {
                    UpdateTempObjectPosition();
                }
            }

            if (Input.GetMouseButtonDown(0) && !inDestructionMode)
            {
                TryPlaceObject();
            }

            if (Input.GetMouseButtonDown(1))
            {
                ResetBuild();
            }
        }
    }

    private void CreateTempObject()
    {
        tempObject = Instantiate(tempObjects[selectedObjectIndex], place, Quaternion.identity);
        tempObjectExist = true;
    }

    private void UpdateTempObjectPosition()
    {
        tempObject.transform.position = place;
    }

    private void TryPlaceObject()
    {
        if (hit.transform.CompareTag("buildingPlace"))
        {
            // Place the object
            GameObject placedObject = Instantiate(buildableObjects[selectedObjectIndex], place, tempObject.transform.rotation);
            placedObjects.Add(placedObject);

            ResetBuild();
        }
        else
        {
            ResetBuild();
        }
    }

    private void RotateObject(float rotationAngle)
    {
        if (tempObject != null)
        {
            tempObject.transform.Rotate(0f, rotationAngle, 0f, Space.World);
        }
    }

    private Vector3 GetSnappedPosition(Vector3 rawPosition)
    {
        float x = Mathf.Round(rawPosition.x / gridSize) * gridSize;
        float y = rawPosition.y; // Keep the y-axis unchanged for ground-level placement
        float z = Mathf.Round(rawPosition.z / gridSize) * gridSize;

        Vector3 snappedPosition = SnapToPlacedObjects(new Vector3(x, y, z));
        return snappedPosition != Vector3.zero ? snappedPosition : new Vector3(x, y, z);
    }

    private Vector3 SnapToPlacedObjects(Vector3 position)
    {
        Vector3 closestPosition = Vector3.zero;
        float minDistance = Mathf.Infinity;

        foreach (GameObject placedObject in placedObjects)
        {
            float currentDistance = Vector3.Distance(position, placedObject.transform.position);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                closestPosition = placedObject.transform.position;
            }
        }

        return minDistance <= gridSize ? closestPosition : Vector3.zero;
    }

    public void PlaceSelectedObject()
    {
        placeNow = true;
    }

    public void SelectBuildableObject(int index)
    {
        if (index >= 0 && index < buildableObjects.Count && index < tempObjects.Count)
        {
            selectedObjectIndex = index;

            // Update tempObject if one is active
            if (tempObjectExist)
            {
                Destroy(tempObject);
                CreateTempObject();
            }
        }
    }

    private void ToggleBuildMode()
    {
        if (inBuildMode)
        {
            ResetBuild();
            buildCanvas.SetActive(false);
            inBuildMode = false;
        }
        else
        {
            inBuildMode = true;
            buildCanvas.SetActive(true);
        }
    }

    private void ResetBuild()
    {
        placeNow = false;
        if (tempObject != null)
        {
            Destroy(tempObject);
            tempObjectExist = false;
        }
        objectInRange = false;
        distance = 0;
    }

    public void changeDestructMode()
    {
        inDestructionMode = !inDestructionMode; // Toggle destruction mode
    }

    public void destructray()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out deHit))
        {
            if (hit.transform.tag == "buildingitem")
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Destroy(deHit.collider.gameObject);
                }
            }
        }
    }
}