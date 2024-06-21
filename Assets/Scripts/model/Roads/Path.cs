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
    public uint Id { get; set; }
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

    public bool IsBlockedFor(Car car)
    {
        if (IncomingCar != null && IncomingCar != car)
            return true;
        if (InterweavingPath != null)
        {
            if (InterweavingPath.Cars.Count != 0)
                return true;
            if (InterweavingPath.IncomingCar != null && Id > InterweavingPath.Id) // deadlock prevention
            return true;
        }

        return false;
    }
}