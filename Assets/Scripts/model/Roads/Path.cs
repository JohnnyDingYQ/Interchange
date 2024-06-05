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
    public Path InterweavingPath { get; set; }
    [JsonIgnore]
    public List<Car> Cars { get; set; }
    [JsonIgnore]
    public Car IncomingCar { get; set; }

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
        car.DistanceOnPath = 0;
        Cars.Add(car);
    }

    public float MoveCar(Car car, float deltaTime, Path nextPath)
    {
        float newDistance = car.DistanceOnPath + deltaTime * Constants.CarSpeed;
        int carIndex = Cars.IndexOf(car);
        // Debug.Log(carIndex);
        if (carIndex != 0)
        {
            if (newDistance + Constants.CarMinimumSeparation > Cars[carIndex - 1].DistanceOnPath)
                newDistance = Cars[carIndex - 1].DistanceOnPath - Constants.CarMinimumSeparation;
        }
        else if (nextPath != null)
        {
            if (Length - newDistance < Constants.CarMinimumSeparation && nextPath.IncomingCar == null)
                nextPath.IncomingCar = car;
            if (nextPath.IsBlockedFor(car))
                newDistance = MathF.Min(newDistance, Length - Constants.CarMinimumSeparation);
        }
        
        if (newDistance > Length)
        {
            Cars.RemoveAt(0);
            if (nextPath != null)
            {
                nextPath.IncomingCar = null;
                Debug.Log("Removed");
            }
        }

        car.DistanceOnPath = newDistance;
        return newDistance;
    }

    public void PopCar()
    {
        Cars.RemoveAt(0);
    }

    public bool IsBlockedFor(Car car)
    {
        if (IncomingCar != car)
            return true;
        if (InterweavingPath != null)
            if (InterweavingPath.Cars.Count != 0 || InterweavingPath.IncomingCar != null)
                return true;
        return false;
    }

    public bool EntranceOccupied()
    {
        if (Cars.Count > 0)
            if (Cars.Last().DistanceOnPath < Constants.CarMinimumSeparation)
                return true;
        return false;
    }
}