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
        UpdateConnectedness();
        District prev = null;
        foreach (District district in Game.Districts.Values)
        {
            if (!district.Enabled)
                if (prev.Connectedness > 20)
                    district.Enable();
            prev = district;
        }
    }

    public static void UpdateConnectedness()
    {
        int connectionCount = 0;
        IEnumerable<SourceZone> sourceZones = Game.SourceZones.Values.Where(z => z.Enabled);
        IEnumerable<TargetZone> targetZones = Game.TargetZones.Values.Where(z => z.Enabled);
        foreach (TargetZone targetZone in targetZones)
            connectionCount += targetZone.ConnectedSources.Count;
        GlobalConnectedness = (float)connectionCount / (sourceZones.Count() * targetZones.Count());
        GlobalConnectedness = MyNumerics.Round(GlobalConnectedness * 100, 2);

        foreach (District district in Game.Districts.Values)
            district.CalculateConnectedness();
    }
}