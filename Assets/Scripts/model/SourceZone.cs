using System.Collections.Generic;

public class SourceZone : Zone
{
    public Dictionary<TargetZone, Path> ConnectedTargets { get; private set; }
    public SourceZone(uint id) : base(id, ZoneType.Source)
    {
    }
}