using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Build Objects")]
    [SerializeField] private List<GameObject> floorobjects = new List<GameObject>();
    [SerializeField] private List<GameObject> wallobjectts = new List<GameObject>();

    [Header("Build Settings")]
    [SerializeField] private SelectedBuildType currentBuildType;
    [SerializeField] private LayerMask connectedLayer;

    [Header("Ghost Settings")]
    [SerializeField] private Material ghostMaterialvalid;
    [SerializeField] private Material ghostMaterialInvalid;
    [SerializeField] private float connectorOverlapRadius = 1f;
    [SerializeField] private float maxgroundAngle = 45f;

    [Header("Internal State")]
    [SerializeField] private bool isbuilding = false;
    [SerializeField] private int currentBuildingIndex;
    private GameObject ghostbuildObject;
    private bool isGhostInValidPosistion = false;
    private Transform modelParent = null;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isbuilding = !isbuilding;

            // Destroy ghost object ONLY when exiting build mode
            if (!isbuilding && ghostbuildObject)
            {
                Destroy(ghostbuildObject);
                ghostbuildObject = null;
            }
        }

        if (isbuilding)
        {
            GhostBuild();

            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuild();
            }
        }
    }



    private void GhostBuild()
    {
        GameObject currentBuild = GetCurrentBuild();
        CreateGhostPrefab(currentBuild);

        MoveGhostToRaycast();
        checkBuildVadility();
    }


    private void CreateGhostPrefab(GameObject currentbuild)
    {
        if (ghostbuildObject == null)
        {
            ghostbuildObject = Instantiate(currentbuild);
            modelParent = ghostbuildObject.transform.GetChild(0);

            ghostifyModel(modelParent, ghostMaterialvalid);
            ghostifyModel(ghostbuildObject.transform);

        }
    }

    private void MoveGhostToRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            ghostbuildObject.transform.position = hit.point;
        }
    }


    private void checkBuildVadility()
    {
        Collider[] colliders = Physics.OverlapSphere(ghostbuildObject.transform.position, connectorOverlapRadius, connectedLayer);
        if (colliders.Length > 0)
        {
            ghostConnectedBuild(colliders); // Pass colliders array
        }
        else
        {
            ghostSeperateBuild();
        }
    }

    private void ghostConnectedBuild(Collider[] colliders)
    {
        Connector bestConnector = null;

        foreach (Collider collider in colliders)
        {
            Connector connector = collider.GetComponent<Connector>();

            if (connector.canConnectTo)
            {
                bestConnector = connector;
                break;
            }
        }

        if (bestConnector == null || currentBuildType == SelectedBuildType.Floor && bestConnector.isConnectedToFloor || currentBuildType == SelectedBuildType.Wall && bestConnector.isConnectedToWall)
        {
            ghostifyModel(modelParent, ghostMaterialvalid);
            isGhostInValidPosistion = false;
            return;
        }


        snapGhostPrefabToConnector(bestConnector);
    }




    private void snapGhostPrefabToConnector(Connector connector)
    {
        Transform ghostConnector = findSnapConnector(connector.transform, ghostbuildObject.transform.GetChild(1));
        ghostbuildObject.transform.position = connector.transform.position - (ghostConnector.position - ghostbuildObject.transform.position);

        if (currentBuildType == SelectedBuildType.Wall)
        {
            Quaternion newRotation = ghostbuildObject.transform.rotation;
            newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, connector.transform.rotation.eulerAngles.y, newRotation.eulerAngles.z);
            ghostbuildObject.transform.rotation = newRotation;
        }


        ghostifyModel(modelParent, ghostMaterialvalid);
        isGhostInValidPosistion = true;
    }

    private void ghostSeperateBuild()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (currentBuildType == SelectedBuildType.Wall)
            {
                ghostifyModel(modelParent, ghostMaterialInvalid);
                isGhostInValidPosistion = false;
                return;
            }

            if (hit.collider.transform.root.CompareTag("Buildables"))
            {
                ghostifyModel(modelParent, ghostMaterialInvalid);
                isGhostInValidPosistion = false;
                return;
            }
            if (Vector3.Angle(hit.normal, Vector3.up) < maxgroundAngle)
            {
                ghostifyModel(modelParent, ghostMaterialvalid);
                isGhostInValidPosistion = true;
            }
            else
            {
                ghostifyModel(modelParent, ghostMaterialInvalid);
                isGhostInValidPosistion = false;
            }
        }
    }


    private Transform findSnapConnector(Transform snapConnector, Transform ghostConnectorParent)
    {
        ConnectorPositions OppisiteConnectorTag = getOppisitePosition(snapConnector.GetComponent<Connector>());

        foreach (Connector connector in ghostConnectorParent.GetComponentsInChildren<Connector>())
        {
            if (connector.connectorPosition == OppisiteConnectorTag)
            {
                return connector.transform;
            }
        }
        return null;
    }

    private ConnectorPositions getOppisitePosition(Connector connector)
    {
        ConnectorPositions positions = connector.connectorPosition;

        if (currentBuildType == SelectedBuildType.Wall && connector.connectorParent == SelectedBuildType.Floor)
        {
            return ConnectorPositions.bottom;
        }

        if (currentBuildType == SelectedBuildType.Floor
            && connector.connectorParent == SelectedBuildType.Wall
            && connector.connectorPosition == ConnectorPositions.top) // Fixed line
        {
            if (connector.transform.root.rotation.y == 0)
            {
                return getConnectorClosetsToPlayer(true);
            }
            else
            {
                return getConnectorClosetsToPlayer(false);
            }
        }

        switch (positions)
        {
            case ConnectorPositions.left:
                return ConnectorPositions.right;
            case ConnectorPositions.right:
                return ConnectorPositions.left;
            case ConnectorPositions.top:
                return ConnectorPositions.bottom;
            case ConnectorPositions.bottom:
                return ConnectorPositions.top;
            default:
                return ConnectorPositions.bottom;
        }
    }

    private ConnectorPositions getConnectorClosetsToPlayer(bool topBottom)
    {
        Transform cameratransfomr = Camera.main.transform;

        if (topBottom)
        {
            return cameratransfomr.position.z >= ghostbuildObject.transform.position.z ? ConnectorPositions.bottom : ConnectorPositions.top;
        }
        else
        {
            return cameratransfomr.position.x >= ghostbuildObject.transform.position.x ? ConnectorPositions.left : ConnectorPositions.right;
        }

    }
    private void ghostifyModel(Transform modelParent, Material ghostMaterial = null)
    {
        if (ghostMaterial != null)
        {
            foreach (MeshRenderer meshRenderer in modelParent.GetComponentsInChildren<MeshRenderer>()) // Fixed GetComponentInChildren  GetComponentsInChildren
            {
                meshRenderer.material = ghostMaterial;
            }
        }
        foreach (Collider modelcollider in modelParent.GetComponentsInChildren<Collider>()) // Fixed GetComponentInChildren  GetComponentsInChildren
        {
            modelcollider.enabled = false;
        }
    }


    private GameObject GetCurrentBuild()
    {
        switch (currentBuildType)
        {
            case SelectedBuildType.Floor:
                return floorobjects[currentBuildingIndex];
            case SelectedBuildType.Wall:
                return wallobjectts[currentBuildingIndex];
        }

        return null;
    }

    private void PlaceBuild()
    {
        if (ghostbuildObject != null && isGhostInValidPosistion)
        {
            GameObject newBuild = Instantiate(GetCurrentBuild(), ghostbuildObject.transform.position, ghostbuildObject.transform.rotation);

            Destroy(ghostbuildObject);
            ghostbuildObject = null;

            isbuilding = false;

            foreach (Connector connector in newBuild.GetComponentsInChildren<Connector>())
            {
                connector.UpdateConnectors(true);
            }

        }
    }
}
[System.Serializable]
public enum SelectedBuildType
{
    Floor,
    Wall
}