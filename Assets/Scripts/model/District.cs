using System.Collections.Generic;
using UnityEngine;

public class District : IPersistable
{
    public uint Id { get; set; }
    public string Name { get; private set; }
    public bool Enabled { get; private set; }
    public float Connectedness { get; private set; }
    [SaveIDCollection]
    public HashSet<SourceZone> SourceZones { get; private set; }
    [SaveIDCollection]
    public HashSet<TargetZone> TargetZones { get; private set; }

    public District()
    {
        SourceZones = new();
        TargetZones = new();
    }

    public District(uint id, string name)
    {
        Id = id;
        Name = name;
        SourceZones = new();
        TargetZones = new();
    }

    public void CalculateConnectedness()
    {
        int connectionCount = 0;
        foreach (TargetZone targetZone in TargetZones)
        {
            foreach (SourceZone sourceZone in targetZone.ConnectedSources)
                if (SourceZones.Contains(sourceZone))
                    connectionCount++;
        }
        Connectedness = MyNumerics.Round((float) connectionCount / (SourceZones.Count * TargetZones.Count) * 100, 2); 
    }

    public void Enable()
    {
        Enabled = true;
        foreach (Zone zone in SourceZones)
            zone.Enabled = true;
        foreach (Zone zone in TargetZones)
            zone.Enabled = true;
    }
}