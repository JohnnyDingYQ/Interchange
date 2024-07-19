using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class Remove
{
    // only called in Game.cs
    public static bool RemoveRoad(Road road, RoadRemovalOption option)
    {
        if (!Game.Roads.ContainsKey(road.Id))
            return false;
        Game.Roads.Remove(road.Id);
        foreach (Lane lane in road.Lanes)
        {
            List<Path> toRemove = new();
            if (option == RoadRemovalOption.Default || option == RoadRemovalOption.Replace)
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

            if (option == RoadRemovalOption.Default || option == RoadRemovalOption.Replace)
            {
                lane.StartNode.OutLane = null;
                lane.EndNode.InLane = null;
                if (option == RoadRemovalOption.Default)
                {
                    if (lane.StartNode.OutLane == null && lane.StartNode.InLane == null
                        && !lane.StartNode.BelongsToPoint)
                    {
                        Game.RemoveNode(lane.StartNode);
                        road.StartIntersection.RemoveNode(lane.StartNode);
                    }
                    if (lane.EndNode.OutLane == null && lane.EndNode.InLane == null
                        && !lane.EndNode.BelongsToPoint)
                    {
                        Game.RemoveNode(lane.EndNode);
                        road.EndIntersection.RemoveNode(lane.EndNode);
                    }
                }
            }
            Game.RemoveLane(lane);
        }
        road.StartIntersection.RemoveRoad(road, Direction.Out);
        road.EndIntersection.RemoveRoad(road, Direction.In);

        if (option != RoadRemovalOption.Combine && option != RoadRemovalOption.Replace)
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
}