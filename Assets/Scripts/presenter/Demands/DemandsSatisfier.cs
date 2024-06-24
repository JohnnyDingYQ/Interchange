using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DemandsSatisfer
{
    static readonly HashSet<uint> toDecrement = new();
    public static void SatisfyDemands(float deltaTime)
    {
        foreach (SourcePoint source in Game.Sources.Values)
        {
            if (source.CarSpawnInterval > 0)
            {
                source.CarSpawnInterval -= deltaTime;
                continue;
            }
            foreach (uint targetID in source.Destinations.Keys)
            {
                int demand = source.Destinations[targetID];
                Point target = Game.Targets[targetID];
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

    public static Car AttemptSchedule(SourcePoint source, Point target)
    {
        ReadOnlySet<Lane> soureOut = source.Node.GetLanes(Direction.Out);
        ReadOnlySet<Lane> targetIn = target.Node.GetLanes(Direction.In);
        if (soureOut.Count == 0 || targetIn.Count == 0)
            return null;
        Vertex startV = soureOut.First().StartVertex;
        Vertex endV = targetIn.First().EndVertex;
        return AttemptSchedule(startV, endV);
    }

    public static Car AttemptSchedule(Vertex origin, Vertex dest)
    {
        IEnumerable<Path> paths = Graph.ShortestPathAStar(origin, dest);
        if (!ScheduleAllowed(paths))
            return null;
        Car car = new(paths.ToArray());
        paths.First().Source.ScheduledCars++;
        return car;
    }

    public static Car AttemptSchedule(Road origin, Road dest)
    {
        Vertex start = origin.Lanes[MyNumerics.GetRandomIndex(origin.LaneCount)].StartVertex;
        Vertex end = dest.Lanes[MyNumerics.GetRandomIndex(dest.LaneCount)].EndVertex;
        return AttemptSchedule(start, end);
    }

    static bool ScheduleAllowed(IEnumerable<Path> paths)
    {
        if (paths == null)
            return false;
        return paths.First().Source.ScheduledCars < Constants.MaxVertexWaitingCar;
    }
}