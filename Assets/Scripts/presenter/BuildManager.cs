using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class BuildManager
{
    private static int startNode, pivotNode, endNode;
    private const float SplineResolution = 0.4f;
    private const float LaneWidth = GlobalConstants.LaneWidth;
    public static int LaneCount { get; set; }
    public static int NextAvailableId { get; set; }
    public static IBuildManagerBoundary Client;
    private const float SnapDistance = GlobalConstants.SnapDistance;
    private static Dictionary<int, Road> roadWatcher;
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
        startNode = -1;
        pivotNode = -1;
        NextAvailableId = 0;
    }

    public static void Reset()
    {
        LaneCount = 1;
        startNode = -1;
        pivotNode = -1;
        RoadWatcher = new();
        Client = null;
        NextAvailableId = 0;
    }

    public static void HandleBuildCommand()
    {
        int hoveredNode = Grid.GetIdByPos(Client.GetPos());
        if (HoveredNodeInvalid(hoveredNode))
        {
            return;
        }
        if (StartNodeUnassigned())
        {
            List<BuildTarget> buildTargets = GetBuildTarget(hoveredNode, LaneCount);
            if (buildTargets[0] == null)
            {
                startNode = hoveredNode;
            }
            else
            {
                startNode = buildTargets[0].Node;
            }
            Log.Info.Log($"Road Manager: Tile A loaded");
        }
        else if (PivotNodeUnassigned())
        {
            pivotNode = hoveredNode;
            Log.Info.Log($"Road Manager: Tile B loaded");
        }
        else
        {
            endNode = hoveredNode;
            Log.Info.Log($"Road Manager: Tile C loaded");
            BuildRoad();

            startNode = -1;
            pivotNode = -1;
        }

        static bool StartNodeUnassigned()
        {
            return startNode == -1;
        }

        static bool PivotNodeUnassigned()
        {
            return pivotNode == -1;
        }

        static bool HoveredNodeInvalid(int hoveredNode)
        {
            return hoveredNode == -1;
        }
    }

    static void BuildRoad()
    {
        Vector3 posA = Grid.GetPosByID(startNode);
        Vector3 posB = Grid.GetPosByID(pivotNode);
        Vector3 posC = Grid.GetPosByID(endNode);

        float linearLength = Vector3.Distance(posA, posB) + Vector3.Distance(posB, posC);
        int segCount = (int)(linearLength * SplineResolution + 1);

        Lane connectedLaneStart = CheckConnection(startNode);
        Lane connectedLaneEnd = CheckConnection(endNode);
        if (connectedLaneStart != null || connectedLaneEnd != null)
        {
            posA = connectedLaneStart != null ? connectedLaneStart.Spline.EvaluatePosition(1) : posA;
            posC = connectedLaneEnd != null ? connectedLaneEnd.Spline.EvaluatePosition(0) : posC;
            Spline spline = BuildSplineQuadraticInterpolation(posA, posB, posC, segCount);
            Log.Info.Log("Road Manager: Connecting Roads");
            Road road = InitiateRoad(spline);
            Road connectedRoad;
            Intersection intersection = null;
            if (connectedLaneEnd != null)
            {
                connectedRoad = connectedLaneEnd.Road;
                intersection = connectedRoad.StartIx;
                connectedRoad.StartIx.NodeWithLane[endNode].Add(road.Lanes[0]);
                road.EndIx = intersection;
                road.InitiateStartIntersection();
            }
            else if (connectedLaneStart != null)
            {
                connectedRoad = connectedLaneStart.Road;
                intersection = connectedRoad.EndIx;
                connectedRoad.EndIx.NodeWithLane[startNode].Add(road.Lanes[0]);
                road.StartIx = intersection;
                road.InitiateEndIntersection();
            }
            intersection.Roads.Add(road);
            Client.EvaluateIntersection(intersection);
        }
        if (connectedLaneStart == null)
        {
            Spline spline = BuildSplineQuadraticInterpolation(posA, posB, posC, segCount);
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
            Spline = spline,
            StartNode = startNode,
            PivotNode = pivotNode,
            EndNode = endNode
        };
        RoadWatcher.Add(road.Id, road);

        List<Lane> lanes = InitiateLanes(road, LaneCount);
        road.Lanes = lanes;

        PathGraph.Graph.AddVertex(startNode);
        PathGraph.Graph.AddVertex(endNode);
        PathGraph.Graph.AddEdge(new TaggedEdge<int, Spline>(startNode, endNode, spline));

        Client.InstantiateRoad(road);
        return road;
    }

    static Spline BuildSplineQuadraticInterpolation(Vector3 posA, Vector3 posB, Vector3 posC, int nodeCount)
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

    static List<BuildTarget> GetBuildTarget(int target, int count)
    {
        List<BuildTarget> BuildTargets = new();
        List<BuildTarget> candidates = new();
        foreach (Road road in roadWatcher.Values)
            foreach (Lane lane in road.Lanes)
                foreach (int node in new List<int>() { lane.Start, lane.End })
                {
                    float distance = Grid.GetDistance(target, node);
                    if (distance < SnapDistance)
                        candidates.Add(new BuildTarget(lane, node, distance));
                }
        candidates.Sort();
        for (int i = 0; i < count; i++)
        {
            if (i < candidates.Count)
            {
                BuildTargets.Add(candidates[i]);
            }
            else
            {
                BuildTargets.Add(null);
            }
            
        }
        return BuildTargets;

    }

    private class BuildTarget : IComparable<BuildTarget>
    {
        public int Node { get; set; }
        public Lane Lane { get; set; }
        public float Distance { get; set; }

        public BuildTarget(Lane lane, int node, float distance)
        {
            Lane = lane;
            Node = node;
            Distance = distance;
        }

        public int CompareTo(BuildTarget other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }

}
