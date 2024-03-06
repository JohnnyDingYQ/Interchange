using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class DivisionTargets
{
    Road Road { get; set; }
    float InterpolationRatio { get; set; }
    public DivisionTargets(float3 clickPos, IEnumerable<Road> GameRoads)
    {
        List<FloatContainer> floatContainers = new();
        foreach (Road road in GameRoads)
        {
            clickPos.y = 0;
            Ray ray = new(clickPos, Vector3.up);
            float distance = CurveUtility.GetNearestPoint(road.Curve, ray, out float3 position, out float interpolation);
            if (distance < GlobalConstants.DivsionTargetSnapRange)
            {
                // floatContainers.Add(new(distance, road));
            }
        }
    }
}