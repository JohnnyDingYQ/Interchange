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
        BuildTargets polled = Build.PollBuildTarget(InputSystem.MouseWorldPos);
        if (polled.SnapNotNull)
            foreach (Node node in polled.Nodes)
            {
                SnapPoint snapPoint = snapPointPool.Get();
                float3 pos = node.Pos;
                pos.y = Constants.MaxElevation + 1;
                snapPoint.transform.position = pos;
                activeSnapPoints.Add(snapPoint);
            }
        BuildTargets startTarget = Build.GetStartTarget();
        if (startTarget != null && startTarget.SnapNotNull)
            foreach (Node node in startTarget.Nodes)
            {
                SnapPoint snapPoint = snapPointPool.Get();
                float3 pos = node.Pos;
                pos.y = Constants.MaxElevation + 1;
                snapPoint.transform.position = pos;
                activeSnapPoints.Add(snapPoint);
            }
    }
}