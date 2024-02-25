using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public static class BuildManager
{
    private static float3 startClick, pivotClick, endClick;
    private static bool startAssigned, pivotAssigned;
    private const float SplineResolution = GlobalConstants.SplineResolution;
    private const float LaneWidth = GlobalConstants.LaneWidth;
    public static int LaneCount { get; set; }
    private static int nextAvailableId;
    private static int nextAvailableNodeID;
    public static IBuildManagerBoundary Client;
    private const float SnapTolerance = GlobalConstants.SnapTolerance;
    private static List<BuildTarget> startTargets;
    private static List<BuildTarget> endTargets;

    static BuildManager()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        nextAvailableId = 0;
        nextAvailableNodeID = 0;
    }

    public static void Reset()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        Client = null;
        nextAvailableId = 0;
        nextAvailableNodeID = 0;
    }

    public static void HandleBuildCommand()
    {
        float3 clickPos = Client.GetPos();

        if (!startAssigned)
        {
            startAssigned = true;
            startTargets = GetBuildTarget(clickPos, LaneCount);
            startClick = clickPos;
            Utility.Info.Log($"Road Manager: StartNode: ");
        }
        else if (!pivotAssigned)
        {
            pivotAssigned = true;
            pivotClick = clickPos;
            Utility.Info.Log($"Road Manager: PivotNode: " + pivotClick);
        }
        else
        {
            endTargets = GetBuildTarget(clickPos, LaneCount);
            endClick = clickPos;
            Utility.Info.Log($"Road Manager: EndNode: ");
            BuildRoad(startTargets, pivotClick, endTargets);
            startAssigned = false;
            pivotAssigned = false;
        }

    }

    static void BuildRoad(List<BuildTarget> startTargets, float3 pivotPos, List<BuildTarget> endTargets)
    {
        float3 startPos = startTargets != null ? GetMedianPoint(startTargets) : startClick;
        float3 endPos = endTargets != null ? GetMedianPoint(endTargets) : endClick;

        float linearLength = Vector3.Distance(startPos, pivotPos) + Vector3.Distance(pivotPos, endPos);
        int knotCount = (int)(linearLength * SplineResolution + 1);

        Road road = InitiateRoad(startPos, pivotPos, endPos, knotCount);

        if (startTargets != null)
        {
            for (int i = 0; i < startTargets.Count; i++)
            {
                Game.NodeWithLane[startTargets[i].Node].Add(road.Lanes[i]);
                road.Lanes[i].StartNode = startTargets[i].Node;
                Utility.Info.Log("RoadManager: Connecting Start");
            }
            
        }
        else if (endTargets != null)
        {
            for (int i = 0; i < endTargets.Count; i++)
            {
                Game.NodeWithLane[endTargets[i].Node].Add(road.Lanes[i]);
                road.Lanes[i].EndNode = endTargets[i].Node;
                Utility.Info.Log("RoadManager: Connecting End");
            }
        }

        AssignUnassignedNodeNumber(road);

        static float3 GetMedianPoint(List<BuildTarget> bts)
        {
            return Vector3.Lerp(bts.First().Pos, bts.Last().Pos, 0.5f);
        }
    }

    static void AssignUnassignedNodeNumber(Road road)
    {
        foreach (Lane lane in road.Lanes)
        {
            if (lane.StartNode == -1)
            {
                Game.NodeWithLane[nextAvailableNodeID] = new HashSet<Lane>() { lane };
                lane.StartNode = nextAvailableNodeID++;
            }

            if (lane.EndNode == -1)
            {
                Game.NodeWithLane[nextAvailableNodeID] = new HashSet<Lane>() { lane };
                lane.EndNode = nextAvailableNodeID++;
            }

        }
    }

    static Road InitiateRoad(float3 startPos, float3 pivotPos, float3 endPos, int knotCount)
    {
        Spline roadSpline = BuildSplineQuadraticInterpolation(startPos, pivotPos, endPos, knotCount);
        Road road = new()
        {
            Id = nextAvailableId++,
            Spline = roadSpline,
            StartPos = startPos,
            PivotPos = pivotPos,
            EndPos = endPos,
            SplineKnotCount = knotCount
        };
        Game.RoadWatcher.Add(road.Id, road);

        List<Lane> lanes = InitiateLanes(road, LaneCount);
        road.Lanes = lanes;

        Client.InstantiateRoad(road);
        Utility.DrawAllSplines();
        return road;
    }

    static void ReloadAllSpline()
    {
        foreach (Road road in Game.RoadWatcher.Values)
        {
            road.Spline = BuildSplineQuadraticInterpolation(
                road.StartPos,
                road.PivotPos,
                road.EndPos,
                road.SplineKnotCount
            );

            int index = 0;
            int laneCount = road.Lanes.Count;
            foreach (Lane lane in road.Lanes)
                lane.Spline = GetLaneSpline(road.Spline, laneCount, index++);
        }
    }

    static Spline BuildSplineQuadraticInterpolation(float3 startPos, float3 pivotPos, float3 endPos, int knotCount)
    {
        Spline spline = new();
        Vector3 AB, BC, AB_BC;
        knotCount -= 1;
        for (int i = 0; i <= knotCount; i++)
        {
            AB = Vector3.Lerp(startPos, pivotPos, 1 / (float)knotCount * i);
            BC = Vector3.Lerp(pivotPos, endPos, 1 / (float)knotCount * i);
            AB_BC = Vector3.Lerp(AB, BC, 1 / (float)knotCount * i);
            spline.Add(new BezierKnot(AB_BC), TangentMode.AutoSmooth);
        }
        return spline;
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
                StartPos = InterpolateLanePos(spline, 0, laneCount, i),
                EndPos = InterpolateLanePos(spline, 1, laneCount, i),
                StartNode = -1,
                EndNode = -1,
                LaneIndex = i
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

            float3 pos = InterpolateLanePos(roadSpline, t, laneCount, laneNumber);
            laneSpline.Add(new BezierKnot(pos), TangentMode.AutoSmooth);
        }
        return laneSpline;
    }

    private static float3 InterpolateLanePos(Spline spline, float t, int laneCount, int lane)
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

    static List<BuildTarget> GetBuildTarget(float3 clickPos, int laneCount)
    {
        float snapRadius = laneCount * LaneWidth + SnapTolerance;
        List<BuildTarget> candidates = new();
        foreach (Lane lane in Game.GetLaneIterator())
        {
            List<int> nodes = new() { lane.StartNode, lane.EndNode };
            List<float3> pos = new() { lane.StartPos, lane.EndPos };
            for (int i = 0; i < 2; i++)
            {
                float distance = Vector3.Distance(clickPos, pos[i]);
                if (distance < snapRadius)
                {
                    candidates.Add(new BuildTarget(lane, nodes[i], distance, pos[i]));
                }
            }
        }

        candidates.Sort();
        if (candidates.Count >= laneCount)
        {
            candidates = candidates.GetRange(0, laneCount);
        }
        else
        {
            return null;
        }
        Road road = candidates.First().Lane.Road;
        foreach (BuildTarget bt in candidates)
        {
            if (road == bt.Lane.Road)
            {
                road = bt.Lane.Road;
            }
            else
            {
                return null;
            }

        }
        List<BuildTarget> sortedByLaneIndex = candidates.OrderBy(o=>o.Lane.LaneIndex).ToList();
        return sortedByLaneIndex;

    }

    private class BuildTarget : IComparable<BuildTarget>
    {
        public int Node { get; set; }
        public Lane Lane { get; set; }
        public float Distance { get; set; }
        public float3 Pos { get; set; }

        public BuildTarget(Lane lane, int node, float distance, float3 pos)
        {
            Lane = lane;
            Node = node;
            Distance = distance;
            Pos = pos;
        }

        public BuildTarget(float3 clickPos)
        {
            Lane = null;
            Node = -1;
            Distance = -1;
            Pos = clickPos;
        }

        public int CompareTo(BuildTarget other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }

    public static void ComplyToNewGameState()
    {
        ReloadAllSpline();
        nextAvailableId = Game.RoadWatcher.Last().Key + 1;
        nextAvailableNodeID = Game.NodeWithLane.Last().Key + 1;
    }
}
