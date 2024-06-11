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
    public bool DestinationUnreachable { get; set; }
    public bool ReachedDestination { get; private set; }
    private readonly Zone origin;
    private readonly Zone destination;
    private readonly Path[] paths;
    public float DistanceOnPath { get; set; }
    private int pathIndex;
    public Path CurrentPath {get {return paths[pathIndex]; }}
    private float speed;
    private bool isBraking;

    public Car(Zone origin, Zone destination, Path[] paths)
    {
        Assert.IsNotNull(paths);
        Assert.IsFalse(origin == null ^ destination == null);
        DestinationUnreachable = false;
        this.origin = origin;
        this.destination = destination;
        this.paths = paths;
        IsTraveling = false;
        DistanceOnPath = 0;
        pathIndex = 0;
        speed = 0;
        Drive?.Invoke(this);
    }
    public void Start()
    {
        IsTraveling = true;
        paths.First().Source.ScheduledCars--;
    }
    public float3 Move(float deltaTime)
    {
        if (ReachedDestination)
            return 0;
        isBraking = false;
        Path path = paths[pathIndex];
        Path nextPath = pathIndex + 1 < paths.Count() ? paths[pathIndex + 1] : null;
        int carIndex = GetCarIndex();

        if (!IsLeadingCarOnPath())
            LookForCarAhead();
        else if (nextPath != null)
            LookForPathAhead();
        
        if (!isBraking)
            speed = MathF.Min(speed + deltaTime * Constants.CarAcceleration, Constants.CarMaxSpeed);

        DistanceOnPath += deltaTime * speed;
        
        if (DistanceOnPath > path.Length)
        {
            path.Cars.RemoveAt(0);
            pathIndex++;
            if (nextPath != null)
            {
                nextPath.IncomingCar = null;
                nextPath.AddCar(this);
                return nextPath.BezierSeries.EvaluatePosition(0);
            }
            else
            {
                ReachedDestination = true;
                return 0;
            }
        }
        return path.BezierSeries.EvaluatePosition(DistanceOnPath / path.Length);

        # region EXTRACTED FUNCTIONS
        int GetCarIndex()
        {
            int carIndex = path.Cars.IndexOf(this);
            if (carIndex == 0)
            {
                carIndex = path.Cars.Count();
                path.Cars.Add(this);
            }
            return carIndex;
        }

        void LookForCarAhead()
        {
            float distToNextCar = path.Cars[carIndex - 1].DistanceOnPath - DistanceOnPath;
            if (distToNextCar < Constants.CarMinimumSeparation)
            {
                isBraking = true;
                speed = distToNextCar / Constants.CarMinimumSeparation * path.Cars[carIndex - 1].speed;
            }
        }

        void LookForPathAhead()
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

        bool IsLeadingCarOnPath()
        {
            return carIndex == 0;
        }
        # endregion
    }

    public bool SpawnBlocked()
    {
        Path path = paths.First();
        if (path.Cars.Count > 0)
            if (path.Cars.Last().DistanceOnPath < Constants.CarMinimumSeparation)
                return true;
        return false;
    }

    public void Stop()
    {
        if (origin != null)
            origin.Demands[destination.Id] += 1;
    }
}