using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class District : IPersistable
{
    public uint Id { get; set; }
    [NotSaved]
    public string Name { get; private set; }
    public bool Enabled { get; private set; }
    public float Connectedness { get; private set; }
    [SaveIDCollection]
    public HashSet<Zone> Zones { get; private set; }

    public District()
    {
        Zones = new();
    }

    public District(uint id, string name)
    {
        Id = id;
        Name = name;
        Zones = new();
    }

    public void CalculateConnectedness()
    {
        int connectionCount = 0;
        foreach (Zone zone in Zones)
            connectionCount += zone.ConnectedZones.Count();
        Connectedness = MyNumerics.Round((float) connectionCount / (Zones.Count * (Zones.Count - 1)) * 100, 2); 
    }

    public void Enable()
    {
        Enabled = true;
        foreach (Zone zone in Zones)
            zone.Enabled = true;
        foreach (Zone zone in Zones)
            zone.Enabled = true;
    }

    public void Disable()
    {
        Enabled = false;
        foreach (Zone zone in Zones)
            zone.Enabled = false;
        foreach (Zone zone in Zones)
            zone.Enabled = false;
    }

    public override bool Equals(object obj)
    {
        if (obj is District other)
            return Id == other.Id && Name.Equals(other.Name) && Connectedness == other.Connectedness && Enabled == other.Enabled
                && Zones.Select(v => Id).SequenceEqual(other.Zones.Select(v => Id))
                && Zones.Select(v => Id).SequenceEqual(other.Zones.Select(v => Id));
        else
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}