using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;

public class Car
{
    public static event Action<Car> Drive;
    public bool IsTraveling { get; set; }
    public bool DestinationUnreachable { get; private set; }
    public bool ReachedDestination { get; private set; }
    private readonly Zone origin;
    private readonly Zone destination;
    private readonly Path[] paths;
    public float DistanceOnPath { get; set; }
    private int pathIndex;
    private float speed;
    private bool isBraking;

    public Car(Zone origin, Zone destination, Path[] paths)
    {
        Assert.IsNotNull(paths);
        DestinationUnreachable = false;
        this.origin = origin;
        this.destination = destination;
        this.paths = paths;
        IsTraveling = false;
        DistanceOnPath = 0;
        pathIndex = 0;
        speed = 0;
        Drive.Invoke(this);
    }

    public float3 Move(float deltaTime)
    {
        if (ReachedDestination)
            return 0;
        isBraking = false;
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
            float distToNextCar = path.Cars[carIndex - 1].DistanceOnPath - DistanceOnPath;
            if (distToNextCar < Constants.CarMinimumSeparation)
            {
                isBraking = true;
                speed = distToNextCar / Constants.CarMinimumSeparation * path.Cars[carIndex - 1].speed;
            }
        }
        else if (carIndex == 0 && nextPath != null)
        {
            if (path.Length - DistanceOnPath < Constants.CarMinimumSeparation && nextPath.IncomingCar == null)
                nextPath.IncomingCar = this;
            if (nextPath.IsBlockedFor(this))
            {
                isBraking = true;
                float distFromEnd = path.Length - DistanceOnPath;
                speed = MathF.Max(Constants.CarMinSpeed, speed - deltaTime * Constants.CarDeceleration / distFromEnd);
            }
        }

        if (!isBraking)
            speed = MathF.Min(speed + deltaTime * Constants.CarAcceleration, Constants.CarMaxSpeed);

        DistanceOnPath += deltaTime * speed;
        Assert.IsTrue(DistanceOnPath >= 0);
        
        if (DistanceOnPath > path.Length)
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

    public void ReturnDemand()
    {
        origin.Demands[destination.Id] += 1;
    }
}