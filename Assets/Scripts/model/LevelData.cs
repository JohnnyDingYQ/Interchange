using Unity.Mathematics;

public class LevelData : IPersistable
{
    public uint Id { get; set; }
    public float2 BoundaryCenter { get; set; }
    public float BoundaryRadius { get; set; }
}