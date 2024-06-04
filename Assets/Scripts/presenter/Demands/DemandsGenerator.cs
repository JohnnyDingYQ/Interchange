public static class DemandsGenerator
{
    public static void GenerateDemands()
    {
        foreach (Zone zone in Game.Zones.Values)
        {
            foreach (Zone other in Game.Zones.Values)
                if (zone != other)
                    zone.Demands[other.Id] = 4;
        }
    }
}