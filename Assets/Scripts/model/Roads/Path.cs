using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine.Assertions;
using UnityEngine;

[JsonObject]
public class Path : IEdge<Vertex>
{
    [JsonProperty]
    public BezierSeries BezierSeries { get; private set; }
    [JsonProperty]
    public Vertex Source { get; private set; }
    [JsonProperty]
    public Vertex Target { get; private set; }
    [JsonIgnore]
    public float Length { get { return BezierSeries.Length; } }
    [JsonIgnore]
    public bool IsBlocked { get { return IsBlockedCheck(); } }
    public Path InterweavingPath { get; set; }
    /// <summary>
    /// Smaller index means closer to path end, value is distance traveled
    /// </summary>
    [JsonIgnore]
    public List<Car> Cars { get; set; }
    [JsonIgnore]
    public Car IncomingCar { get; set; }
    [JsonIgnore]
    public float IncomingDistance { get; set; }

    public Path() { Cars = new(); }

    public Path(BezierSeries bezierSeries, Vertex source, Vertex target)
    {
        BezierSeries = bezierSeries;
        Source = source;
        Target = target;
        Cars = new();
    }

    public List<float3> GetOutline(Orientation orientation)
    {
        return BezierSeries.GetOutline(orientation);
    }

    public void AddCar(Car car)
    {
        car.PathIndex = Cars.Count;
        car.DistanceOnPath = 0;
        Cars.Add(car);
    }

    public float MoveCar(Car car, float deltaTime, Path nextPath)
    {
        float distanceMoved = deltaTime * Constants.CarSpeed;
        float currDistance = car.DistanceOnPath;
        float newDistance = currDistance + distanceMoved;
        if (car.PathIndex != 0)
        {
            if (newDistance + Constants.CarMinimumSeparation > Cars[car.PathIndex - 1].DistanceOnPath)
                newDistance = Cars[car.PathIndex - 1].DistanceOnPath - Constants.CarMinimumSeparation;
        }
        else if (nextPath != null && Length - newDistance < Constants.CarMinimumSeparation)
        {
            if (nextPath.IncomingCar == null || nextPath.IncomingCar == car)
            {
                nextPath.IncomingCar = car;
                nextPath.IncomingDistance = Constants.CarMinimumSeparation - (Length - newDistance);
            }
            else
                newDistance = MathF.Min(newDistance, Length - (Constants.CarMinimumSeparation - nextPath.IncomingDistance));
        }

        car.DistanceOnPath = newDistance;
        return newDistance;
    }

    public void RemoveCar(Car car)
    {
        Assert.AreEqual(0, car.PathIndex);
        Cars.RemoveAt(0);
        foreach (Car c in Cars)
            c.PathIndex--;
    }

    public bool IsBlockedCheck()
    {
        if (InterweavingPath != null)
            if (InterweavingPath.Cars.Count != 0)
                return true;
        if (Cars.Count > 0)
            if (Cars[0].DistanceOnPath < Constants.CarMinimumSeparation)
                return true;
        return false;
    }
}