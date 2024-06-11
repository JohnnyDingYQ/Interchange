using System;
using System.Collections.Generic;
using System.Linq;
using GraphExtensions;
using UnityEngine;

public static class DemandsSatisfer
{
    static readonly HashSet<ulong> toDecrement = new();
    public static void SatisfyDemands(float deltaTime)
    {
        foreach (Zone zone in Game.Zones.Values)
        {
            if (zone.CarSpawnInterval > 0)
            {
                zone.CarSpawnInterval -= deltaTime;
                continue;
            }
            foreach (ulong zoneID in zone.Demands.Keys)
            {
                int demand = zone.Demands[zoneID];
                if (demand > 0)
                {
                    Car car = AttemptSchedule(zone, Game.Zones[zoneID]);
                    if (car != null)
                    {
                        Game.RegisterCar(car);
                        toDecrement.Add(zoneID);
                        zone.CarSpawnInterval = Constants.ZoneDemandSatisfyCooldown / demand;
                    }
                }
            }
            foreach (ulong i in toDecrement)
                zone.Demands[i] -= 1;
            toDecrement.Clear();
        }
    }

    public static Car AttemptSchedule(Zone origin, Zone dest)
    {
        Vertex startV = origin.GetRandomOutVertex();
        Vertex endV = dest.GetRandomInVertex();
        if (startV == null || endV == null)
            return null;
        IEnumerable<Path> paths = Game.Graph.ShortestPathAStar(startV, endV);
        if (!ScheduleAllowed(paths))
            return null;
        Car car = new(origin, dest, paths.ToArray());
        paths.First().Source.ScheduledCars++;
        return car;
    }

    public static Car AttemptSchedule(Vertex origin, Vertex dest)
    {
        IEnumerable<Path> paths = Game.Graph.ShortestPathAStar(origin, dest);
        if (!ScheduleAllowed(paths))
            return null;
        Car car = new(null, null, paths.ToArray());
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