using System.Collections.Generic;

public class SourcePoint : Point
{
    public Dictionary<uint, int> Destinations { get; set; }
    public float CarSpawnInterval { get; set; }
    public SourcePoint(uint id) : base(id)
    {
        Destinations = new();
    }
}