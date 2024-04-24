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
        ghostroad = Instantiate(roadPrefab);
        activeSnapPoints = new();
    }
    void FixedUpdate()
    {
        UpdateSnapPoints();
        UpdateGhostRoad();
    }

    void UpdateGhostRoad()
    {
        RemoveGhostRoad();
        if (BuildHandler.ShouldShowGhostRoad())
        {
            ghostroad.gameObject.SetActive(true);
            Road road = BuildHandler.BuildGhostRoad(InputSystem.MouseWorldPos);
            ghostroad.Road = road;
            if (road != null)
                Roads.UpdateRoadMesh(road);
        }
        else
        {
            ghostroad.gameObject.SetActive(false);
        }
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
        BuildTargets polled = BuildHandler.PollBuildTarget(InputSystem.MouseWorldPos);
        if (polled.SnapNotNull)
            foreach (Node node in polled.Nodes)
            {
                SnapPoint snapPoint = snapPointPool.Get();
                snapPoint.transform.position = node.Pos;
                activeSnapPoints.Add(snapPoint);
            }
        BuildTargets startTarget = BuildHandler.GetStartTarget();
        if (startTarget != null && startTarget.SnapNotNull)
            foreach (Node node in startTarget.Nodes)
            {
                SnapPoint snapPoint = snapPointPool.Get();
                snapPoint.transform.position = node.Pos;
                activeSnapPoints.Add(snapPoint);
            }
    }
}