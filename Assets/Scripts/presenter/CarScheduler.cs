using System.Collections.Generic;
using System.Linq;
using Interchange;

public static class CarScheduler
{
    public static void DetermineZoneConnectedness()
    {
        foreach (SourceZone source in Game.SourceZones.Values)
            source.ConnectedTargets.Clear();
        foreach (TargetZone target in Game.TargetZones.Values)
            target.ConnectedSources.Clear();

        foreach (SourceZone source in Game.SourceZones.Values)
            foreach (TargetZone target in Game.TargetZones.Values)
            {
                if (source.Vertices.Count == 0 || target.Vertices.Count == 0)
                    continue;
                IEnumerable<Edge> edges = Graph.ShortestPathAStar(source.Vertices.First(), target.Vertices.First());
                if (edges != null)
                {
                    source.ConnectedTargets.Add(target);
                    target.ConnectedSources.Add(source);
                }
            }


    }
}