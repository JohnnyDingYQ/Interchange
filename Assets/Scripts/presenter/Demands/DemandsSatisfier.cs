using System;
using System.Collections.Generic;
using System.Linq;
using GraphExtensions;

public static class DemandsSatisfer
{
    public static void SatisfyDemands(float deltaTime)
    {
        foreach (Zone zone in Game.Zones.Values)
        {
            if (zone.CarSpawnInterval > 0)
            {
                zone.CarSpawnInterval -= deltaTime;
                continue;
            }
            int toDecrement = -1;
            foreach (int zoneID in zone.Demands.Keys)
            {
                int demand = zone.Demands[zoneID];
                if (demand > 0 && zone.OutVerticesCount != 0 && Game.Zones[zoneID].InVerticesCount != 0)
                {
                    IEnumerable<Path> paths = FindPath(zone, Game.Zones[zoneID]);
                    if (paths != null)
                    {
                        Car car = new(zone, Game.Zones[zoneID], paths.ToArray());
                        toDecrement = zoneID;
                        zone.CarSpawnInterval = (float)10 / demand;
                        break;
                    }
                }
            }
            if (toDecrement != -1)
                zone.Demands[toDecrement] -= 1;
        }
    }

    public static IEnumerable<Path> FindPath(Zone origin, Zone dest)
    {
        Vertex startV = origin.GetRandomOutVertex();
        Vertex endV = dest.GetRandomInVertex();
        if (startV == null || endV == null)
            return null;
        return Game.Graph.GetPathsFromAtoB(startV, endV);
    }
}