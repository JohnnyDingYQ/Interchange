using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;

public class Car
{
    public static event Action<Car> TravelCoroutine;
    public bool DestinationUnreachable { get; set; }
    public Zone Origin { get; private set; }
    public Zone Destination { get; private set; }
    private Path[] paths;
    // public bool Blocked { get; set; }
    public float DistanceOnPath { get; set; }
    private int pathIndex;

    public Car(Zone origin, Zone destination, Path[] paths)
    {
        Assert.IsNotNull(paths);
        DestinationUnreachable = false;
        Origin = origin;
        Destination = destination;
        this.paths = paths;
        DistanceOnPath = 0;
        pathIndex = 0;
    }

    public float3 Move(float deltaTime)
    {
        float newDistance = DistanceOnPath + deltaTime * Constants.CarSpeed;
        Path path = paths[pathIndex];
        Path nextPath = pathIndex + 1 < paths.Count() ? paths[pathIndex + 1] : null;
        int carIndex = path.Cars.IndexOf(this);
        if (carIndex == -1)
        {
            carIndex = path.Cars.Count();
            path.Cars.Add(this);
        }
        if (carIndex != 0)
        {
            if (newDistance + Constants.CarMinimumSeparation > path.Cars[carIndex - 1].DistanceOnPath)
                newDistance = MathF.Max(path.Cars[carIndex - 1].DistanceOnPath - Constants.CarMinimumSeparation, 0);
        }
        else if (carIndex == 0 && nextPath != null)
        {
            if (path.Length - newDistance < Constants.CarMinimumSeparation && nextPath.IncomingCar == null)
                nextPath.IncomingCar = this;
            if (nextPath.IsBlockedFor(this))
                newDistance = MathF.Min(newDistance, path.Length - Constants.CarMinimumSeparation);
        }

        if (newDistance > path.Length)
        {
            path.Cars.RemoveAt(0);
            pathIndex++;
            if (pathIndex >= paths.Count())
                return new(-1, -1, -1);
            if (nextPath != null)
            {
                nextPath.IncomingCar = null;
                nextPath.AddCar(this);
            }
            return nextPath.BezierSeries.EvaluatePosition(0);
        }
        Assert.IsTrue(newDistance >= 0);
        DistanceOnPath = newDistance;
        return path.BezierSeries.EvaluatePosition(DistanceOnPath / path.Length);
    }

    public bool SpawnBlocked()
    {
        Path path = paths.First();
        if (path.Cars.Count > 0)
            if (path.Cars.Last().DistanceOnPath < Constants.CarMinimumSeparation)
                return true;
        return false;
    }

    public void Travel()
    {
        TravelCoroutine?.Invoke(this);
        if (DestinationUnreachable)
            Origin.Demands[Destination.Id] += 1;
    }
}