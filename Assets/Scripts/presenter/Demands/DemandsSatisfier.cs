using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using Unity.Mathematics;
using UnityEngine;

public static class DemandsSatisfer
{
    public static void SatisfyDemands()
    {
        foreach (Zone zone in Game.Zones.Values)
        {
            List<int> shouldDecrement = new(); // avoids concurrent modification of dict
            foreach (int zoneID in zone.Demands.Keys)
            {
                int demand = zone.Demands[zoneID];
                if (demand > 0 && zone.OutVerticesCount != 0 && Game.Zones[zoneID].InVerticesCount != 0)
                {
                    IEnumerable<Path> paths = FindPath(zone, Game.Zones[zoneID]);
                    if (paths != null)
                    {
                        Car car = new(zone, Game.Zones[zoneID], paths);
                        car.Travel();
                        shouldDecrement.Add(zoneID);
                    }
                }
            }
            foreach (int i in shouldDecrement)
            {
                zone.Demands[i] -= 1;
            }
        }
    }

    public static IEnumerable<Path> FindPath(Zone origin, Zone dest)
    {
        Vertex startV = origin.GetRandomOutVertex();
        Vertex endV = dest.GetRandomInVertex();
        if (startV == null || endV == null)
            return null;
        TryFunc<Vertex, IEnumerable<Path>> tryFunc = Game.Graph.ShortestPathsAStar(
            (Path p) => p.Length,
            (Vertex to) => math.distance(startV.Pos, to.Pos),
            startV
        );
        tryFunc(endV, out IEnumerable<Path> paths);
        return paths;
    }
}