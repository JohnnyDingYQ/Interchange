using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;

public class Car
{
    public static event Action<Car> TravelCoroutine;
    public bool DestinationUnreachable { get; private set; }
    public bool ReachedDestination { get; private set; }
    private readonly Zone origin;
    private readonly Zone destination;
    private readonly Path[] paths;
    public float DistanceOnPath { get; set; }
    private int pathIndex;
    private float speed;

    public Car(Zone origin, Zone destination, Path[] paths)
    {
        Assert.IsNotNull(paths);
        DestinationUnreachable = false;
        this.origin = origin;
        this.destination = destination;
        this.paths = paths;
        DistanceOnPath = 0;
        pathIndex = 0;
        speed = 0;
    }

    public float3 Move(float deltaTime)
    {
        if (ReachedDestination)
            return 0;
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
            {
                ReachedDestination = true;
                return 0;
            }
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
            origin.Demands[destination.Id] += 1;
    }
}