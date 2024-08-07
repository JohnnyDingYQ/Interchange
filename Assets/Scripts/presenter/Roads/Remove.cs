using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Assets.Scripts.model.Roads;

public static class Remove
{
    // only called in Game.cs
    public static bool RemoveRoad(Road road, RoadRemovalOption option)
    {
        Game.Roads.Remove(road.Id);
        Game.RemoveCurve(road.Curve);
        foreach (Lane lane in road.Lanes)
        {
            Game.RemoveLane(lane);
            List<Edge> toRemove = new();
            if (option == RoadRemovalOption.Default || option == RoadRemovalOption.Replace)
            {
                Game.SourceZones?.Values.ToList().ForEach(zone =>
                {
                    zone.RemoveVertex(lane.StartVertex);
                    zone.RemoveVertex(lane.EndVertex);
                });
                Game.TargetZones?.Values.ToList().ForEach(zone =>
                {
                    zone.RemoveVertex(lane.StartVertex);
                    zone.RemoveVertex(lane.EndVertex);
                });
                Graph.RemoveVertex(lane.StartVertex);
                Graph.RemoveVertex(lane.EndVertex);
            }
            else
            {
                foreach (Edge p in Game.Edges.Values)
                    if (p.Source == lane.StartVertex && p.Target == lane.EndVertex)
                        toRemove.Add(p);
            }
            toRemove.ForEach(p => Graph.RemoveEdge(p));

            if (option == RoadRemovalOption.Default || option == RoadRemovalOption.Replace)
            {
                lane.StartNode.OutLane = null;
                lane.EndNode.InLane = null;
                if (option == RoadRemovalOption.Default)
                {
                    if (lane.StartNode.OutLane == null && lane.StartNode.InLane == null)
                        Game.RemoveNode(lane.StartNode);

                    if (lane.EndNode.OutLane == null && lane.EndNode.InLane == null)
                        Game.RemoveNode(lane.EndNode);
                }
            }
        }
        road.StartIntersection.RemoveRoad(road, Direction.Out);
        road.EndIntersection.RemoveRoad(road, Direction.In);

        if (option != RoadRemovalOption.Combine && option != RoadRemovalOption.Replace)
            EvaluateIntersections(road, option);

        Game.InvokeRoadRemoved(road);
        road.Id = 0;
        return true;

        static void EvaluateIntersections(Road road, RoadRemovalOption option)
        {
            if (!road.StartIntersection.IsEmpty())
            {
                IntersectionUtil.EvaluateEdges(road.StartIntersection);
                IntersectionUtil.EvaluateOutline(road.StartIntersection);
            }
            else
                Game.RemoveIntersection(road.StartIntersection);

            if (!road.EndIntersection.IsEmpty())
            {
                IntersectionUtil.EvaluateEdges(road.EndIntersection);
                IntersectionUtil.EvaluateOutline(road.EndIntersection);
            }
            else
                Game.RemoveIntersection(road.EndIntersection);

            if (option != RoadRemovalOption.Divide)
            {
                if (!road.StartIntersection.IsEmpty())
                    Game.UpdateIntersection(road.StartIntersection);
                if (!road.EndIntersection.IsEmpty())
                    Game.UpdateIntersection(road.EndIntersection);
            }

        }
    }
}