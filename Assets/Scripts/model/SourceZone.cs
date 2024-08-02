using System.Collections.Generic;
using Assets.Scripts.model.Roads;

public class SourceZone : Zone
{
    [NotSaved]
    public Dictionary<TargetZone, IEnumerable<Edge>> ConnectedTargets { get; private set; }
    public const float ScheduleInterval = 1.5f;
    public float ScheduleCooldown { get; set; }
    public SourceZone(uint id) : base(id)
    {
        ConnectedTargets = new();
    }
}