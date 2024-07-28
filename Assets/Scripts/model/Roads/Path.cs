using System.Collections.Generic;
using QuikGraph;
using Unity.Mathematics;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine.Assertions;
using UnityEngine;

[JsonObject]
public class Path : IEdge<Vertex>, IPersistable
{
    public uint Id { get; set; }
    public Curve Curve { get; private set; }
    public Vertex Source { get; set; }
    public Vertex Target { get; set; }
    public float Length { get => Curve.Length; }
    public Path InterweavingPath { get; set; }
    public List<Car> Cars { get; set; }
    public Car IncomingCar { get; set; }

    public Path() { Cars = new(); }

    public Path(Curve curve, Vertex source, Vertex target)
    {
        Curve = curve;
        Source = source;
        Target = target;
        Cars = new();
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

    public void Save(Writer writer)
    {
        throw new System.NotImplementedException();
    }

    public void Load(Reader reader)
    {
        throw new System.NotImplementedException();
    }

    

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        return Id == ((Path) obj).Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    
}