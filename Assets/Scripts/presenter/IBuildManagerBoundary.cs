using Unity.Mathematics;

public interface IBuildManagerBoundary
{
    void InstantiateRoad(Road road);

    float3 GetPos();

    void RedrawAllRoads();
}