using System;
using System.Linq;

public static class DemandsGenerator
{
    private static float coolDown = 0;
    public static void GenerateDemands(float deltaTime)
    {
        if (coolDown > 0)
        {
            coolDown -= deltaTime;
            return;
        }
        foreach (Zone zone in Game.Zones.Values)
        {
            Zone otherZone = zone;
            while (otherZone == zone)
                otherZone = Game.Zones.ElementAt((int) (UnityEngine.Random.value * Game.Zones.Count())).Value;
            if (zone.Demands.ContainsKey(otherZone.Id))
                zone.Demands[otherZone.Id] = Math.Max(Constants.ZoneDemandCap, zone.Demands[otherZone.Id] + 1);
            else
                zone.Demands[otherZone.Id] = 1;
        }
        coolDown = 0.3f;
    }
}