using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class BuildManager
{
    private static int start, pivot, end;
    private const float SplineResolution = 0.4f;
    private const float LaneWidth = GlobalConstants.LaneWidth;
    public static int LaneCount {get; set;}
    public static int NextAvailableId { get; set; }
    public static IBuildManagerBoundary client;
    public static Dictionary<int, Road> roadWatcher;
    public static Dictionary<int, Road> RoadWatcher
    {
        get
        {
            roadWatcher ??= new();
            return roadWatcher;
        }
        set
        {
            roadWatcher = value;
        }
    }

    static BuildManager()
    {
        LaneCount = 1;
        start = -1;
        pivot = -1;
    }

    public static void HandleBuildCommand()
    {
        int hoveredTile = Grid.GetIdByPos(client.GetPos());
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

    static void BuildRoad()
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

    static Road InitiateRoad(Spline spline)
    {
        
        Road road = new()
        {
            Id = NextAvailableId++,
            Spline = spline
        };
        RoadWatcher.Add(road.Id, road);

        List<Lane> lanes = InitiateLanes(road, LaneCount);
        road.Lanes = lanes;

        PathGraph.Graph.AddVertex(start);
        PathGraph.Graph.AddVertex(end);
        PathGraph.Graph.AddEdge(new TaggedEdge<int, Spline>(start, end, spline));

        client.InstantiateRoad(road);
        return road;
    }

    static Spline BuildSpline(Vector3 posA, Vector3 posB, Vector3 posC, int nodeCount)
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

    static Lane CheckConnection(int node)
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

    static List<Lane> InitiateLanes(Road road, int laneCount)
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
