using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Assets.Scripts.Model.Roads;

public static class Remove
{
    // only called in Game.cs
    public static bool RemoveRoad(Road road, RoadRemovalOption option)
    {
        if (road.RoadProp != RoadProp.PlayerBuilt && !Game.LevelEditorOn)
            return false;
        Game.Roads.Remove(road.Id);
        Game.RemoveCurve(road.Curve);
        foreach (Lane lane in road.Lanes)
        {
            Game.RemoveLane(lane);
            List<Edge> toRemove = new();
            if (option == RoadRemovalOption.Default || option == RoadRemovalOption.Replace)
            {
                Graph.RemoveVertex(lane.StartVertex);
                Graph.RemoveVertex(lane.EndVertex);
            }
            else
            {
                Edge edge = Graph.GetEdge(lane.StartVertex, lane.EndVertex);
                if (edge != null)
                    toRemove.Add(edge);
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