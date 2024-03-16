using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public class ReplaceTargets
{

    public bool SnapNotNull { get; set; }
    public ReplaceTargets(float3 pos, int laneCount, IEnumerable<Road> gameRoads)
    {
        foreach (Road road in gameRoads)
        {
            SnapNotNull = false;
        }
    }
}