using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BuildAid : MonoBehaviour
{
    [SerializeField]
    SnapPoint snapPoint;
    ObjectPool<SnapPoint> snapPointPool;
    List<SnapPoint> activeSnapPoints;
    void Awake()
    {
        snapPointPool = new(
            () => Instantiate(snapPoint),
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
        foreach (SnapPoint snapPoint in activeSnapPoints)
            snapPointPool.Release(snapPoint);
        activeSnapPoints.Clear();
        BuildTargets polled = BuildHandler.PollBuildTarget(InputSystem.MouseWorldPos);
        if (polled.Nodes != null)
            foreach (Node node in polled.Nodes)
            {
                SnapPoint snapPoint = snapPointPool.Get();
                snapPoint.transform.position = node.Pos;
                activeSnapPoints.Add(snapPoint);
            }
    }
}