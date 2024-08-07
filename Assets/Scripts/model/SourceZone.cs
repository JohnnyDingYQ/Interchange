using System.Collections.Generic;
using Assets.Scripts.model.Roads;

public class SourceZone : Zone
{
    public SourceZone() { ConnectedTargets = new(); }

    [NotSaved]
    public Dictionary<TargetZone, IEnumerable<Edge>> ConnectedTargets { get; private set; }
    [NotSaved]
    public const float ScheduleInterval = 1.5f;
    [NotSaved]
    public float ScheduleCooldown { get; set; }
    public SourceZone(uint id) : base(id)
    {
        ConnectedTargets = new();
    }
}