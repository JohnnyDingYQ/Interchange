using Unity.Mathematics;
using UnityEngine;

public class BuildCandidate
{
    public Road Road;
    public float CurveInterpolation;

    public BuildCandidate(Road road, float3 clickPos)
    {
        Road = road;
        clickPos.y = 0;
        Ray ray = new(clickPos, Vector3.up);
        road.BezierSeries.GetNearestPoint(ray, out _, out float interpolation);
        CurveInterpolation = interpolation;
    }
}