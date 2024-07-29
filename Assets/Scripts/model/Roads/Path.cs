using System.Collections.Generic;
using QuikGraph;
using UnityEngine.Assertions;
using UnityEngine;

public class Path : IEdge<Vertex>, IPersistable
{
    public uint Id { get; set; }
    public Curve Curve { get; set; }
    public Vertex Source { get; set; }
    public Vertex Target { get; set; }
    public Path InterweavingPath { get; set; }
    public List<Car> Cars { get; set; }
    public Car IncomingCar { get; set; }
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

    public void Save(Writer writer)
    {
        writer.Write(Id);
        writer.Write(Curve.Id);
        writer.Write(Source.Id);
        writer.Write(Target.Id);
        writer.Write(InterweavingPath == null ? 0 : InterweavingPath.Id);
    }

    public void Load(Reader reader)
    {
        Id = reader.ReadUint();
        Curve = new() { Id = reader.ReadUint() };
        Source = new() { Id = reader.ReadUint() };
        Target = new() { Id = reader.ReadUint() };
        uint interweavingPathId = reader.ReadUint();
        InterweavingPath = interweavingPathId == 0 ? null : new() { Id = interweavingPathId };
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