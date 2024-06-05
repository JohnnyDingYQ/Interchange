using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class Car
{
    public static event Action<Car> TravelCoroutine;
    public bool DestinationUnreachable { get; set; }
    public Zone Origin { get; private set; }
    public Zone Destination { get; private set; }
    public IEnumerable<Path> paths;
    // public bool Blocked { get; set; }
    public float DistanceOnPath { get; set; }

    public Car(Zone origin, Zone destination, IEnumerable<Path> paths)
    {
        Assert.IsNotNull(paths);
        DestinationUnreachable = false;
        Origin = origin;
        Destination = destination;
        this.paths = paths;
    }

    public void Travel()
    {

        TravelCoroutine?.Invoke(this);
        if (DestinationUnreachable)
            Origin.Demands[Destination.Id] += 1;

    }
}