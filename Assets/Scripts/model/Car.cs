using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;
using Assets.Scripts.model.Roads;
using System;
using System.Linq;


public class Car : IPersistable
{
    public uint Id { get; set; }
    public bool IsDone { get; private set; }
    public Edge CurrentEdge { get => path.edges[edgeIndex]; }
    public float3 Pos { get => CurrentEdge.Curve.EvaluateDistancePos(distanceOnEdge); }
    float distanceOnEdge;
    int edgeIndex;
    public float Speed { get; set; }
    [SaveID]
    public Path path;

    public Car(Path path)
    {
        this.path = path;
        Speed = 0;
    }

    public void Move(float deltaTime)
    {
        bool isBraking;
        if (IsDone)
            return;
        isBraking = false;
        Edge edge = path.edges[edgeIndex];
        Edge nextEdge = edgeIndex < path.edges.Count - 1 ? path.edges[edgeIndex + 1] : null;
        int carIndex = GetCarIndex();

        if (!IsLeadingCarOnEdge())
            LookForCarAhead();
        else if (nextEdge != null)
            LookForEdgeAhead();

        if (!isBraking)
            Speed = MathF.Min(Speed + deltaTime * Constants.CarAcceleration, Constants.CarMaxSpeed);

        distanceOnEdge += deltaTime * Speed;

        if (distanceOnEdge > edge.Length)
        {
            edge.Cars.RemoveAt(0);
            if (nextEdge != null)
            {
                distanceOnEdge = 0;

                nextEdge.IncomingCar = null;
                edgeIndex++;
                nextEdge.AddCar(this);
            }
            else
            {
                Cancel();
            }
            return;
        }

        # region EXTRACTED FUNCTIONS
        int GetCarIndex()
        {
            int carIndex = edge.Cars.IndexOf(this);
            if (carIndex == -1) // not found
            {
                carIndex = edge.Cars.Count();
                edge.Cars.Add(this);
            }
            return carIndex;
        }

        void LookForCarAhead()
        {
            float distToNextCar = edge.Cars[carIndex - 1].distanceOnEdge - distanceOnEdge;
            if (distToNextCar < Constants.CarMinimumSeparation)
            {
                isBraking = true;
                Speed = distToNextCar / Constants.CarMinimumSeparation * edge.Cars[carIndex - 1].Speed;
            }
        }

        void LookForEdgeAhead()
        {
            if (edge.Length - distanceOnEdge < Constants.CarMinimumSeparation && nextEdge.IncomingCar == null)
                nextEdge.IncomingCar = this;
            if (nextEdge.IsBlockedFor(this))
            {
                isBraking = true;
                float distFromEnd = edge.Length - distanceOnEdge;
                Speed = MathF.Max(Constants.CarMinSpeed, Speed - deltaTime * Constants.CarDeceleration / distFromEnd);
            }
        }

        bool IsLeadingCarOnEdge()
        {
            return carIndex == 0;
        }
        #endregion
    }

    public void Cancel()
    {
        IsDone = true;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}