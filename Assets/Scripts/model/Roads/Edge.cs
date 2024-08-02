using System.Collections.Generic;
using QuikGraph;
using UnityEngine.Assertions;
using UnityEngine;

namespace Interchange
{
    public class Edge : IEdge<Vertex>, IPersistable
    {
        public uint Id { get; set; }
        [SaveID]
        public Curve Curve { get; set; }
        [SaveID]
        public Vertex Source { get; set; }
        [SaveID]
        public Vertex Target { get; set; }
        [SaveID]
        public Edge InterweavingEdge { get; set; }
        [NotSaved]
        public List<Car> Cars { get; set; }
        [NotSaved]
        public Car IncomingCar { get; set; }
        [NotSaved]
        public float Length { get => Curve.Length; }

        public Edge() { Cars = new(); }

        public Edge(Curve curve, Vertex source, Vertex target)
        {
            Curve = curve;
            Source = source;
            Target = target;
            Cars = new();
        }

        public void AddCar(Car car)
        {
            car.DistanceOnEdge = 0;
            Cars.Add(car);
        }

        public bool IsBlockedFor(Car car)
        {
            if (IncomingCar != null && IncomingCar != car)
                return true;
            if (InterweavingEdge != null)
            {
                if (InterweavingEdge.Cars.Count != 0)
                    return true;
                if (InterweavingEdge.IncomingCar != null && Id > InterweavingEdge.Id) // deadlock prevention
                    return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge other)
                return Id == other.Id && IPersistable.Equals(Curve, other.Curve) && IPersistable.Equals(Source, other.Source)
                    && IPersistable.Equals(Target, other.Target) && IPersistable.Equals(InterweavingEdge, other.InterweavingEdge);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}