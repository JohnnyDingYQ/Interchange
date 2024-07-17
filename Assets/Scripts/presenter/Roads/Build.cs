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
    public static int LaneCount { get; set; }
    public static BuildTargets StartTarget;
    public static BuildTargets EndTarget;
    public static List<Tuple<float3, float3>> SupportLines { get; }
    public static bool BuildsGhostRoad { get; set; }
    public static bool EnforcesTangent { get; set; }
    public static List<uint> GhostRoads { get; private set; }
    public static bool ParallelBuildOn { get; set; }
    public static float ParallelSpacing { get; set; }
    public static int Elevation { get; set; }

    static Build()
    {
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        pivotAligned = false;
        SupportLines = new();
        BuildsGhostRoad = true;
        EnforcesTangent = true;
        GhostRoads = new();
        ParallelSpacing = Constants.DefaultParallelSpacing;
    }

    public static void Reset()
    {
        ResetSelection();
        RemoveAllGhostRoads();
        EnforcesTangent = true;
        BuildsGhostRoad = true;
        GhostRoads = new();
        ParallelBuildOn = false;
        ParallelSpacing = Constants.DefaultParallelSpacing;
    }

    public static void ResetSelection()
    {
        startAssigned = false;
        pivotAssigned = false;
        pivotAligned = false;
        StartTarget = null;
        EndTarget = null;
        pivotPos = 0;
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
        {
            Assert.IsTrue(Game.Roads.ContainsKey(id));
            Game.RemoveRoad(Game.Roads[id]);
        }
        GhostRoads.Clear();
    }

    public static void BuildGhostRoad(float3 endTargetClickPos)
    {
        RemoveAllGhostRoads();
        EndTarget = Snapping.Snap(endTargetClickPos, LaneCount);
        if (ParallelBuildOn)
            BuildParallelRoads(StartTarget, pivotPos, EndTarget, BuildMode.Ghost);
        else
            BuildRoads(StartTarget, pivotPos, EndTarget, BuildMode.Ghost);
    }

    public static void HandleHover(float3 hoverPos)
    {
        if (!startAssigned)
            StartTarget = Snapping.Snap(hoverPos, LaneCount);
        if (startAssigned && !pivotAssigned)
            pivotPos = hoverPos;
        if (EnforcesTangent && !pivotAssigned && startAssigned)
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
        if (!startAssigned)
        {
            startAssigned = true;
            StartTarget = Snapping.Snap(clickPos, LaneCount);
            return null;
        }
        else if (!pivotAssigned)
        {
            pivotAssigned = true;
            pivotPos = clickPos;
            if (EnforcesTangent)
                AlignPivotByStart(StartTarget, pivotPos);
            return null;
        }
        else
        {
            RemoveAllGhostRoads();
            EndTarget = Snapping.Snap(clickPos, LaneCount);
            List<Road> roads;
            if (ParallelBuildOn)
                roads = BuildParallelRoads(StartTarget, pivotPos, EndTarget, BuildMode.Actual);
            else
                roads = BuildRoads(StartTarget, pivotPos, EndTarget, BuildMode.Actual);
            ResetSelection();
            return roads;
        }
    }

    static List<Road> BuildRoads(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget, BuildMode buildMode)
    {
        if (buildMode == BuildMode.Actual)
            PreprocessSnap();
        Road road = InitRoad(startTarget, pivotPos, endTarget);
        if (road == null)
            return null;
        if (buildMode == BuildMode.Ghost)
            road.IsGhost = true;
        return ProcessRoad(road, startTarget, endTarget);

        void PreprocessSnap()
        {
            if (!startTarget.Snapped && startTarget.DivideIsValid)
            {
                Divide.HandleDivideCommand(startTarget.SelectedRoad, startTarget.ClickPos);
                startTarget = Snapping.Snap(startTarget.ClickPos, LaneCount);
            }
            if (!endTarget.Snapped && endTarget.DivideIsValid)
            {
                Divide.HandleDivideCommand(endTarget.SelectedRoad, endTarget.ClickPos);
                endTarget = Snapping.Snap(endTarget.ClickPos, LaneCount);
            }
            if (!startTarget.Snapped && startTarget.CombineAndDivideIsValid)
            {
                Combine.CombineRoads(startTarget.Intersection);
                Divide.HandleDivideCommand(startTarget.SelectedRoad, startTarget.ClickPos);
                startTarget = Snapping.Snap(startTarget.ClickPos, LaneCount);
            }
        }
    }

    static List<Road> BuildParallelRoads(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget, BuildMode buildMode)
    {
        Road road = InitRoad(startTarget, pivotPos, endTarget);
        if (road == null)
            return null;
        BezierSeries offsetted = road.BezierSeries.Offset(ParallelSpacing);
        BuildTargets startTargetParallel = Snapping.Snap(offsetted.EvaluatePosition(0), LaneCount);
        BuildTargets endTargetParallel = Snapping.Snap(offsetted.EvaluatePosition(1), LaneCount);
        offsetted.Reverse();
        Road parallel = new(offsetted, LaneCount);
        if (buildMode == BuildMode.Ghost)
        {
            road.IsGhost = true;
            parallel.IsGhost = true;
        }
        List<Road> roads = ProcessRoad(road, startTarget, endTarget);
        roads.AddRange(ProcessRoad(parallel, endTargetParallel, startTargetParallel));
        return roads;
    }

    static Road InitRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget)
    {
        float3 startPos = startTarget.Pos;
        float3 endPos = endTarget.Pos;
        if (EnforcesTangent && !pivotAligned)
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
        List<Node> startNodes = startTarget.Nodes;
        List<Node> endNodes = endTarget.Nodes;
        if (road.HasLaneShorterThanMinLaneLength())
            return null;
        if (startTarget.Snapped)
            road.StartIntersection = startTarget.Intersection;
        if (endTarget.Snapped)
            road.EndIntersection = endTarget.Intersection;
        Game.RegisterRoad(road);

        if (startTarget.Snapped)
            ConnectRoadStartToNodes(startNodes, road);
        else
            IntersectionUtil.EvaluateOutline(road.StartIntersection);

        if (endTarget.Snapped)
            ConnectRoadEndToNodes(endNodes, road);
        else
            IntersectionUtil.EvaluateOutline(road.EndIntersection);

        List<Road> resultingRoads = new() { road };

        RegisterUnregisteredNodes(road);
        if (!road.IsGhost)
        {
            ReplaceExistingRoad();
            // move statement below out of this block to divide ghost road
            AutoDivideRoad(road);
        }

        if (road.IsGhost)
            GhostRoads.AddRange(resultingRoads.Select(r => r.Id));
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
            SubRoads subRoads = Divide.DivideRoad(road, 1 / (float)divisions);
            resultingRoads.Remove(road);
            resultingRoads.Add(subRoads.Left);
            resultingRoads.Add(subRoads.Right);
            RecursiveRoadDivision(subRoads.Right, divisions - 1);
        }

        void ReplaceExistingRoad()
        {
            if (!(startTarget.Snapped && endTarget.Snapped))
                return;
            List<Vertex> startV = new();
            foreach (Node n in startNodes)
                foreach (Lane l in n.GetLanes(Direction.Out))
                    startV.Add(l.StartVertex);
            List<Vertex> endV = new();
            foreach (Node n in endNodes)
                foreach (Lane l in n.GetLanes(Direction.In))
                    endV.Add(l.EndVertex);

            HashSet<Road> roads = new();
            foreach (Vertex start in startV)
                foreach (Vertex end in endV)
                {
                    List<Path> paths = Graph.ShortestPathAStar(start, end)?.ToList();
                    if (paths != null)
                        foreach (Path p in paths)
                        {
                            roads.Add(p.Source.Road);
                            roads.Add(p.Target.Road);
                        }
                }
            foreach (Road r in roads)
                if (r != road)
                    Game.RemoveRoad(r);
        }
        #endregion
    }

    static BezierSeries ApproximateCircularArc(float3 q0, float3 q1, float3 q2)
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

    public static void ConnectRoadStartToNodes(List<Node> nodes, Road road)
    {
        Assert.IsTrue(nodes.First().Intersection == road.StartIntersection);
        Assert.IsTrue(Game.Roads.ContainsKey(road.Id));
        for (int i = 0; i < road.LaneCount; i++)
        {
            nodes[i].AddLane(road.Lanes[i], Direction.Out);
            road.Lanes[i].StartNode = nodes[i];
        }
        road.StartIntersection.AddRoad(road, Direction.Out);
        IntersectionUtil.EvaluatePaths(road.StartIntersection);
        IntersectionUtil.EvaluateOutline(road.StartIntersection);
    }

    public static void ConnectRoadEndToNodes(List<Node> nodes, Road road)
    {
        Assert.IsTrue(nodes.First().Intersection == road.EndIntersection);
        Assert.IsTrue(Game.Roads.ContainsKey(road.Id));
        for (int i = 0; i < road.LaneCount; i++)
        {
            nodes[i].AddLane(road.Lanes[i], Direction.In);
            road.Lanes[i].EndNode = nodes[i];
        }
        road.EndIntersection.AddRoad(road, Direction.In);
        IntersectionUtil.EvaluatePaths(road.EndIntersection);
        IntersectionUtil.EvaluateOutline(road.EndIntersection);
    }

    static void RegisterUnregisteredNodes(Road road)
    {
        foreach (Lane lane in road.Lanes)
        {
            if (!Game.HasNode(lane.StartNode))
                Game.RegisterNode(lane.StartNode);

            if (!Game.HasNode(lane.EndNode))
                Game.RegisterNode(lane.EndNode);
        }
    }

    public static bool RemoveRoad(Road road, RoadRemovalOption option)
    {
        Game.Roads.Remove(road.Id);
        foreach (Lane lane in road.Lanes)
        {
            List<Path> toRemove = new();
            if (option == RoadRemovalOption.Default)
            {
                Graph.RemoveVertex(lane.StartVertex);
                Graph.RemoveVertex(lane.EndVertex);
            }
            else
            {
                foreach (Path p in Game.Paths.Values)
                    if (p.Source == lane.StartVertex && p.Target == lane.EndVertex)
                        toRemove.Add(p);
            }
            toRemove.ForEach(p => Graph.RemovePath(p));

            if (option != RoadRemovalOption.Combine)
            {
                foreach (Node node in new List<Node>() { lane.StartNode, lane.EndNode })
                {
                    node.RemoveLane(lane);
                    if (node.Lanes.Count == 0 && !node.BelongsToPoint)
                    {
                        Game.Nodes.Remove(node.Id);
                        road.StartIntersection.RemoveNode(node);
                        road.EndIntersection.RemoveNode(node);
                    }
                }
            }
            Game.RemoveLane(lane);
        }
        road.StartIntersection.RemoveRoad(road, Direction.Out);
        road.EndIntersection.RemoveRoad(road, Direction.In);

        if (option != RoadRemovalOption.Combine)
            EvaluateIntersections(road);

        Game.InvokeRoadRemoved(road);
        return true;

        static void EvaluateIntersections(Road road)
        {
            if (!road.StartIntersection.IsEmpty())
            {
                IntersectionUtil.EvaluatePaths(road.StartIntersection);
                IntersectionUtil.EvaluateOutline(road.StartIntersection);
            }
            else
                Game.RemoveIntersection(road.StartIntersection);

            if (!road.EndIntersection.IsEmpty())
            {
                IntersectionUtil.EvaluatePaths(road.EndIntersection);
                IntersectionUtil.EvaluateOutline(road.EndIntersection);
            }
            else
                Game.RemoveIntersection(road.EndIntersection);
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
