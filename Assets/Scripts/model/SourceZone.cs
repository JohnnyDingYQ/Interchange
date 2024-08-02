using System.Collections.Generic;

public class SourceZone : Zone
{
    public HashSet<TargetZone> ConnectedTargets { get; private set; }
    public SourceZone(uint id) : base(id, ZoneType.Source)
    {
    }
}