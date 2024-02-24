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
    private static float3 pivotPos;
    private static bool pivotPosAssigned;
    private const float SplineResolution = GlobalConstants.SplineResolution;
    private const float LaneWidth = GlobalConstants.LaneWidth;
    public static int LaneCount { get; set; }
    private static int nextAvailableId;
    private static int nextAvailableNodeID;
    public static IBuildManagerBoundary Client;
    private const float SnapDistance = GlobalConstants.SnapDistance;
    private static List<BuildTarget> startTargets = null;
    private static List<BuildTarget> endTargets = null;

    static BuildManager()
    {
        LaneCount = 1;
        pivotPosAssigned = false;
        nextAvailableId = 0;
        nextAvailableNodeID = 0;
    }

    public static void Reset()
    {
        LaneCount = 1;
        pivotPosAssigned = false;
        Client = null;
        nextAvailableId = 0;
        nextAvailableNodeID = 0;
        startTargets = null;
        endTargets = null;
    }

    public static void HandleBuildCommand()
    {
        float3 clickPos = Client.GetPos();

        if (startTargets == null)
        {
            startTargets = GetBuildTarget(clickPos, LaneCount);
            Utility.Info.Log($"Road Manager: StartNode: ");
        }
        else if (!pivotPosAssigned)
        {
            pivotPosAssigned = true;
            pivotPos = clickPos;
            Utility.Info.Log($"Road Manager: PivotNode: " + pivotPos);
        }
        else
        {
            endTargets = GetBuildTarget(clickPos, LaneCount);
            Utility.Info.Log($"Road Manager: EndNode: ");
            BuildRoad(startTargets, pivotPos, endTargets);

            startTargets = null;
            pivotPosAssigned = false;
        }

    }

    static void BuildRoad(List<BuildTarget> startTargets, float3 pivotPos, List<BuildTarget> endTargets)
    {
        float3 startPos = startTargets[0].Pos;
        float3 endPos = endTargets[0].Pos;

        float linearLength = Vector3.Distance(startPos, pivotPos) + Vector3.Distance(pivotPos, endPos);
        int knotCount = (int)(linearLength * SplineResolution + 1);

        if (startTargets[0].Node != -1 || endTargets[0].Node != -1)
        {
            Utility.Info.Log("Road Manager: Connecting Roads");
            Road road = InitiateRoad(startPos, pivotPos, endPos, knotCount);
            if (startTargets[0].Node != -1)
            {
                Game.NodeWithLane[startTargets[0].Node].Add(road.Lanes[0]);
                road.Lanes[0].StartNode = startTargets[0].Node;
            }
            else if (endTargets[0].Node != -1)
            {
                Game.NodeWithLane[endTargets[0].Node].Add(road.Lanes[0]);
                road.Lanes[0].EndNode = endTargets[0].Node;
            }
            AutoAssignNodeNumber(road);
        }
        else
        {
            Road road = InitiateRoad(startPos, pivotPos, endPos, knotCount);
            AutoAssignNodeNumber(road);
        }
    }

    static void AutoAssignNodeNumber(Road road)
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

    static List<BuildTarget> GetBuildTarget(float3 clickPos, int count)
    {
        List<BuildTarget> BuildTargets = new();
        List<BuildTarget> candidates = new();
        foreach (Road road in Game.RoadWatcher.Values)
            foreach (Lane lane in road.Lanes)
            {
                List<int> nodes = new() { lane.StartNode, lane.EndNode };
                List<float3> pos = new() { lane.StartPos, lane.EndPos };
                for (int i = 0; i < 2; i++)
                {
                    float distance = Vector3.Distance(clickPos, pos[i]);
                    if (distance < SnapDistance)
                    {
                        candidates.Add(new BuildTarget(lane, nodes[i], distance, pos[i]));
                    }
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
                BuildTargets.Add(new BuildTarget(clickPos));
            }

        }
        return BuildTargets;

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

    public static void RedrawAllRoads()
    {
        ReloadAllSpline();
        Client.RedrawAllRoads();
    }

    public static void ComplyToNewGameState()
    {
        ReloadAllSpline();
        nextAvailableId = Game.RoadWatcher.Last().Key + 1;
        nextAvailableNodeID = Game.NodeWithLane.Last().Key + 1;
    }
}
