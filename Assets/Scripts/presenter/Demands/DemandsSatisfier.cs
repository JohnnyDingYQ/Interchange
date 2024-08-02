using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Interchange;

public static class DemandsSatisfer
{
    static readonly HashSet<uint> toDecrement = new();
    public static void SatisfyDemands(float deltaTime)
    {
        foreach (Zone source in Game.SourceZones.Values)
        {
            if (source.CarSpawnInterval > 0)
            {
                source.CarSpawnInterval -= deltaTime;
                continue;
            }
            foreach (uint targetID in source.Destinations.Keys)
            {
                int demand = source.Destinations[targetID];
                Zone target = Game.TargetZones[targetID];
                if (demand > 0)
                {
                    Car car = AttemptSchedule(source, target);
                    if (car != null)
                    {
                        Game.RegisterCar(car);
                        toDecrement.Add(targetID);
                        source.CarSpawnInterval = Constants.ZoneDemandSatisfyCooldownSpeed / demand;
                    }
                }
            }
            foreach (uint i in toDecrement)
                source.Destinations[i] -= 1;
            toDecrement.Clear();
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
        if (!ScheduleAllowed(edges))
            return null;
        Car car = new(edges.ToArray());
        edges.First().Source.ScheduledCars++;
        return car;
    }

    public static Car AttemptSchedule(Road origin, Road dest)
    {
        Vertex start = origin.Lanes[MyNumerics.GetRandomIndex(origin.LaneCount)].StartVertex;
        Vertex end = dest.Lanes[MyNumerics.GetRandomIndex(dest.LaneCount)].EndVertex;
        return AttemptSchedule(start, end);
    }

    static bool ScheduleAllowed(IEnumerable<Edge> edges)
    {
        if (edges == null)
            return false;
        return edges.First().Source.ScheduledCars < Constants.MaxVertexWaitingCar;
    }
}