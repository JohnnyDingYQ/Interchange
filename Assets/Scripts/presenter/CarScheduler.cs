using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.model.Roads;
using System.Collections.ObjectModel;

public static class CarScheduler
{
    static CarScheduler()
    {
        Game.RoadRemoved += DetermineZoneConnectedness;
    }

    public static void Schedule(float deltaTime)
    {
        foreach (SourceZone source in Game.SourceZones.Values)
        {
            if (source.ScheduleCooldown <= 0)
            {
                if (source.ConnectedTargets.Count != 0)
                {
                    Game.RegisterCar(new(
                        new(source.ConnectedTargets.Values.ElementAt(MyNumerics.GetRandomIndex(source.ConnectedTargets.Count)))
                    ));
                    source.ScheduleCooldown = SourceZone.ScheduleInterval;
                }
            }
            else
                source.ScheduleCooldown -= deltaTime;
        }
    }

    public static void DetermineZoneConnectedness(Road road)
    {
        if (!road.IsGhost)
            DetermineZoneConnectedness();
    }

    public static void DetermineZoneConnectedness()
    {
        foreach (SourceZone source in Game.SourceZones.Values)
            source.ConnectedTargets.Clear();
        foreach (TargetZone target in Game.TargetZones.Values)
            target.ConnectedSources.Clear();

        foreach (SourceZone source in Game.SourceZones.Values)
            foreach (TargetZone target in Game.TargetZones.Values)
            {
                bool pathFound = false;
                foreach (Vertex sourceVertex in source.Vertices)
                {
                    foreach (Vertex targetVertex in target.Vertices)
                    {
                        IEnumerable<Edge> edges = Graph.ShortestPathAStar(sourceVertex, targetVertex);
                        if (edges != null)
                        {
                            source.ConnectedTargets.Add(target, edges);
                            target.ConnectedSources.Add(source);
                            pathFound = true;
                            break;
                        }
                    }
                    if (pathFound)
                        break;
                }

            }
    }
    public static Car AttemptSchedule(Zone source, Zone target)
    {
        if (source.Vertices.Count == 0 || target.Vertices.Count == 0)
            return null;
        Vertex startV = source.Vertices.ElementAt(MyNumerics.GetRandomIndex(source.Vertices.Count));
        Vertex endV = target.Vertices.ElementAt(MyNumerics.GetRandomIndex(target.Vertices.Count));
        return AttemptSchedule(startV, endV);
    }

    public static Car AttemptSchedule(Vertex origin, Vertex dest)
    {
        IEnumerable<Edge> edges = Graph.ShortestPathAStar(origin, dest);
        Car car = new(new(edges));
        return car;
    }

    public static Car AttemptSchedule(Road origin, Road dest)
    {
        Vertex start = origin.Lanes[MyNumerics.GetRandomIndex(origin.LaneCount)].StartVertex;
        Vertex end = dest.Lanes[MyNumerics.GetRandomIndex(dest.LaneCount)].EndVertex;
        return AttemptSchedule(start, end);
    }
}