using System.Collections.Generic;
using Assets.Scripts.model.Roads;

public class SourceZone : Zone
{
    public SourceZone() { ConnectedTargets = new(); }

    [NotSaved]
    public Dictionary<TargetZone, Path> ConnectedTargets { get; private set; }
    public SourceZone(uint id) : base(id)
    {
        ConnectedTargets = new();
    }
}