using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public ConnectorPositions connectorPosition;
    public SelectedBuildType connectorParent;

    [HideInInspector] public bool isConnectedToFloor = false;
    [HideInInspector] public bool isConnectedToWall = false;
    [HideInInspector] public bool canConnectTo = true;

    [SerializeField] private bool canConnectToFloor = true;
    [SerializeField] private bool canConnectToWall = true;

    private void OnDrawGizmos()
    {
        Gizmos.color = isConnectedToFloor ? (isConnectedToWall ? Color.red : Color.blue) : (!isConnectedToWall ? Color.green : Color.yellow);
        Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x / 2f);
    }
    public void UpdateConnectors(bool rootcall = false)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.lossyScale.x / 2f);

        isConnectedToFloor = !canConnectToFloor;
        isConnectedToWall = !canConnectToWall;
        foreach (Collider collider in colliders)
        {
            if (collider.GetInstanceID() == GetComponent<Collider>().GetInstanceID())
            {
                continue;
            }
            if (collider.gameObject.layer == gameObject.layer)
            {
                Connector foundconnector = collider.GetComponent<Connector>();
                if (foundconnector.connectorParent == SelectedBuildType.Floor)
                {
                    isConnectedToFloor = true;
                }
                if (foundconnector.connectorParent == SelectedBuildType.Wall)
                {
                    isConnectedToWall = true;
                }

                if (rootcall)
                {
                    foundconnector.UpdateConnectors();
                }
            }
        }

        canConnectTo = true;

        if (isConnectedToFloor && isConnectedToWall)
        {
            canConnectTo = false;
        }
    }
}
[System.Serializable]
public enum ConnectorPositions
{
    left,
     right,
     top,
     bottom
}
