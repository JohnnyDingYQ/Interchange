using System.Collections.Generic;

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

            if (option == RoadRemovalOption.Default)
            {
                foreach (Node node in new List<Node>() { lane.StartNode, lane.EndNode })
                {
                    if (node == lane.StartNode)
                        node.OutLane = null;
                    else
                        node.InLane = null;
                    if (node.InLane == null && node.OutLane == null && !node.BelongsToPoint)
                    {
                        Game.RemoveNode(node);
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
}