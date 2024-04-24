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
        return DivideRoad(road, GetLocation(road, clickPos));
    }

    public static SeriesLocation GetLocation(Road road, float3 clickPos)
    {
        clickPos.y = 0;
        Ray ray = new(clickPos, Vector3.up);
        float distance = road.BezierSeries.GetNearestPoint(ray, out float3 p, out SeriesLocation location);
        return location;
    }

    public static SubRoads DivideRoad(Road road, float t)
    {
        if (!Game.Roads.ContainsKey(road.Id))
            return null;
        return DivideRoad(road, road.BezierSeries.InterpolationToLocation(t));
    }

    public static SubRoads DivideRoad(Road road, SeriesLocation location)
    {
        if (!Game.Roads.ContainsKey(road.Id))
            return null;
        int laneCount = road.LaneCount;
        road.BezierSeries.Split(location, out BezierSeries left, out BezierSeries right);
        Road leftRoad = new(left, laneCount);
        Road rightRoad = new(right, laneCount);
        if (leftRoad.HasLaneShorterThanMinimumLaneLength() || rightRoad.HasLaneShorterThanMinimumLaneLength())
            return null;
        Game.RegisterRoad(leftRoad);
        Game.RegisterRoad(rightRoad);
        List<Node> leftNodes = new();
        for (int i = 0; i < leftRoad.LaneCount; i++)
            leftNodes.Add(leftRoad.Lanes[i].EndNode);
        OperateNodes();
        OperateVertices();
        OperateOutline();
        Game.RemoveRoad(road, true);
        Build.ConnectRoadStartToNodes(leftNodes, rightRoad);

        return new SubRoads(leftRoad, rightRoad);

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