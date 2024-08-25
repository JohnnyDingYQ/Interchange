using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Progression
{
    public static float GlobalConnectedness { get; private set; }
    static Progression()
    {
    }

    public static void CheckProgression()
    {
        UpdateGlobalConnectedness();
        District prev = null;
        foreach (District district in Game.Districts.Values)
        {
            if (!district.Enabled)
                if (prev.Connectedness > 20)
                    district.Enable();
            prev = district;
        }
    }

    public static void UpdateGlobalConnectedness()
    {
        int connectionCount = 0;
        IEnumerable<Zone> zones = Game.Zones.Values.Where(z => z.Enabled);
        foreach (Zone zone in zones)
            connectionCount += zone.ConnectedZones.Count();
        GlobalConnectedness = connectionCount / (zones.Count() * (zones.Count() - 1));
        GlobalConnectedness = MyNumerics.Round(GlobalConnectedness * 100, 2);

        foreach (District district in Game.Districts.Values)
            district.CalculateConnectedness();
    }
}