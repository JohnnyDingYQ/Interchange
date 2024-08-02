using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.model.Roads;

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
                if (source.Vertices.Count == 0 || target.Vertices.Count == 0)
                    continue;
                IEnumerable<Edge> edges = Graph.ShortestPathAStar(source.Vertices.First(), target.Vertices.First());
                if (edges != null)
                {
                    source.ConnectedTargets.Add(target, edges);
                    target.ConnectedSources.Add(source);
                    
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