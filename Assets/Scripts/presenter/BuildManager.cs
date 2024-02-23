using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class BuildManager
{
    private static int pivotNode;
    private const float SplineResolution = GlobalConstants.SplineResolution;
    private const float LaneWidth = GlobalConstants.LaneWidth;
    public static int LaneCount { get; set; }
    public static int NextAvailableId { get; set; }
    public static IBuildManagerBoundary Client;
    private const float SnapDistance = GlobalConstants.SnapDistance;
    private static SortedDictionary<int, Road> roadWatcher;
    public static SortedDictionary<int, Road> RoadWatcher
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
    private static List<BuildTarget> startTargets = null;
    private static List<BuildTarget> endTargets = null;

    static BuildManager()
    {
        LaneCount = 1;
        pivotNode = -1;
        NextAvailableId = 0;
    }

    public static void Reset()
    {
        LaneCount = 1;
        pivotNode = -1;
        RoadWatcher = new();
        Client = null;
        NextAvailableId = 0;
        startTargets = null;
        endTargets = null;
    }

    public static void HandleBuildCommand()
    {
        int hoveredNode = Grid.GetIdByPos(Client.GetPos());
        
        if (HoveredNodeInvalid(hoveredNode))
        {
            return;
        }
        if (startTargets == null)
        {
            startTargets = GetBuildTarget(hoveredNode, LaneCount);
            Utility.Info.Log($"Road Manager: StartNode: ");
        }
        else if (PivotNodeUnassigned())
        {
            pivotNode = hoveredNode;
            Utility.Info.Log($"Road Manager: PivotNode: " + pivotNode);
        }
        else
        {
            endTargets = GetBuildTarget(hoveredNode, LaneCount);
            Utility.Info.Log($"Road Manager: EndNode: ");
            BuildRoad(startTargets, pivotNode, endTargets);

            startTargets = null;
            pivotNode = -1;
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

    static void BuildRoad(List<BuildTarget> startTargets, int pivotNode, List<BuildTarget> endTargets)
    {
        Vector3 posA;
        int startNode = startTargets[0].Node;
        int endNode = endTargets[0].Node;
        if (startTargets[0].Lane == null)
        {
            posA = Grid.GetPosByID(startTargets[0].Node);
        }
        else
        {
            posA = startTargets[0].ExactPos;
        }
        Vector3 posB = Grid.GetPosByID(pivotNode);
        Vector3 posC;
        if (endTargets[0].Lane == null)
        {
            posC = Grid.GetPosByID(endTargets[0].Node);
        }
        else
        {
            posC = endTargets[0].ExactPos;
        }


        float linearLength = Vector3.Distance(posA, posB) + Vector3.Distance(posB, posC);
        int knotCount = (int)(linearLength * SplineResolution + 1);

        Lane connectedLaneStart = CheckConnection(startNode);
        Lane connectedLaneEnd = CheckConnection(endNode);
        if (connectedLaneStart != null || connectedLaneEnd != null)
        {
            Utility.Info.Log("Road Manager: Connecting Roads");
            Road road = InitiateRoad(startNode, pivotNode, endNode, knotCount);
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
            Road road = InitiateRoad(startNode, pivotNode, endNode, knotCount);
            road.InitiateStartIntersection();
            road.InitiateEndIntersection();
        }
    }

    static Road InitiateRoad(int startNode, int pivotNode, int endNode, int knotCount)
    {
        Spline roadSpline = BuildSplineQuadraticInterpolation(startNode, pivotNode, endNode, knotCount);
        Road road = new()
        {
            Id = NextAvailableId++,
            Spline = roadSpline,
            StartNode = startNode,
            PivotNode = pivotNode,
            EndNode = endNode,
            SplineKnotCount = knotCount
        };
        RoadWatcher.Add(road.Id, road);

        List<Lane> lanes = InitiateLanes(road, LaneCount);
        road.Lanes = lanes;

        PathGraph.Graph.AddVertex(startNode);
        PathGraph.Graph.AddVertex(endNode);
        PathGraph.Graph.AddEdge(new TaggedEdge<int, Spline>(startNode, endNode, roadSpline));

        Client.InstantiateRoad(road);
        return road;
    }

    static void ReloadAllSpline()
    {
        foreach (Road road in RoadWatcher.Values)
        {
            road.Spline = BuildSplineQuadraticInterpolation(
                road.StartNode,
                road.PivotNode,
                road.EndNode,
                road.SplineKnotCount
            );

            int index = 0;
            int laneCount = road.Lanes.Count;
            foreach (Lane lane in road.Lanes)
                lane.Spline = GetLaneSpline(road.Spline, laneCount, index++);
        }
    }

    static Spline BuildSplineQuadraticInterpolation(int startNode, int pivotNode, int endNode, int knotCount)
    {
        Vector3 posA = Grid.GetPosByID(startNode);
        Vector3 posB = Grid.GetPosByID(pivotNode);
        Vector3 posC = Grid.GetPosByID(endNode);

        Spline spline = new();
        Vector3 AB, BC, AB_BC;
        knotCount -= 1;
        for (int i = 0; i <= knotCount; i++)
        {
            AB = Vector3.Lerp(posA, posB, 1 / (float)knotCount * i);
            BC = Vector3.Lerp(posB, posC, 1 / (float)knotCount * i);
            AB_BC = Vector3.Lerp(AB, BC, 1 / (float)knotCount * i);
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
                Spline = GetLaneSpline(road.Spline, laneCount, i),
                Road = road,
                Start = Grid.GetIdByPos(GetLanePosition(spline, 0, laneCount, i)),
                End = Grid.GetIdByPos(GetLanePosition(spline, 1, laneCount, i))
            });
        }
        return lanes;
    }

    public static Spline GetLaneSpline(Spline roadSpline, int laneCount, int laneNumber)
    {
        int segCount = roadSpline.Knots.Count() - 1;
        Spline laneSpline = new();

        // Iterate by distance on curve
        for (int i = 0; i <= segCount; i++)
        {
            float t = 1 / (float)segCount * i;

            float3 pos = GetLanePosition(roadSpline, t, laneCount, laneNumber);
            laneSpline.Add(new BezierKnot(pos), TangentMode.AutoSmooth);
        }
        return laneSpline;
    }

    private static float3 GetLanePosition(Spline spline, float t, int laneCount, int lane)
    {
        float3 normal = GetNormal(spline, t);
        float3 position = spline.EvaluatePosition(t);
        return position + normal * LaneWidth * ((float)laneCount / 2 - 0.5f) - lane * normal * LaneWidth;
    }
    private static float3 GetNormal(Spline spline, float t)
    {
        float3 tangent = spline.EvaluateTangent(t);
        float3 upVector = spline.EvaluateUpVector(t);
        return Vector3.Cross(tangent, upVector).normalized;
    }

    static List<BuildTarget> GetBuildTarget(int hoveredNode, int count)
    {
        List<BuildTarget> BuildTargets = new();
        List<BuildTarget> candidates = new();
        if (roadWatcher != null)
            foreach (Road road in roadWatcher.Values)
                foreach (Lane lane in road.Lanes)
                    foreach (int node in new List<int>() { lane.Start, lane.End })
                    {
                        float distance = Grid.GetDistance(hoveredNode, node);
                        if (distance < SnapDistance)
                        {
                            float t = node == lane.Start ? 0 : 1;
                            float3 exactPos = lane.Spline.EvaluatePosition(t);
                            candidates.Add(new BuildTarget(lane, node, distance, exactPos));
                        }
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
                BuildTargets.Add(new BuildTarget( hoveredNode));
            }

        }
        return BuildTargets;

    }

    private class BuildTarget : IComparable<BuildTarget>
    {
        public int Node { get; set; }
        public Lane Lane { get; set; }
        public float Distance { get; set; }
        public float3 ExactPos { get; set; }

        public BuildTarget(Lane lane, int node, float distance, float3 ExactPos)
        {
            Lane = lane;
            Node = node;
            Distance = distance;
        }

        public BuildTarget(int node)
        {
            Lane = null;
            Node = node;
            Distance = -1;
            ExactPos = new float3(-1, -1, -1);
        }

        public int CompareTo(BuildTarget other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }

    public static void RedrawAllRoads()
    {
        ReloadAllSpline();
        Client.RedrawAllRoads();
        NextAvailableId = RoadWatcher.Last().Key + 1;
    }
}
