using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using Unity.Mathematics;

public static class DemandsSatisfer
{
    public static event Action<IEnumerable<Path>> Drive;
    public static void SatisfyDemands()
    {
        foreach (IZone zone in Game.Zones.Values)
        {
            List<int> shouldZero = new();
            foreach (int zoneID in zone.Demands.Keys)
            {
                int demand = zone.Demands[zoneID];
                if (demand == 1 && zone.OutVerticesCount != 0 && Game.Zones[zoneID].InVerticesCount != 0)
                {
                    SendCar(zone, Game.Zones[zoneID]);
                    shouldZero.Add(zoneID);
                }
            }
            foreach (int i in shouldZero)
            {
                zone.Demands[i] = 0;
            }
        }
    }

    public static void SendCar(IZone origin, IZone destination)
    {
        Vertex startV = origin.GetRandomOutVertex();
        Vertex endV = destination.GetRandomInVertex();
        TryFunc<Vertex, IEnumerable<Path>> tryFunc = Game.Graph.ShortestPathsAStar(
            (Path p) => p.Length,
            (Vertex to) => math.distance(startV.Pos, to.Pos),
            startV
        );
        tryFunc(endV, out IEnumerable<Path> paths);
        Drive.Invoke(paths);
    }
}