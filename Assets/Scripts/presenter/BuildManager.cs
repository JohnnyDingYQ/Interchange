using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class BuildManager : MonoBehaviour
{
    private static int start, pivot, end;
    private const float SplineResolution = 0.4f;
    private const float LaneWidth = GlobalConstants.LaneWidth;
    [SerializeField] private GameObject roads;
    [SerializeField] private RoadGameObject roadPrefab;
    [SerializeField] private InputManager inputManager;
    private int laneCount = 1;
    private int nextAvailableId;
    public static Dictionary<int, Road> RoadWatcher { get; set; }
    void Start()
    {
        start = -1;
        pivot = -1;

        RoadWatcher = new();

        if (inputManager != null)
        { 
            inputManager.BuildRoad += HandleBuildCommand;
            inputManager.Build1Lane += BuildMode_OneLane;
            inputManager.Build2Lane += BuildMode_TwoLanes;
            inputManager.Build3Lane += BuildMode_ThreeLanes;
        }
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.BuildRoad -= HandleBuildCommand;
            inputManager.Build1Lane -= BuildMode_OneLane;
            inputManager.Build2Lane -= BuildMode_TwoLanes;
            inputManager.Build3Lane -= BuildMode_ThreeLanes;
        }
    }

    void HandleBuildCommand()
    {
        int hoveredTile = Grid.GetIdByPos(Main.MouseWorldPos);
        if (hoveredTile == -1)
        {
            return;
        }
        if (start == -1)
        {
            start = Snapper.Snapped() ? Snapper.SnappedNode : hoveredTile;
            Log.Info.Log($"Road Manager: Tile A loaded");
        }
        else if (pivot == -1)
        {
            pivot = hoveredTile;
            Log.Info.Log($"Road Manager: Tile B loaded");
        }
        else
        {
            end = hoveredTile;
            Log.Info.Log($"Road Manager: Tile C loaded");
            BuildRoad();

            start = -1;
            pivot = -1;
        }
    }

    void BuildMode_OneLane()
    {
        laneCount = 1;
    }

    void BuildMode_TwoLanes()
    {
        laneCount = 2;
    }

    void BuildMode_ThreeLanes()
    {
        laneCount = 3;
    }
    void BuildRoad()
    {
        Vector3 posA = Grid.GetWorldPosByID(start);
        Vector3 posB = Grid.GetWorldPosByID(pivot);
        Vector3 posC = Grid.GetWorldPosByID(end);

        float linearLength = Vector3.Distance(posA, posB) + Vector3.Distance(posB, posC);
        int segCount = (int)(linearLength * SplineResolution + 1);

        Lane connectedLane = CheckConnection(start);
        if (connectedLane != null)
        {
            Spline spline = BuildSpline(connectedLane.Spline.EvaluatePosition(1), posB, posC, segCount);
            Log.Info.Log("Road Manager: Connecting Roads");
            Road road = InitiateRoad(spline);
            Road connectedRoad = connectedLane.Road;
            Intersection intersection = null;
            if (start == connectedLane.Start)
            {
                intersection = connectedRoad.Start;
                intersection.NodeWithLane[start].Add(road.Lanes[0]);

            }
            else if (start == connectedLane.End)
            {
                intersection = connectedRoad.End;
                connectedLane.Road.End.NodeWithLane[start].Add(road.Lanes[0]);
            }
            intersection.Roads.Add(road);
            road.InitiateEndIntersection();
            RoadView.EvaluateIntersection(intersection);
        }
        if (connectedLane == null)
        {
            Spline spline = BuildSpline(posA, posB, posC, segCount);
            Road road = InitiateRoad(spline);
            road.InitiateStartIntersection();
            road.InitiateEndIntersection();
        }
    }

    Road InitiateRoad(Spline spline)
    {
        RoadGameObject roadGameObject = Instantiate(roadPrefab, roads.transform, true);
        roadGameObject.name = $"Road-{nextAvailableId}";
        roadGameObject.LaneSplines = new();
        Road road = new()
        {
            Id = nextAvailableId++,
            RoadGameObject = roadGameObject,
            Spline = spline
        };
        RoadWatcher.Add(road.Id, road);


        Mesh mesh = RoadView.CreateMesh(road, laneCount);
        roadGameObject.GetComponent<MeshFilter>().mesh = mesh;
        roadGameObject.OriginalMesh = Instantiate(mesh);

        List<Lane> lanes = InitiateLanes(road, laneCount);
        road.Lanes = lanes;

        roadGameObject.Road = road;

        PathGraph.Graph.AddVertex(start);
        PathGraph.Graph.AddVertex(end);
        PathGraph.Graph.AddEdge(new TaggedEdge<int, Spline>(start, end, spline));
        return road;
    }

    Spline BuildSpline(Vector3 posA, Vector3 posB, Vector3 posC, int nodeCount)
    {
        Spline spline = new();
        Vector3 AB, BC, AB_BC;
        nodeCount -= 1;
        for (int i = 0; i <= nodeCount; i++)
        {
            AB = Vector3.Lerp(posA, posB, 1 / (float)nodeCount * i);
            BC = Vector3.Lerp(posB, posC, 1 / (float)nodeCount * i);
            AB_BC = Vector3.Lerp(AB, BC, 1 / (float)nodeCount * i);
            spline.Add(new BezierKnot(AB_BC), TangentMode.AutoSmooth);
        }
        return spline;
    }

    Lane CheckConnection(int node)
    {
        Lane connectedLane = null;
        foreach (var (id, road) in RoadWatcher)
        {
            foreach (Lane lane in road.Lanes)
            {
                if (node == lane.Start)
                {
                    connectedLane = lane;
                }
                else if (node == lane.End)
                {
                    connectedLane = lane;
                }
            }

        }
        return connectedLane;
    }

    List<Lane> InitiateLanes(Road road, int laneCount)
    {
        Spline spline = road.Spline;

        List<Lane> lanes = new();

        for (int i = 0; i < laneCount; i++)
        {
            lanes.Add(new()
            {
                Spline = new(),
                Road = road,
                Start = Grid.GetIdByPos(GetLanePosition(spline, 0, laneCount, i)),
                End = Grid.GetIdByPos(GetLanePosition(spline, 1, laneCount, i))
            });
        }

        int segCount = spline.Knots.Count() - 1;

        // Iterate by distance on curve
        for (int i = 0; i <= segCount; i++)
        {
            float t = 1 / (float)segCount * i;

            // Iterate by each lane
            for (int j = 0; j < laneCount; j++)
            {
                float3 pos = GetLanePosition(spline, t, laneCount, j);
                lanes[j].Spline.Add(new BezierKnot(pos), TangentMode.AutoSmooth);
            }
        }
        return lanes;

        float3 GetLanePosition(Spline spline, float t, int laneCount, int lane)
        {
            float3 normal = GetNormal(spline, t);
            float3 position = spline.EvaluatePosition(t);
            return position + normal * LaneWidth * ((float)laneCount / 2 - 0.5f) - lane * normal * LaneWidth;
        }
        float3 GetNormal(Spline spline, float t)
        {
            float3 tangent = spline.EvaluateTangent(t);
            float3 upVector = spline.EvaluateUpVector(t);
            return Vector3.Cross(tangent, upVector).normalized;
        }

    }

}
