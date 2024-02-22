using Unity.Mathematics;

public interface IBuildManagerBoundary
{
    void InstantiateRoad(Road road);

    float3 GetPos();

    void EvaluateIntersection(Intersection intersection);

    void RedrawAllRoads();
}