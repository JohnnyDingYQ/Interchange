using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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
        float3 startPos = SnapSuccessful(startTargets) ? GetMedianPoint(startTargets) : startClick;
        float3 endPos = SnapSuccessful(endTargets) ? GetMedianPoint(endTargets) : endClick;

        float linearLength = Vector3.Distance(startPos, pivotPos) + Vector3.Distance(pivotPos, endPos);
        int knotCount = (int)(linearLength * SplineResolution + 1);

        AlignPivotPos();

        if (SnapSuccessful(startTargets))
        {
            Road other = GetConnectingRoad(startTargets);
            pivotPos = (float3)Vector3.Project(pivotPos - startPos, CurveUtility.EvaluateTangent( other.Curve, 1)) + startPos;
        }
        if (SnapSuccessful(endTargets))
        {
            Road other = GetConnectingRoad(endTargets);
            pivotPos = (float3)Vector3.Project(pivotPos - endPos, -1 * CurveUtility.EvaluateTangent( other.Curve, 0)) + endPos;
        }

        Road road = InitRoad(startPos, pivotPos, endPos, knotCount); ;

        if (SnapSuccessful(startTargets))
        {
            for (int i = 0; i < startTargets.Count; i++)
            {
                Game.NodeWithLane[startTargets[i].Node].Add(road.Lanes[i]);
                road.Lanes[i].StartNode = startTargets[i].Node;
                Utility.Info.Log("RoadManager: Connecting Start");
            }

        }
        if (SnapSuccessful(endTargets))
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

        static bool SnapSuccessful(List<BuildTarget> bts)
        {
            return bts != null;
        }

        static Road GetConnectingRoad(List<BuildTarget> bts)
        {
            return bts.First().Lane.Road;
        }

        void AlignPivotPos()
        {
            if (SnapSuccessful(startTargets))
            {
                Road other = GetConnectingRoad(startTargets);
                pivotPos = (float3) Vector3.Project(pivotPos - startPos, CurveUtility.EvaluateTangent( other.Curve, 1)) + startPos;
            }
            if (SnapSuccessful(endTargets))
            {
                Road other = GetConnectingRoad(endTargets);
                pivotPos = (float3) Vector3.Project(pivotPos - endPos, -1 * CurveUtility.EvaluateTangent( other.Curve, 0)) + endPos;
            }
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

    static Road InitRoad(float3 startPos, float3 pivotPos, float3 endPos, int knotCount)
    {
        Road road = new(startPos, pivotPos, endPos, LaneCount)
        {
            Id = nextAvailableId++
        };
        Game.RoadWatcher.Add(road.Id, road);

        Client.InstantiateRoad(road);
        return road;
    }

    static List<BuildTarget> GetBuildTarget(float3 clickPos, int laneCount)
    {
        float snapRadius = laneCount / 2 * LaneWidth + SnapTolerance;
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
        List<BuildTarget> sortedByLaneIndex = candidates.OrderBy(o => o.Lane.LaneIndex).ToList();
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

    static void ReloadAllSpline()
    {
        foreach (Road road in Game.RoadWatcher.Values)
        {
            road.InitCurve();
            foreach (Lane lane in road.Lanes)
                lane.InitSpline();
        }
    }
    public static void ComplyToNewGameState()
    {
        ReloadAllSpline();
        nextAvailableId = Game.RoadWatcher.Last().Key + 1;
        nextAvailableNodeID = Game.NodeWithLane.Last().Key + 1;
    }
}
