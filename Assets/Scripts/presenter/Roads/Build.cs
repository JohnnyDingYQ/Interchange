using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public static class Build
{
    private static float3 pivotPos;
    private static bool startAssigned, pivotAssigned;
    public static Zone StartZone { get; private set; }
    static readonly SupportLine supportLine = new();
    private static int laneCount;
    public static int LaneCount { get => laneCount; set { laneCount = value; ResetSelection(); } }
    public static BuildTargets StartTarget { get; set; }
    public static BuildTargets EndTarget { get; set; }
    public static event Action<SupportLine> SupportedLineUpdated;
    public static bool BuildsGhostRoad { get; set; }
    public static List<uint> GhostRoads { get; private set; }
    public static bool ParallelBuildOn { get; set; }
    public static float ParallelSpacing { get; set; }
    public static int Elevation { get; set; }
    public static bool StraightMode { get; set; }
    public static bool ContinuousBuilding { get; set; }
    public static bool ReplaceSuggestionOn
    {
        get => !startAssigned && !pivotAssigned && StartTarget != null && EndTarget != null &&
        StartTarget.IsReplaceSuggestion && EndTarget.IsReplaceSuggestion && Game.HoveredRoad != null;
    }

    static Build()
    {
        GhostRoads = new();
        LaneCount = 1;
        startAssigned = false;
        pivotAssigned = false;
        BuildsGhostRoad = true;
        StraightMode = false;
        ParallelSpacing = Constants.DefaultParallelSpacing;
        Game.RoadRemoved += RoadRemoved;
    }

    public static void Reset()
    {
        ResetSelection();
        RemoveAllGhostRoads();
        BuildsGhostRoad = true;
        GhostRoads = new();
        ParallelBuildOn = false;
        ParallelSpacing = Constants.DefaultParallelSpacing;
        ContinuousBuilding = false;
        LaneCount = 1;
    }

    static void RoadRemoved(Road road)
    {
        if (road.IsGhost || StartTarget == null)
            return;
        if (road.EndIntersection == StartTarget.Intersection)
            ResetSelection();
    }

    public static bool StartAssigned()
    {
        return startAssigned;
    }

    public static void UndoBuildCommand()
    {
        if (pivotAssigned)
        {
            pivotAssigned = false;
            return;
        }
        if (startAssigned)
        {
            ResetSelection();
        }
    }

    public static void ResetSelection()
    {
        startAssigned = false;
        pivotAssigned = false;
        StartTarget = null;
        EndTarget = null;
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
            float distance = math.distance(road.Curve.EvaluatePosition(distOnRoad), new float3(pos.x, 0, pos.z));
            bool isOnLeftSide = math.cross(
                road.Curve.EvaluateTangent(distOnRoad),
                road.Curve.EvaluatePosition(distOnRoad) - pos).y > 0;
            float offset = isOnLeftSide ? distance : -distance;
            StartTarget = Snapping.Snap(road.StartPos + offset * road.Curve.StartNormal, LaneCount, Side.Both);
            EndTarget = Snapping.Snap(road.EndPos + offset * road.Curve.EndNormal, LaneCount, Side.Both);

            if (ReplaceIsValid(road))
            {
                StartTarget.IsReplaceSuggestion = true;
                EndTarget.IsReplaceSuggestion = true;
            }
            else
            {
                StartTarget = null;
                EndTarget = null;
            }
        }

        bool ReplaceIsValid(Road road)
        {
            if (StartTarget.Intersection == null || EndTarget.Intersection == null)
                return false;
            if (StartTarget.Intersection != road.StartIntersection || EndTarget.Intersection != road.EndIntersection)
                return false;
            bool startGood = true;
            bool endGood = true;
            if (StartTarget.Intersection.IsMajorRoad(road))
            {
                HashSet<int> connected = new();
                foreach (Node node in road.Lanes.Select(l => l.StartNode))
                    if (node.InLane != null)
                        connected.Add(node.NodeIndex);
                for (int i = StartTarget.Offset; i < StartTarget.Offset + LaneCount; i++)
                    connected.Remove(i);
                if (connected.Count != 0)
                    startGood = false;
            }

            if (EndTarget.Intersection.IsMajorRoad(road))
            {
                HashSet<int> connected = new();
                foreach (Node node in road.Lanes.Select(l => l.EndNode))
                    if (node.OutLane != null)
                        connected.Add(node.NodeIndex);
                for (int i = EndTarget.Offset; i < EndTarget.Offset + LaneCount; i++)
                    connected.Remove(i);
                if (connected.Count != 0)
                    endGood = false;
            }
            return startGood && endGood;
        }

        void HandlePosAsStart(float3 pos)
        {
            Road road = Game.HoveredRoad;
            float distOnRoad = road != null ? road.GetNearestDistance(pos) : 0;
            if (Game.HoveredRoad == null || !road.DistanceBetweenVertices(distOnRoad))
            {
                StartTarget = Snapping.Snap(pos, LaneCount, Side.Start);
                EndTarget = null;
            }
            else if (Game.HoveredRoad != null)
            {
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
                    {
                        if (MyNumerics.Get2DVectorsIntersection(
                            StartTarget.Pos.xz, StartTarget.Tangent.xz,
                            EndTarget.Pos.xz, -EndTarget.Tangent.xz,
                            out Vector2 pt))
                            pivotPos = new(pt.x, (StartTarget.Pos.y + EndTarget.Pos.y) / 2, pt.y);
                        else
                            pivotPos = Vector3.Lerp(StartTarget.Pos, EndTarget.Pos, 0.5f);
                    }
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
                BuildRoad(StartTarget, pivotPos, EndTarget, BuildMode.Ghost);
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
            roads = new() { BuildSuggested() };
            ResetSelection();
            return roads;
        }
        if (!startAssigned)
        {
            StartZone = Game.HoveredZone;
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
            roads = new() { BuildRoad(StartTarget, pivotPos, EndTarget, BuildMode.Actual) };

        if (!EndTarget.Snapped && ContinuousBuilding)
            ContinueBuild(roads.First());
        else
            ResetSelection();

        if (roads != null)
            CarScheduler.FindNewConnection();

        Assert.IsTrue(Game.GameSave.IPersistableAreInDict());
        
        return roads;

        static Road BuildSuggested()
        {
            float roadMidIndex = StartTarget.Offset + (float)LaneCount / 2;
            float currMidIndex = StartTarget.SelectedRoad.Lanes.First().StartNode.NodeIndex
                + (float)StartTarget.SelectedRoad.LaneCount / 2;
            float offsetDist = (currMidIndex - roadMidIndex) * Constants.LaneWidth;
            Road road = new(StartTarget.SelectedRoad.Curve.Offset(offsetDist), LaneCount);
            Game.RemoveRoad(StartTarget.SelectedRoad, RoadRemovalOption.Replace);

            road = ProcessRoad(road, StartTarget, EndTarget);
            foreach (Node node in StartTarget.Intersection.Nodes)
                if (!road.Lanes.Contains(node.OutLane) && node.InLane == null)
                    Game.RemoveNode(node);

            foreach (Node node in EndTarget.Intersection.Nodes)
                if (!road.Lanes.Contains(node.InLane) && node.OutLane == null)
                    Game.RemoveNode(node);
            return road;
        }

        static void ContinueBuild(Road road)
        {
            ResetSelection();
            StartTarget = Snapping.Snap(road.EndPos, LaneCount, Side.Start);
            startAssigned = true;
            StartZone = Game.HoveredZone;
        }
    }

    static Road BuildRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget, BuildMode buildMode)
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
        offsetted = offsetted.Reverse();
        BuildTargets startTargetParallel = Snapping.Snap(offsetted.StartPos, LaneCount, Side.Start);
        BuildTargets endTargetParallel = Snapping.Snap(offsetted.EndPos, LaneCount, Side.End);
        Road parallel = new(offsetted, LaneCount) { IsParallel = true };
        if (buildMode == BuildMode.Ghost)
        {
            road.IsGhost = true;
            parallel.IsGhost = true;
        }
        List<Road> roads = new()
        {
            ProcessRoad(road, startTarget, endTarget),
            ProcessRoad(parallel, startTargetParallel, endTargetParallel)
        };
        return roads;
    }

    static Road InitRoad(BuildTargets startTarget, float3 pivotPos, BuildTargets endTarget)
    {
        float3 startPos = startTarget.Pos;
        float3 endPos = endTarget.Pos;
        if (RoadIsTooBent())
            return null;
        Road road = new(GetCurve(), LaneCount);
        if (road.HasLaneShorterThanMinLaneLength() || RoadIsTooSteep())
            return null;
        int segmemtCount = (int)MathF.Ceiling(road.Curve.Length / Constants.MaxRoadCurveLength);
        // road.Curve = road.Curve.SplitInToSegments(segmemtCount);
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

        bool RoadIsTooSteep()
        {
            float heightDiff = Math.Abs(startPos.y - endPos.y);
            if (heightDiff / road.Length > Constants.MaxRampGrade / 100)
                return true;
            return false;
        }
    }

    static Road ProcessRoad(Road road, BuildTargets startTarget, BuildTargets endTarget)
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

        RegisterNodes(road);

        if (road.IsGhost)
            GhostRoads.Add(road.Id);
        else
            HandleZoneConnection();

        Game.UpdateIntersection(road.StartIntersection);
        Game.UpdateIntersection(road.EndIntersection);
        return road;

        # region extracted funcitons
        void HandleZoneConnection()
        {
            if (road.StartPos.y == Constants.MinElevation)
            {
                if (StartZone != null && !road.IsParallel)
                    StartZone.AddVertex(road.Lanes.Select(l => l.StartVertex));
                if (Game.HoveredZone != null && road.IsParallel)
                    Game.HoveredZone.AddVertex(road.Lanes.Select(l => l.StartVertex));
            }
            if (road.EndPos.y == Constants.MinElevation)
            {
                if (Game.HoveredZone != null && !road.IsParallel)
                    Game.HoveredZone.AddVertex(road.Lanes.Select(l => l.EndVertex));
                if (StartZone != null && road.IsParallel)
                    StartZone.AddVertex(road.Lanes.Select(l => l.EndVertex));
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


    static float3 AlignPivot(BuildTargets buildTarget, float3 p)
    {
        Assert.IsNotNull(buildTarget);
        float oldY = p.y;
        if (buildTarget.TangentAssigned)
        {
            float3 projection = math.project(p - buildTarget.Pos, buildTarget.Tangent);
            if (math.dot(buildTarget.Tangent, projection) < 0 && buildTarget.Side == Side.Start)
                projection = 0;
            p = projection + buildTarget.Pos;
        }
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
        IntersectionUtil.EvaluateEdges(road.StartIntersection);
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
        IntersectionUtil.EvaluateEdges(road.EndIntersection);
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