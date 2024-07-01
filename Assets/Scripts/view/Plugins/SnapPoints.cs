using System.Collections.Generic;
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
            if (endTarget.Snapped)
                foreach (Node node in endTarget.Nodes)
                {
                    SnapPoint snapPoint = snapPointPool.Get();
                    float3 pos = node.Pos;
                    pos.y = Constants.MaxElevation + 1;
                    snapPoint.transform.position = pos;
                    activeSnapPoints.Add(snapPoint);
                }
            else if (endTarget.DividePossible)
                foreach (float3 pos in endTarget.NodesIfDivded)
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
            if (startTarget.Snapped)
                foreach (Node node in startTarget.Nodes)
                {
                    SnapPoint snapPoint = snapPointPool.Get();
                    float3 pos = node.Pos;
                    pos.y = Constants.MaxElevation + 1;
                    snapPoint.transform.position = pos;
                    activeSnapPoints.Add(snapPoint);
                }
            else if (startTarget.DividePossible)
                foreach (float3 pos in startTarget.NodesIfDivded)
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