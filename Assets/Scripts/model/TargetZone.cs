using System.Collections.Generic;

public class TargetZone : Zone
{
    public HashSet<SourceZone> ConnectedSources { get; private set; }
    public TargetZone(uint id) : base(id, ZoneType.Target)
    {
    }
}