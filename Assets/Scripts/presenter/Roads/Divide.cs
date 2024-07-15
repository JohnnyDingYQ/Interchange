using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public static class Divide
{
    public static SubRoads HandleDivideCommand(Road road, float3 clickPos)
    {
        if (road == null)
            throw new InvalidOperationException("Road to divide cannot be null");
        float interpolation = road.GetNearestInterpolation(clickPos);
        if (RoadIsDividable(road, interpolation))
            return DivideRoad(road, road.GetNearestInterpolation(clickPos));
        return null;
    }


    public static bool RoadIsDividable(Road road, float t)
    {
        return road.SplitIsValid(t);
    }

    public static SubRoads DivideRoad(Road road, float t)
    {
        Assert.IsTrue(Game.Roads.ContainsKey(road.Id));
        int laneCount = road.LaneCount;
        road.BezierSeries.Split(t, out BezierSeries left, out BezierSeries right);
        Road leftRoad = new(left, laneCount)
        {
            IsGhost = road.IsGhost
        };
        Road rightRoad = new(right, laneCount)
        {
            IsGhost = road.IsGhost
        };
        if (leftRoad.HasLaneShorterThanMinLaneLength() || rightRoad.HasLaneShorterThanMinLaneLength())
            return null;
        OperateNodes();
        OperateIntersections();
        OperateVertices();
        Game.RegisterRoad(leftRoad);
        Game.RegisterRoad(rightRoad);
        OperateOutline();
        Game.RemoveRoad(road, RoadRemovalOption.Divide);
        List<Node> leftNodes = new();
        for (int i = 0; i < leftRoad.LaneCount; i++)
            leftNodes.Add(leftRoad.Lanes[i].EndNode);
        Build.ConnectRoadStartToNodes(leftNodes, rightRoad);

        return new SubRoads(leftRoad, rightRoad);

        void OperateIntersections()
        {
            leftRoad.StartIntersection = road.StartIntersection;
            rightRoad.StartIntersection = leftRoad.EndIntersection;
            rightRoad.EndIntersection = road.EndIntersection;

            leftRoad.StartIntersection.AddRoad(leftRoad, Direction.Out);
            rightRoad.EndIntersection.AddRoad(rightRoad, Direction.In);
        }

        void OperateNodes()
        {
            for (int i = 0; i < leftRoad.LaneCount; i++)
            {
                Lane laneLeft = leftRoad.Lanes[i];
                Lane laneRight = rightRoad.Lanes[i];
                Lane lane = road.Lanes[i];

                laneLeft.StartNode = lane.StartNode;
                lane.StartNode.AddLane(laneLeft, Direction.Out);

                laneRight.EndNode = lane.EndNode;
                lane.EndNode.AddLane(laneRight, Direction.In);

                Game.RegisterNode(laneLeft.EndNode);
            }
        }

        void OperateVertices()
        {
            for (int i = 0; i < leftRoad.LaneCount; i++)
            {
                Lane laneLeft = leftRoad.Lanes[i];
                Lane laneRight = rightRoad.Lanes[i];
                Lane lane = road.Lanes[i];
                laneLeft.StartVertex = lane.StartVertex;
                laneRight.EndVertex = lane.EndVertex;
                laneLeft.StartVertex.SetOwnerLane(laneLeft, Side.Start);
                laneRight.EndVertex.SetOwnerLane(laneRight, Side.End);
                laneLeft.InnerPath = new(laneLeft.InnerPath.BezierSeries, laneLeft.StartVertex, laneLeft.EndVertex);
                laneRight.InnerPath = new(laneRight.InnerPath.BezierSeries, laneRight.StartVertex, laneRight.EndVertex);
            }
        }

        void OperateOutline()
        {
            leftRoad.LeftOutline.Start = road.LeftOutline.Start;
            leftRoad.RightOutline.Start = road.RightOutline.Start;
            rightRoad.LeftOutline.End = road.LeftOutline.End;
            rightRoad.RightOutline.End = road.RightOutline.End;

            leftRoad.LeftOutline.Start.Add(leftRoad.LeftOutline.Mid.First());
            leftRoad.RightOutline.Start.Add(leftRoad.RightOutline.Mid.First());
            rightRoad.LeftOutline.End.Insert(0, rightRoad.LeftOutline.Mid.Last());
            rightRoad.RightOutline.End.Insert(0, rightRoad.RightOutline.Mid.Last());
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