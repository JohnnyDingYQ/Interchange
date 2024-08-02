using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.model.Roads;

public static class CarScheduler
{
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
                    source.ConnectedTargets.Add(target, new(edges));
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