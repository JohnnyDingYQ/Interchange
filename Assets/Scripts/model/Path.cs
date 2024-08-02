using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.model.Roads;
using Unity.Mathematics;

public class Path
{
    readonly List<Edge> edges;
    public float3 Pos { get => CurrentEdge.Curve.EvaluateDistancePos(DistanceOnEdge); }
    public Edge CurrentEdge { get => edges.Last(); }
    public float DistanceOnEdge { get; set; }

    public Path(IEnumerable<Edge> given)
    {
        edges = given.Reverse().ToList();
    }
    
    public void Move(Car car, float deltaTime)
    {
        bool isBraking;
        if (car.IsDone)
            return;
        isBraking = false;
        Edge edge = edges.Last();
        Edge nextEdge = edges.Count > 1 ? edges[^2] : null;
        int carIndex = GetCarIndex();

        if (!IsLeadingCarOnEdge())
            LookForCarAhead();
        else if (nextEdge != null)
            LookForEdgeAhead();

        if (!isBraking)
            car.Speed = MathF.Min(car.Speed + deltaTime * Constants.CarAcceleration, Constants.CarMaxSpeed);

        DistanceOnEdge += deltaTime * car.Speed;

        if (DistanceOnEdge > edge.Length)
        {
            edge.Cars.RemoveAt(0);
            if (nextEdge != null)
            {
                DistanceOnEdge = 0;
                edges.Remove(edges.Last());
                nextEdge.IncomingCar = null;
                nextEdge.AddCar(car);
            }
            else
            {
                car.Cancel();
            }
            return;
        }

        # region EXTRACTED FUNCTIONS
        int GetCarIndex()
        {
            int carIndex = edge.Cars.IndexOf(car);
            if (carIndex == -1)
            {
                carIndex = edge.Cars.Count();
                edge.Cars.Add(car);
            }
            return carIndex;
        }

        void LookForCarAhead()
        {
            float distToNextCar = edge.Cars[carIndex - 1].DistanceOnEdge - DistanceOnEdge;
            if (distToNextCar < Constants.CarMinimumSeparation)
            {
                isBraking = true;
                car.Speed = distToNextCar / Constants.CarMinimumSeparation * edge.Cars[carIndex - 1].Speed;
            }
        }

        void LookForEdgeAhead()
        {
            if (edge.Length - DistanceOnEdge < Constants.CarMinimumSeparation && nextEdge.IncomingCar == null)
                nextEdge.IncomingCar = car;
            if (nextEdge.IsBlockedFor(car))
            {
                isBraking = true;
                float distFromEnd = edge.Length - DistanceOnEdge;
                car.Speed = MathF.Max(Constants.CarMinSpeed, car.Speed - deltaTime * Constants.CarDeceleration / distFromEnd);
            }
        }

        bool IsLeadingCarOnEdge()
        {
            return carIndex == 0;
        }
        #endregion
    }
}