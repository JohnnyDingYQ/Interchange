using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public static class Build
{
    private static float3 pivotPos;
    private static bool startAssigned, pivotAssigned, pivotAligned;
    private static Zone startZone;
    public static int LaneCount { get; set; }
    public static BuildTargets StartTarget { get; set; }
    public static BuildTargets EndTarget { get; set; }
    public static List<Tuple<float3, float3>> SupportLines { get; }
    public static bool BuildsGhostRoad { get; set; }
    public static List<uint> GhostRoads { get; private set; }
    public static bool ParallelBuildOn { get; set; }
    public static float ParallelSpacing { get; set; }
    public static int Elevation { get; set; }
    public static bool ReplaceSuggestionOn { get; set; }
    static Build()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        pivotAligned = false;
        SupportLines = new();
        BuildsGhostRoad = true;
        GhostRoads = new();
        ParallelSpacing = Constants.DefaultParallelSpacing;
    }

    public static void Reset()
    {
        ResetSelection();
        RemoveAllGhostRoads();
        BuildsGhostRoad = true;
        GhostRoads = new();
        ParallelBuildOn = false;
        ParallelSpacing = Constants.DefaultParallelSpacing;
        ReplaceSuggestionOn = false;
    }

    public static void ResetSelection()
    {
        startAssigned = false;
        pivotAssigned = false;
        pivotAligned = false;
        StartTarget = null;
        EndTarget = null;
        ReplaceSuggestionOn = false;
        RemoveAllGhostRoads();
    }

    public static List<Tuple<float3, float3>> SetSupportLines()
    {
        SupportLines.Clear();
        if (startAssigned)
        {
            float3 startPoint = StartTarget.Snapped ? StartTarget.Pos : StartTarget.ClickPos;
            float3 pivotPoint = pivotPos;
            startPoint.y = 0;
            pivotPoint.y = 0;
            SupportLines.Add(new(startPoint, pivotPoint));
        }
        if (pivotAssigned && EndTarget != null)
        {
            float3 endPoint = EndTarget.Snapped ? EndTarget.Pos : EndTarget.ClickPos;
            float3 pivotPoint = pivotPos;
            endPoint.y = 0;
            pivotPoint.y = 0;
            SupportLines.Add(new(pivotPoint, endPoint));
        }
        return SupportLines;
    }

    static void RemoveAllGhostRoads()
    {
        foreach (uint id in GhostRoads)
            Game.RemoveRoad(Game.Roads[id]);
        GhostRoads.Clear();
    }

    public static void BuildGhostRoad(float3 endTargetClickPos)
    {
        RemoveAllGhostRoads();
        EndTarget = Snapping.Snap(endTargetClickPos, LaneCount, Side.End);
        if (ParallelBuildOn)
            BuildParallelRoads(StartTarget, pivotPos, EndTarget, BuildMode.Ghost);
        else
            BuildRoads(StartTarget, pivotPos, EndTarget, BuildMode.Ghost);
    }

    public static void HandleHover(float3 hoverPos)
    {
        if (!startAssigned)
        {
            Road road = Game.HoveredRoad;
            float distOnRoad = road != null ? road.GetNearestDistance(hoverPos) : 0;
            if (Game.HoveredRoad == null || !road.DistanceBetweenVertices(distOnRoad))
            {
                StartTarget = Snapping.Snap(hoverPos, LaneCount, Side.Start);
                EndTarget = null;
                ReplaceSuggestionOn = false;
            }
            else
            {
                float distance = math.distance(road.Curve.EvaluateDistancePos(distOnRoad), hoverPos);
                bool isOnLeftSide = math.cross(
                    road.Curve.EvaluateDistanceTangent(distOnRoad),
                    road.Curve.EvaluateDistancePos(distOnRoad) - hoverPos).y > 0;
                float offset = isOnLeftSide ? distance : -distance;
                StartTarget = Snapping.Snap(road.StartPos + offset * road.Curve.StartNormal, LaneCount, Side.Both);
                EndTarget = Snapping.Snap(road.EndPos + offset * road.Curve.EndNormal, LaneCount, Side.Both);
                ReplaceSuggestionOn = true;
            }
        }
        if (startAssigned && !pivotAssigned)
            pivotPos = hoverPos;
        if (!pivotAssigned && startAssigned)
            AlignPivotByStart(StartTarget, pivotPos);
        if (startAssigned && pivotAssigned && BuildsGhostRoad)
            BuildGhostRoad(hoverPos);
        SetSupportLines();
    }

    public static void ToggletParallelBuild()
    {
        ParallelBuildOn = !ParallelBuildOn;
    }

    public static List<Road> HandleBuildCommand(float3 clickPos)
    {
        if (!Game.BuildModeOn)
            return null;
        List<Road> roads;
        if (ReplaceSuggestionOn)
        {
            roads = BuildSuggested();
            ResetSelection();
            return roads;
        }
        if (!startAssigned)
        {
            startAssigned = true;
            startZone = Game.HoveredZone;
            StartTarget = Snapping.Snap(clickPos, LaneCount, Side.Start);
            return null;
        }
        if (!pivotAssigned)
        {
            pivotAssigned = true;
            pivotPos = clickPos;
            AlignPivotByStart(StartTarget, pivotPos);
            return null;
        }
        RemoveAllGhostRoads();
        EndTarget = Snapping.Snap(clickPos, LaneCount, Side.End);
        if (ParallelBuildOn)
            roads = BuildParallelRoads(StartTarget, pivotPos, EndTarget, BuildMode.Actual);
        else
        {
            roads = BuildRoads(StartTarget, pivotPos, EndTarget, BuildMode.Actual);
            if (roads != null && LaneCount == 1)
            {
                if (startZone != null && Game.SourceZones.Values.Contains(startZone))
                    startZone.AddVertex(roads.First().Lanes.Single().StartVertex);
                if (Game.HoveredZone != null && Game.TargetZones.Values.Contains(Game.HoveredZone))
                    Game.HoveredZone.AddVertex(roads.Last().Lanes.Single().EndVertex);
            }
        }
        ResetSelection();
        return roads;

        List<Road> BuildSuggested()
        {
            float roadMidIndex = StartTarget.Offset + (float)LaneCount / 2;
            float currMidIndex = StartTarget.SelectedRoad.Lanes.First().StartNode.NodeIndex
                + (float)StartTarget.SelectedRoad.LaneCount / 2;
            float offsetDist = (currMidIndex - roadMidIndex) * Constants.LaneWidth;
            Road road = new(StartTarget.SelectedRoad.Curve.Offset(offsetDist), LaneCount);
            Game.RemoveRoad(StartTarget.SelectedRoad, RoadRemovalOption.Replace);
            roads = ProcessRoad(road, StartTarget, EndTarget);
            Assert.IsTrue(road == roads.Single());

            foreach (Node node in StartTarget.Intersection.Nodes)
                if (!road.Lanes.Contains(node.OutLane) && node.InLane == null)
                    Game.RemoveNode(node);

            foreach (Node node in EndTarget.Intersection.Nodes)
                if (!road.Lanes.Contains(node.InLane) && node.OutLane == null)
                    Game.RemoveNode(node);
            return roads;
        }
    }

    static List<Road> BuildRoads(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget, BuildMode buildMode)
    {
        Road road = InitRoad(startTarget, pivotPos, endTarget);
        if (road == null)
            return null;
        if (buildMode == BuildMode.Ghost)
            road.IsGhost = true;
        return ProcessRoad(road, startTarget, endTarget);
    }

    static List<Road> BuildParallelRoads(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget, BuildMode buildMode)
    {
        Road road = InitRoad(startTarget, pivotPos, endTarget);
        if (road == null)
            return null;
        Curve offsetted = road.Curve.Duplicate().Offset(ParallelSpacing);
        offsetted = offsetted.ReverseChain();
        BuildTargets startTargetParallel = Snapping.Snap(offsetted.StartPos, LaneCount, Side.Start);
        BuildTargets endTargetParallel = Snapping.Snap(offsetted.EndPos, LaneCount, Side.End);
        Road parallel = new(offsetted, LaneCount);
        if (buildMode == BuildMode.Ghost)
        {
            road.IsGhost = true;
            parallel.IsGhost = true;
        }
        List<Road> roads = ProcessRoad(road, startTarget, endTarget);
        roads.AddRange(ProcessRoad(parallel, startTargetParallel, endTargetParallel));
        return roads;
    }

    static Road InitRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget)
    {
        float3 startPos = startTarget.Pos;
        float3 endPos = endTarget.Pos;
        if (!pivotAligned)
            pivotPos = AlignPivotEnd(endTarget, pivotPos);
        if (RoadIsTooBent() || BadSegmentRatio())
            return null;
        Road road = new(ApproximateCircularArc(startPos, pivotPos, endPos), LaneCount);
        return road;

        bool RoadIsTooBent()
        {
            float3 v1 = pivotPos - startPos;
            float3 v2 = endPos - pivotPos;
            float angle = MathF.Abs(
                MathF.Acos(
                    Math.Clamp(math.dot(v1, v2) / math.length(v1) / math.length(v2), -1f, 1f)
                    )
            );
            if (angle > Constants.MaxRoadBendAngle * MathF.PI / 180)
                return true;
            return false;
        }

        bool BadSegmentRatio()
        {
            float segA = math.length(pivotPos - startPos);
            float segB = math.length(pivotPos - endPos);
            float ratio = segA > segB ? segB / segA : segA / segB;
            return ratio < Constants.MinSegmentRatio;
        }
    }

    static List<Road> ProcessRoad(Road road, BuildTargets startTarget, BuildTargets endTarget)
    {
        if (road.HasLaneShorterThanMinLaneLength())
            return null;
        if (startTarget.Snapped)
            road.StartIntersection = startTarget.Intersection;
        if (endTarget.Snapped)
            road.EndIntersection = endTarget.Intersection;
        Game.RegisterRoad(road);

        if (startTarget.Snapped)
            ConnectRoadStartToNodes(startTarget.Intersection, startTarget.Offset, road);
        else
            IntersectionUtil.EvaluateOutline(road.StartIntersection);

        if (endTarget.Snapped)
            ConnectRoadEndToNodes(endTarget.Intersection, endTarget.Offset, road);
        else
            IntersectionUtil.EvaluateOutline(road.EndIntersection);

        List<Road> resultingRoads = new() { road };

        RegisterNodes(road);
        if (!road.IsGhost)
        {
            // move statement below out of this block to divide ghost road
            AutoDivideRoad(road);
        }

        if (road.IsGhost)
            GhostRoads.AddRange(resultingRoads.Select(r => r.Id));
        Game.UpdateIntersection(road.StartIntersection);
        Game.UpdateIntersection(resultingRoads.Last().EndIntersection);
        return resultingRoads;

        # region extracted funcitons
        static float GetLongestLaneLength(Road road)
        {
            float length = 0;
            foreach (Lane lane in road.Lanes)
                length = Math.Max(length, lane.Length);
            return length;
        }

        void AutoDivideRoad(Road road)
        {
            float longestLength = GetLongestLaneLength(road);
            if (longestLength <= Constants.MaxLaneLength)
                return;
            int divisions = 2;
            while (longestLength / divisions > Constants.MaxLaneLength)
                divisions++;
            RecursiveRoadDivision(road, divisions);
        }

        void RecursiveRoadDivision(Road road, int divisions)
        {
            if (divisions == 1)
                return;
            SubRoads subRoads = Divide.DivideRoad(road, road.Curve.Length / divisions);
            resultingRoads.Remove(road);
            resultingRoads.Add(subRoads.Left);
            resultingRoads.Add(subRoads.Right);
            RecursiveRoadDivision(subRoads.Right, divisions - 1);
        }
        #endregion
    }

    static Curve ApproximateCircularArc(float3 q0, float3 q1, float3 q2)
    {
        // reference: https://pomax.github.io/bezierinfo/#circles_cubic
        float k = 0.551785f;
        float3 c1 = Vector3.Lerp(q0, q1, k);
        float3 c2 = Vector3.Lerp(q2, q1, k);

        return new(new BezierCurve(q0, c1, c2, q2));
    }

    static float3 AlignPivotByStart(BuildTargets startTarget, float3 p)
    {
        Assert.IsNotNull(startTarget);
        float oldY = p.y;
        if (startTarget.TangentAssigned)
            p = math.project(p - startTarget.Pos, startTarget.Tangent) + startTarget.Pos;
        else
            return p;
        p.y = oldY;
        pivotPos = p;
        pivotAligned = true;
        return p;
    }

    static float3 AlignPivotEnd(BuildTargets endTarget, float3 p)
    {
        Assert.IsNotNull(endTarget);
        float oldY = p.y;
        if (endTarget.TangentAssigned)
            p = math.project(p - endTarget.Pos, endTarget.Tangent) + endTarget.Pos;
        else
            return p;
        p.y = oldY;
        pivotPos = p;
        pivotAligned = true;
        return p;
    }

    public static void ConnectRoadStartToNodes(Intersection ix, int index, Road road)
    {
        Assert.IsTrue(Game.Roads.ContainsKey(road.Id));
        List<Node> nodes = ix.WalkNodes(index, road.LaneCount);
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            node.OutLane = road.Lanes[i];
            road.Lanes[i].StartNode = node;
        }

        road.StartIntersection.AddRoad(road, Direction.Out);
        IntersectionUtil.EvaluatePaths(road.StartIntersection);
        IntersectionUtil.EvaluateOutline(road.StartIntersection);
    }

    public static void ConnectRoadEndToNodes(Intersection ix, int index, Road road)
    {
        Assert.IsTrue(Game.Roads.ContainsKey(road.Id));
        List<Node> nodes = ix.WalkNodes(index, road.LaneCount);
        for (int i = 0; i < nodes.Count; i++)
        {
            Node node = nodes[i];
            node.InLane = road.Lanes[i];
            road.Lanes[i].EndNode = node;
        }
        road.EndIntersection.AddRoad(road, Direction.In);
        IntersectionUtil.EvaluatePaths(road.EndIntersection);
        IntersectionUtil.EvaluateOutline(road.EndIntersection);
    }

    static void RegisterNodes(Road road)
    {
        foreach (Lane lane in road.Lanes)
        {
            Game.RegisterNode(lane.StartNode);
            Game.RegisterNode(lane.EndNode);
        }
    }

    public static void IncreaseElevation()
    {
        if (Elevation + Constants.ElevationStep <= Constants.MaxElevation)
            Elevation += Constants.ElevationStep;
    }

    public static void DecreaseElevation()
    {
        if (Elevation - Constants.ElevationStep >= Constants.MinElevation)
            Elevation -= Constants.ElevationStep;
    }
}
