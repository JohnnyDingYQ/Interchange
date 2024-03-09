using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class DivideTargets
{
    public Road Road { get; set; }
    public float Interpolation { get; set; }
    public bool IsValid { get; set; }
    public DivideTargets(float3 clickPos, IEnumerable<Road> GameRoads)
    {
        List<FloatContainer> floatContainers = new();
        foreach (Road road in GameRoads)
        {
            clickPos.y = 0;
            Ray ray = new(clickPos, Vector3.up);
            float distance = CurveUtility.GetNearestPoint(road.Curve, ray, out float3 position, out float interpolation);
            if (distance < GConsts.DivsionTargetSnapRange)
            {
                floatContainers.Add(new(distance, new RoadNearestPoint(road, interpolation)));
            }
        }
        if (floatContainers.Count == 0)
        {
            IsValid = false;
            return;
        }
        floatContainers.Sort();
        RoadNearestPoint target = FloatContainer.Unwrap<RoadNearestPoint>(floatContainers).First();
        Road = target.Road;
        Interpolation = target.Interpolation;
        IsValid = true;
    }

    private readonly struct RoadNearestPoint
    {
        public float Interpolation { get; }
        public Road Road { get; }

        public RoadNearestPoint(Road road, float interpolation)
        {
            Road = road;
            Interpolation = interpolation;
        }
    }
}