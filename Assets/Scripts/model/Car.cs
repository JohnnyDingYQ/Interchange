using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;

public class Car
{
    public uint Id { get; set; }
    public bool IsTraveling { get; set; }
    public bool IsDone { get; private set; }
    public float3 Pos { get; private set; }
    private readonly Path[] paths;
    public float DistanceOnPath { get; set; }
    private int pathIndex;
    public Path CurrentPath {get => paths[pathIndex];}
    private float speed;
    private bool isBraking;

    public Car(Path[] paths)
    {
        Assert.IsNotNull(paths);
        this.paths = paths;
        IsTraveling = false;
        DistanceOnPath = 0;
        pathIndex = 0;
        speed = 0;
        Pos = new(0, -100, 0);
    }
    public void Start()
    {
        IsTraveling = true;
        paths.First().Source.ScheduledCars--;
    }
    public void Move(float deltaTime)
    {
        if (IsDone)
            return;
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
                Pos = nextPath.Curve.StartPos;
            }
            else
            {
                IsDone = true;
            }
            return;
        }
        Pos = path.Curve.EvaluateDistancePos(DistanceOnPath);

        # region EXTRACTED FUNCTIONS
        int GetCarIndex()
        {
            int carIndex = path.Cars.IndexOf(this);
            if (carIndex == -1)
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

    public void Cancel()
    {
        IsDone = true;
    }
}