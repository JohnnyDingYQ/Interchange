using System;
using Unity.Mathematics;
using UnityEngine;

public class Snapper : MonoBehaviour
{
    private const float SnapDistance = 5;
    private GameObject snapPoint;
    public static int SnappedNode {get; set;}
    [SerializeField] private GameObject snapPointPrefab;

    void Start()
    {
        snapPoint = Instantiate(snapPointPrefab);
    }
    void Update()
    {
        SnappedNode = SnapToLaneNodes(Main.MouseWorldPos, SnapDistance);
        if (SnappedNode != 1)
        {
            snapPoint.GetComponent<Renderer>().enabled = true;
            snapPoint.transform.position = Grid.GetWorldPosByID(SnappedNode);
            snapPoint.transform.position = new Vector3(snapPoint.transform.position.x, Grid.Level + 0.1f, snapPoint.transform.position.z);
        }
        else
        {
            snapPoint.GetComponent<Renderer>().enabled = false;
        }

    }
    public int SnapToLaneNodes(float3 worldPos, float snapDistance)
    {
        float minDistance = float.MaxValue;
        int closest = -1;
        foreach (Road road in BuildManager.RoadWatcher.Values)
        {
            foreach (Lane lane in road.Lanes)
            {
                float distance = Vector3.Distance(Grid.GetWorldPosByID(lane.Start), worldPos);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    closest = lane.Start;
                }

                distance = Vector3.Distance(Grid.GetWorldPosByID(lane.End), worldPos);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    closest = lane.End;
                }
            }
        }
        if (minDistance > snapDistance)
        {
            return -1;
        }
        return closest;
    }


    public static bool Snapped()
    {
        return SnappedNode != -1;
    }
}