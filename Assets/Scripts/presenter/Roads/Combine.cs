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
        {
            Debug.Log("invalid");
            return null;
        }
        Road left = ix.InRoads.Single();
        Road right = ix.OutRoads.Single();

        left.Curve.Add(right.Curve);
        left.EndIntersection = right.EndIntersection;
        left.EndIntersection.RemoveRoad(right, Direction.In);

        left.LeftOutline.EndCurve = right.LeftOutline.EndCurve;
        left.RightOutline.EndCurve = right.RightOutline.EndCurve;

        for (int i = 0; i < left.LaneCount; i++)
        {
            left.EndIntersection.RemoveNode(right.Lanes[i].EndNode);
            left.Lanes[i].Curve.Add(right.Lanes[i].Curve);
            Graph.RemoveVertex(left.Lanes[i].EndVertex);
            Graph.RemoveVertex(right.Lanes[i].StartVertex);
            left.Lanes[i].EndVertex = right.Lanes[i].EndVertex;
            left.Lanes[i].InitInnerPath();
            Graph.AddPath(left.Lanes[i].InnerPath);
        }

        left.SetInnerOutline();
        
        Game.RemoveRoad(right, RoadRemovalOption.Combine);
        Game.RemoveIntersection(right.StartIntersection);
        for (int i = 0; i < left.LaneCount; i++)
        {
            Game.Nodes.Remove(left.Lanes[i].EndNode.Id);
            left.Lanes[i].EndNode = right.Lanes[i].EndNode;
            left.Lanes[i].EndNode.InLane = left.Lanes[i];
        }
        left.EndIntersection.AddRoad(left, Direction.In);
        Game.UpdateIntersection(left.StartIntersection);

        // Debug.Log(left.Curve.GetChainLength());
        return left;

    }
}