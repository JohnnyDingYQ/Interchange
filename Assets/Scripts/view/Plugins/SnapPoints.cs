using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

public class SnapPoints : MonoBehaviour
{
    [SerializeField]
    SnapPoint snapPoint;
    ObjectPool<SnapPoint> snapPointPool;
    List<SnapPoint> activeSnapPoints;
    void Awake()
    {
        snapPointPool = new(
            () => Instantiate(snapPoint, gameObject.transform),
            (o) => o.gameObject.SetActive(true),
            (o) => o.gameObject.SetActive(false),
            (o) => Destroy(o.gameObject),
            false,
            6,
            10
        );
        activeSnapPoints = new();
    }
    void FixedUpdate()
    {
        UpdateSnapPoints();
    }

    void UpdateSnapPoints()
    {
        foreach (SnapPoint snapPoint in activeSnapPoints)
            snapPointPool.Release(snapPoint);
        activeSnapPoints.Clear();
        BuildTargets endTarget = Build.EndTarget;
        if (endTarget != null)
        {
            IEnumerable<float3> nodePos = null;
            if (endTarget.Snapped)
                nodePos = endTarget.Nodes.Select(n => n.Pos);
            else if (endTarget.DivideIsPossible)
                nodePos = endTarget.NodesPosIfDivded;
            if (nodePos != null)
                foreach (float3 pos in nodePos)
                {
                    SnapPoint snapPoint = snapPointPool.Get();
                    float3 posCopy = pos;
                    posCopy.y = Constants.MaxElevation + 1;
                    snapPoint.transform.position = pos;
                    activeSnapPoints.Add(snapPoint);
                }
        }
        BuildTargets startTarget = Build.StartTarget;
        if (startTarget != null)
        {
            IEnumerable<float3> nodePos = null;
            if (startTarget.Snapped)
                nodePos = startTarget.Nodes.Select(n => n.Pos);
            else if (startTarget.DivideIsPossible)
                nodePos = startTarget.NodesPosIfDivded;
            if (nodePos != null)
                foreach (float3 pos in nodePos)
                {
                    SnapPoint snapPoint = snapPointPool.Get();
                    float3 posCopy = pos;
                    posCopy.y = Constants.MaxElevation + 1;
                    snapPoint.transform.position = pos;
                    activeSnapPoints.Add(snapPoint);
                }
        }
    }
}