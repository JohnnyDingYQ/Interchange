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
    [JsonIgnore]
    public Vertex Source { get; set; }
    public uint Source_ { get; set; }
    [JsonIgnore]
    public Vertex Target { get; set; }
    public uint Target_ { get; set; }
    [JsonIgnore]
    public float Length { get => BezierSeries.Length; }
    [JsonIgnore]
    public Path InterweavingPath { get; set; }
    public uint InterweavingPath_ { get; set; }
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

    // public override int GetHashCode()
    // {
    //     return Id.GetHashCode();
    // }

    // public override bool Equals(object obj)
    // {
    //     if (obj == null || GetType() != obj.GetType())
    //         return false;
    //     return Id == ((Path) obj).Id;
    // }
}