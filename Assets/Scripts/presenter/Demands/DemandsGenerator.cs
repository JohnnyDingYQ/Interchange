public static class DemandsGenerator
{
    public static void GenerateDemands()
    {
        foreach (IZone zone in Game.Zones.Values)
            foreach (IZone other in Game.Zones.Values)
                if (zone != other)
                    zone.Demands[other.Id] = 1;
    }
}