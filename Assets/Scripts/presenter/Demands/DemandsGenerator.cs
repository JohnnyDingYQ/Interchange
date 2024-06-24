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
        foreach (SourcePoint source in Game.Sources.Values)
        {
            Point target = Game.Targets.ElementAt((int) (UnityEngine.Random.value * Game.Targets.Count())).Value;
            if (source.Destinations.ContainsKey(target.Id))
                source.Destinations[target.Id] = Math.Min(Constants.DestinationCap, source.Destinations[target.Id] + 1);
            else
                source.Destinations[target.Id] = 1;
        }
        coolDown = 0.3f;
    }
}