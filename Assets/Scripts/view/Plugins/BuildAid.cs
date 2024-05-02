using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BuildAid : MonoBehaviour
{
    [SerializeField]
    SnapPoint snapPoint;
    ObjectPool<SnapPoint> snapPointPool;
    List<SnapPoint> activeSnapPoints;
    [SerializeField]
    RoadGameObject roadPrefab;
    static RoadGameObject ghostroad;
    public static bool GhostIsOn { get; set; }
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
        ghostroad = Instantiate(roadPrefab);
        activeSnapPoints = new();
    }
    void FixedUpdate()
    {
        UpdateSnapPoints();
        if (GhostIsOn)
            UpdateGhostRoad();
    }

    void UpdateGhostRoad()
    {
        RemoveGhostRoad();
        if (Build.ShouldShowGhostRoad())
        {
            ghostroad.gameObject.SetActive(true);
            Road road = Build.BuildGhostRoad(InputSystem.MouseWorldPos);
            ghostroad.Road = road;
        }
        else
            ghostroad.gameObject.SetActive(false);
    }

    public static void RemoveGhostRoad()
    {
        if (ghostroad.Road != null)
            Game.RemoveRoad(ghostroad.Road);
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
                snapPoint.transform.position = node.Pos;
                activeSnapPoints.Add(snapPoint);
            }
        BuildTargets startTarget = Build.GetStartTarget();
        if (startTarget != null && startTarget.SnapNotNull)
            foreach (Node node in startTarget.Nodes)
            {
                SnapPoint snapPoint = snapPointPool.Get();
                snapPoint.transform.position = node.Pos;
                activeSnapPoints.Add(snapPoint);
            }
    }
}