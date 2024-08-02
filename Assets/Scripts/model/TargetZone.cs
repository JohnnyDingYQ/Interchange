using System.Collections.Generic;

public class TargetZone : Zone
{
    [NotSaved]
    public HashSet<SourceZone> ConnectedSources { get; private set; }
    public TargetZone(uint id) : base(id)
    {
        ConnectedSources = new();
    }
}