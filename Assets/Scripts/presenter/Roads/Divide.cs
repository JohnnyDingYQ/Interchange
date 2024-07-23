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
        road.Curve.GetNearestPoint(new(clickPos, Vector3.down), out float distanceOnCurve);
        return DivideRoad(road, distanceOnCurve);
    }

    public static SubRoads DivideRoad(Road road, float distanceOnCurve)
    {
        Assert.IsTrue(Game.Roads.ContainsKey(road.Id));
        int laneCount = road.LaneCount;
        road.Curve.Split(distanceOnCurve, out Curve left, out Curve right);
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
        Build.ConnectRoadStartToNodes(leftRoad.EndIntersection, 0, rightRoad);

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
                lane.StartNode.OutLane = laneLeft;

                laneRight.EndNode = lane.EndNode;
                lane.EndNode.InLane = laneRight;

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
                laneLeft.InnerPath = new(laneLeft.InnerPath.Curve, laneLeft.StartVertex, laneLeft.EndVertex);
                laneRight.InnerPath = new(laneRight.InnerPath.Curve, laneRight.StartVertex, laneRight.EndVertex);
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