using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class DivideHandler
{
    public static SubRoads HandleDivideCommand(Road road, float3 clickPos)
    {
        if (road == null)
            throw new InvalidOperationException("Road to divide cannot be null");
        return DivideRoad(road, GetInterpolation(road, clickPos));
    }

    public static float GetInterpolation(Road road, float3 clickPos)
    {
        clickPos.y = 0;
        Ray ray = new(clickPos, Vector3.up);
        float distance = CurveUtility.GetNearestPoint(road.BezierCurve, ray, out float3 position, out float interpolation);
        return interpolation;
    }

    public static SubRoads DivideRoad(Road road, float interpolation)
    {
        if (!Game.Roads.ContainsKey(road.Id))
            return null;
        Game.RemoveRoad(road);
        int laneCount = road.LaneCount;
        interpolation = CurveUtility.GetDistanceToInterpolation(road.BezierCurve, road.Length * interpolation);
        CurveUtility.Split(road.BezierCurve, interpolation, out BezierCurve left, out BezierCurve right);
        Road LeftRoad = new(left, laneCount);
        Game.RegisterRoad(LeftRoad);
        Road RightRoad = new(right, laneCount);
        Game.RegisterRoad(RightRoad);
        OperateNodes();
        OperatePaths();
        OperateOutline();


        return new SubRoads(LeftRoad, RightRoad);

        void OperateNodes()
        {
            for (int i = 0; i < LeftRoad.LaneCount; i++)
            {
                Lane laneLeft = LeftRoad.Lanes[i];
                Lane laneRight = RightRoad.Lanes[i];
                Lane lane = road.Lanes[i];
                laneLeft.EndNode = laneRight.StartNode;
                laneLeft.EndNode.AddLane(laneLeft, Direction.In);
                Game.RegisterNode(laneLeft.EndNode);

                laneLeft.StartNode = lane.StartNode;
                lane.StartNode.AddLane(laneLeft, Direction.Out);

                laneRight.EndNode = lane.EndNode;
                lane.EndNode.AddLane(laneRight, Direction.In);

                Game.RegisterNode(laneLeft.StartNode);
                Game.RegisterNode(laneRight.EndNode);
            }
        }

        void OperatePaths()
        {
            List<Node> nodes = new();
            List<Lane> lanes = new();
            for (int i = 0; i < LeftRoad.LaneCount; i++)
            {
                Lane laneLeft = LeftRoad.Lanes[i];
                Lane laneRight = RightRoad.Lanes[i];
                Lane lane = road.Lanes[i];
                laneLeft.StartVertex = lane.StartVertex;
                Game.AddVertex(laneLeft.EndVertex);
                Game.AddVertex(laneRight.StartVertex);
                laneRight.EndVertex = lane.EndVertex;

                nodes.Add(laneLeft.EndNode);
                lanes.Add(laneRight);
            }
            InterRoad.BuildAllPaths(lanes, nodes, Direction.Out);
        }

        void OperateOutline()
        {
            LeftRoad.LeftOutline.Start = road.LeftOutline.Start;
            LeftRoad.RightOutline.Start = road.RightOutline.Start;
            RightRoad.LeftOutline.End = road.LeftOutline.End;
            RightRoad.RightOutline.End = road.RightOutline.End;

            LeftRoad.LeftOutline.Start.Add(LeftRoad.LeftOutline.Mid.First());
            LeftRoad.RightOutline.Start.Add(LeftRoad.RightOutline.Mid.First());
            RightRoad.LeftOutline.Start.Insert(0, RightRoad.LeftOutline.Mid.Last());
            RightRoad.RightOutline.Start.Insert(0, RightRoad.RightOutline.Mid.Last());

            LeftRoad.LeftOutline.End = InterRoad.GetOutLineAtTwoEnds(LeftRoad, Orientation.Left, Side.End);
            LeftRoad.RightOutline.End = InterRoad.GetOutLineAtTwoEnds(LeftRoad, Orientation.Right, Side.End);
            RightRoad.LeftOutline.Start = InterRoad.GetOutLineAtTwoEnds(RightRoad, Orientation.Left, Side.Start);
            RightRoad.RightOutline.Start = InterRoad.GetOutLineAtTwoEnds(RightRoad, Orientation.Right, Side.Start);
        }
    }
}

public class SubRoads
{
    public SubRoads(Road left, Road right)
    {
        Left = left;
        Right = right;
    }
    public Road Left { get; set; }
    public Road Right { get; set; }
}