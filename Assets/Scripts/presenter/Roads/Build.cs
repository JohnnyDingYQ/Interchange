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
    private static bool startAssigned, pivotAssigned;
    private static Zone startZone;
    static readonly SupportLine supportLine = new();
    public static int LaneCount { get; set; }
    public static BuildTargets StartTarget { get; set; }
    public static BuildTargets EndTarget { get; set; }
    public static Action<SupportLine> SupportedLineUpdated;
    public static bool BuildsGhostRoad { get; set; }
    public static List<uint> GhostRoads { get; private set; }
    public static bool ParallelBuildOn { get; set; }
    public static float ParallelSpacing { get; set; }
    public static int Elevation { get; set; }
    public static bool ReplaceSuggestionOn { get; set; }
    public static bool StraightMode { get; set; }
    static Build()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        BuildsGhostRoad = true;
        GhostRoads = new();
        StraightMode = false;
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
        StartTarget = null;
        EndTarget = null;
        ReplaceSuggestionOn = false;
        StraightMode = false;
        RemoveAllGhostRoads();
    }

    public static SupportLine SetSupportLines()
    {
        supportLine.Segment1Set = false;
        supportLine.Segment2Set = false;
        if (startAssigned)
        {
            float3 startPoint = StartTarget.Snapped ? StartTarget.Pos : StartTarget.ClickPos;
            float3 pivotPoint = pivotPos;
            startPoint.y = 0;
            pivotPoint.y = 0;
            supportLine.Segment1Set = true;
            supportLine.Segment1 = new(startPoint, pivotPoint);
        }
        if (pivotAssigned && EndTarget != null)
        {
            float3 endPoint = EndTarget.Snapped ? EndTarget.Pos : EndTarget.ClickPos;
            float3 pivotPoint = pivotPos;
            endPoint.y = 0;
            pivotPoint.y = 0;
            supportLine.Segment2Set = true;
            supportLine.Segment2 = new(pivotPoint, endPoint);
        }
        SupportedLineUpdated?.Invoke(supportLine);
        return supportLine;
    }

    public static void RemoveAllGhostRoads()
    {
        foreach (uint id in GhostRoads)
            Game.RemoveRoad(Game.Roads[id]);
        GhostRoads.Clear();
    }

    static void UpdateBuildTargetsAndPivot(float3 pos)
    {
        if (!startAssigned)
            HandlePosAsStart(pos);
        else if (startAssigned && !pivotAssigned)
            pivotPos = AlignPivot(StartTarget, pos);
        else if (startAssigned && pivotAssigned)
            HandlePosAsEnd(pos);

        void SetupReplaceSuggestion(Road road, float distOnRoad)
        {
            float distance = math.distance(road.Curve.EvaluateDistancePos(distOnRoad), pos);
            bool isOnLeftSide = math.cross(
                road.Curve.EvaluateDistanceTangent(distOnRoad),
                road.Curve.EvaluateDistancePos(distOnRoad) - pos).y > 0;
            float offset = isOnLeftSide ? distance : -distance;
            StartTarget = Snapping.Snap(road.StartPos + offset * road.Curve.StartNormal, LaneCount, Side.Both);
            EndTarget = Snapping.Snap(road.EndPos + offset * road.Curve.EndNormal, LaneCount, Side.Both);

        }

        void HandlePosAsStart(float3 pos)
        {
            Road road = Game.HoveredRoad;
            float distOnRoad = road != null ? road.GetNearestDistance(pos) : 0;
            if (Game.HoveredRoad == null || !road.DistanceBetweenVertices(distOnRoad))
            {
                StartTarget = Snapping.Snap(pos, LaneCount, Side.Start);
                EndTarget = null;
                ReplaceSuggestionOn = false;
            }
            else
            {
                ReplaceSuggestionOn = true;
                SetupReplaceSuggestion(road, distOnRoad);
            }
        }

        static void HandlePosAsEnd(float3 pos)
        {
            if (StraightMode && StartTarget.Snapped)
            {
                EndTarget = Snapping.Snap(AlignPivot(StartTarget, pos), LaneCount, Side.End);
            }
            else
            {
                EndTarget = Snapping.Snap(pos, LaneCount, Side.End);
                if (EndTarget.Snapped)
                {
                    if (StraightMode)
                    {
                        float3 startPos = EndTarget.Pos - math.length(StartTarget.Pos - EndTarget.Pos) * EndTarget.Tangent;
                        StartTarget = Snapping.Snap(startPos, LaneCount, Side.Start);
                    }
                    else if (StartTarget.Snapped)
                        pivotPos = Vector3.Lerp(StartTarget.Pos, EndTarget.Pos, 0.5f);
                    else
                        pivotPos = AlignPivot(EndTarget, pivotPos);
                }
            }
            if (StraightMode)
                pivotPos = Vector3.Lerp(StartTarget.Pos, EndTarget.Pos, 0.5f);
        }
    }

    public static void HandleHover(float3 hoverPos)
    {
        RemoveAllGhostRoads();
        UpdateBuildTargetsAndPivot(hoverPos);
        SetSupportLines();
        BuildGhostRoad();
    }
    public static void BuildGhostRoad()
    {
        if (startAssigned && pivotAssigned)
            if (ParallelBuildOn)
                BuildParallelRoads(StartTarget, pivotPos, EndTarget, BuildMode.Ghost);
            else
                BuildRoads(StartTarget, pivotPos, EndTarget, BuildMode.Ghost);
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
        RemoveAllGhostRoads();
        UpdateBuildTargetsAndPivot(clickPos);
        if (ReplaceSuggestionOn)
        {
            roads = BuildSuggested();
            ResetSelection();
            return roads;
        }
        if (!startAssigned)
        {
            startZone = Game.HoveredZone;
            startAssigned = true;
            return null;
        }
        if (!pivotAssigned)
        {
            pivotAssigned = true;
            return null;
        }
        if (ParallelBuildOn)
            roads = BuildParallelRoads(StartTarget, pivotPos, EndTarget, BuildMode.Actual);
        else
            roads = BuildRoads(StartTarget, pivotPos, EndTarget, BuildMode.Actual);
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
            ReplaceSuggestionOn = false;
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
        if (RoadIsTooBent() || BadSegmentRatio())
            return null;
        Road road = new(GetCurve(), LaneCount);
        if (road.HasLaneShorterThanMinLaneLength())
            return null;
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

        List<Road> roads = new() { road };

        RegisterNodes(road);

        if (road.IsGhost)
            GhostRoads.AddRange(roads.Select(r => r.Id));
        else
        {
            AutoDivideRoad();
            HandleZoneConnection();
        }

        Game.UpdateIntersectionRoads(road.StartIntersection);
        Game.UpdateIntersectionRoads(roads.Last().EndIntersection);
        return roads;

        # region extracted funcitons
        static float GetLongestLaneLength(Road road)
        {
            float length = 0;
            foreach (Lane lane in road.Lanes)
                length = Math.Max(length, lane.Length);
            return length;
        }

        void AutoDivideRoad()
        {
            float longestLength = GetLongestLaneLength(road);
            if (longestLength <= Constants.MaxLaneLength)
                return;
            int divisions = 2;
            while (longestLength / divisions > Constants.MaxLaneLength)
                divisions++;
            RecursiveRoadDivision(road, divisions, roads);
        }

        static void RecursiveRoadDivision(Road road, int divisions, List<Road> roads)
        {
            if (divisions == 1)
                return;
            SubRoads subRoads = Divide.DivideRoad(road, road.Curve.Length / divisions);
            roads.Remove(road);
            roads.Add(subRoads.Left);
            roads.Add(subRoads.Right);
            RecursiveRoadDivision(subRoads.Right, divisions - 1, roads);
        }

        void HandleZoneConnection()
        {
            if (roads != null && LaneCount == 1)
            {
                if (startZone?.Type == ZoneType.Source && roads.First().StartPos.y == Constants.MinElevation)
                {
                    startZone?.AddVertex(roads.First().Lanes.Single().StartVertex);
                    roads.Last().EndIntersection.OutRoads
                        .Where(r => r.LaneCount == 1)
                        .Select(r => r.Lanes.Single()).ToList()
                        .ForEach(l => startZone.RemoveVertex(l.StartVertex));
                }
                if (Game.HoveredZone?.Type == ZoneType.Target && roads.Last().EndPos.y == Constants.MinElevation)
                {
                    Game.HoveredZone?.AddVertex(roads.Last().Lanes.Single().EndVertex);
                    roads.First().StartIntersection.InRoads
                        .Where(r => r.LaneCount == 1)
                        .Select(r => r.Lanes.Single()).ToList()
                        .ForEach(l => Game.HoveredZone.RemoveVertex(l.EndVertex));
                }
            }
        }
        #endregion
    }

    static Curve GetCurve()
    {
        // reference: https://pomax.github.io/bezierinfo/#circles_cubic
        float k = 0.551785f;
        Assert.IsTrue(startAssigned && pivotAssigned);
        if (StartTarget.Snapped && EndTarget.Snapped)
        {
            float length = math.length(math.project(pivotPos - StartTarget.Pos, StartTarget.Tangent));
            float3 q0 = StartTarget.Pos;
            float3 q3 = EndTarget.Pos;
            float3 q1 = Vector3.Lerp(q0, q0 + StartTarget.Tangent * length, k);
            float3 q2 = Vector3.Lerp(EndTarget.Pos - EndTarget.Tangent * length, EndTarget.Pos, k);
            return new(new(q0, q1, q2, q3));
        }
        return ApproximateCircularArc(StartTarget.Pos, pivotPos, EndTarget.Pos);

        Curve ApproximateCircularArc(float3 q0, float3 q1, float3 q2)
        {
            float3 c1 = Vector3.Lerp(q0, q1, k);
            float3 c2 = Vector3.Lerp(q2, q1, k);

            return new(new BezierCurve(q0, c1, c2, q2));
        }
    }


    static float3 AlignPivot(BuildTargets startTarget, float3 p)
    {
        Assert.IsNotNull(startTarget);
        float oldY = p.y;
        if (startTarget.TangentAssigned)
            p = math.project(p - startTarget.Pos, startTarget.Tangent) + startTarget.Pos;
        else
            return p;
        p.y = oldY;
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
