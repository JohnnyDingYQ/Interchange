using UnityEngine;
using System.Linq;

public static class Combine
{

    public static bool CombineIsValid(Intersection ix)
    {
        return ix.IsForLaneChangeOnly();
    }

    public static Road CombineRoads(Intersection ix)
    {
        if (!CombineIsValid(ix))
            return null;
        Road left = ix.InRoads.Single();
        Road right = ix.OutRoads.Single();

        left.BezierSeries.Add(right.BezierSeries);
        left.EndIntersection = right.EndIntersection;
        left.EndIntersection.RemoveRoad(right, Direction.In);

        left.LeftOutline.Mid.AddRange(left.LeftOutline.End);
        left.LeftOutline.Mid.AddRange(right.LeftOutline.Start);
        left.LeftOutline.Mid.AddRange(right.LeftOutline.Mid);
        left.LeftOutline.End = right.LeftOutline.End;

        left.RightOutline.Mid.AddRange(left.RightOutline.End);
        left.RightOutline.Mid.AddRange(right.RightOutline.Start);
        left.RightOutline.Mid.AddRange(right.RightOutline.Mid);
        left.RightOutline.End = right.RightOutline.End;

        for (int i = 0; i < left.LaneCount; i++)
        {
            left.EndIntersection.RemoveNode(right.Lanes[i].EndNode);
            left.Lanes[i].BezierSeries.Add(right.Lanes[i].BezierSeries);
            Graph.RemoveVertex(left.Lanes[i].EndVertex);
            Graph.RemoveVertex(right.Lanes[i].StartVertex);
            left.Lanes[i].EndVertex = right.Lanes[i].EndVertex;
            left.Lanes[i].InitInnerPath();
            Graph.AddPath(left.Lanes[i].InnerPath);
        }
        
        Game.RemoveRoad(right, RoadRemovalOption.Combine);
        Game.RemoveIntersection(right.StartIntersection);
        for (int i = 0; i < left.LaneCount; i++)
        {
            Game.Nodes.Remove(left.Lanes[i].EndNode.Id);
            left.Lanes[i].EndNode = right.Lanes[i].EndNode;
            left.Lanes[i].EndNode.InLane = left.Lanes[i];
        }
        left.EndIntersection.AddRoad(left, Direction.In);
        Game.InvokeRoadUpdated(left);

        return left;

    }
}