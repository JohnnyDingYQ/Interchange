using UnityEngine;
using System.Linq;

public static class Combine
{

    public static bool CombineIsValid(Intersection ix)
    {
        if (ix.InRoads.Count != 1 || ix.OutRoads.Count != 1)
            return false;
        Road left = ix.InRoads.Single();
        Road right = ix.OutRoads.Single();
        if (left.LaneCount != right.LaneCount)
            return false;
        for (int i = 0; i < left.LaneCount; i++)
            if (left.Lanes[i].Length + right.Lanes[i].Length > Constants.MaxLaneLength)
                return false;
        return true;
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
        left.EndIntersection.AddRoad(left, Direction.In);

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
            left.Lanes[i].BezierSeries.Add(right.Lanes[i].BezierSeries);
            left.Lanes[i].EndNode = right.Lanes[i].EndNode;
            left.Lanes[i].EndVertex = right.Lanes[i].EndVertex;
            left.Lanes[i].InnerPath.Target = right.Lanes[i].InnerPath.Target;
            left.Lanes[i].InnerPath.Cars.AddRange(right.Lanes[i].InnerPath.Cars);
            Game.RemovePath(left.Lanes[i].InnerPath);
            left.Lanes[i].InitInnerPath();
        }
        Game.RemoveRoad(right, RoadRemovalOption.Combine);
        Game.RemoveIntersection(right.StartIntersection);
        for (int i = 0; i < left.LaneCount; i++)
        {
            Game.RemoveVertex(left.Lanes[i].EndVertex);
            Game.RemoveVertex(right.Lanes[i].StartVertex);
        }
        Game.InvokeRoadUpdated(left);

        return left;

    }
}