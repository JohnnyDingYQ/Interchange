using Unity.Mathematics;

public class LevelData : IPersistable
{
    public uint Id { get; set; }
    public float2 boundaryCenter;
    public float boundaryRadius;
}