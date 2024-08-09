using System.Collections.Generic;
using Assets.Scripts.model.Roads;

public class SourceZone : Zone
{
    public SourceZone() { ConnectedTargets = new(); }

    [NotSaved]
    public Dictionary<TargetZone, Path> ConnectedTargets { get; private set; }
    [NotSaved]
    public const float ScheduleInterval = 1.5f;
    public SourceZone(uint id) : base(id)
    {
        ConnectedTargets = new();
    }
}