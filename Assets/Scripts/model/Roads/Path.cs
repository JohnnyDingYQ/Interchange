using System.Collections.Generic;
using QuikGraph;
using UnityEngine.Assertions;
using UnityEngine;

public class Path : IEdge<Vertex>, IPersistable
{
    public uint Id { get; set; }
    [SaveID]
    public Curve Curve { get; set; }
    [SaveID]
    public Vertex Source { get; set; }
    [SaveID]
    public Vertex Target { get; set; }
    [SaveID]
    public Path InterweavingPath { get; set; }
    [NotSaved]
    public List<Car> Cars { get; set; }
    [NotSaved]
    public Car IncomingCar { get; set; }
    [NotSaved]
    public float Length { get => Curve.Length; }

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

    public override bool Equals(object obj)
    {
        if (obj is Path other)
            return Id == other.Id && IPersistable.Equals(Curve, other.Curve) && IPersistable.Equals(Source, other.Source)
                && IPersistable.Equals(Target, other.Target) && IPersistable.Equals(InterweavingPath, other.InterweavingPath);
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

}