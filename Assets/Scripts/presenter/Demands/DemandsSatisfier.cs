using System;
using System.Collections.Generic;
using System.Linq;
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
                    Car car = new(zone, Game.Zones[zoneID]);
                    car.Travel();
                    shouldDecrement.Add(zoneID);
                }
            }
            foreach (int i in shouldDecrement)
            {
                zone.Demands[i] -= 1;
            }
        }
    }
}