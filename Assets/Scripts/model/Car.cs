using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;
using Assets.Scripts.Model.Roads;
using System;
using System.Linq;
using System.Collections.Generic;


public class Car
{
    public uint Id { get; set; }
    readonly Path path;
    public float3 Pos { get; private set; }
    public CarStatus Status;
    float speed;
    public float TimeTaken;
    int edgeIndex;
    public float DistanceOnEdge { get; private set; }

    public Car(Path path)
    {
        this.path = path;
        DistanceOnEdge = UnityEngine.Random.value * path.Edges[0].Length;
        // DistanceOnEdge = 0.1f;
    }

    public void Move(float deltaTime)
    {
        if (Status != CarStatus.Traveling)
            return;

        bool isBraking;
        TimeTaken += deltaTime;
        isBraking = false;
        Edge edge = path.Edges[edgeIndex];
        Edge nextEdge = edgeIndex < path.Edges.Count - 1 ? path.Edges[edgeIndex + 1] : null;
        int carIndex = GetCarIndex();

        if (!IsLeadingCarOnEdge())
            LookForCarAhead();
        else if (nextEdge != null)
            LookForEdgeAhead();

        if (!isBraking)
            speed = MathF.Min(speed + deltaTime * Constants.CarAcceleration, Constants.CarMaxSpeed);

        DistanceOnEdge += deltaTime * speed;

        if (DistanceOnEdge > edge.Length)
        {
            edge.Cars.RemoveAt(0);
            if (nextEdge != null)
            {
                DistanceOnEdge = 0;
                nextEdge.IncomingCar = null;
                edgeIndex++;
                nextEdge.Insert(this);
            }
            else
            {
                Finish();
            }
            return;
        }

        Pos = edge.Curve.EvaluatePosition(DistanceOnEdge);

        # region EXTRACTED FUNCTIONS
        int GetCarIndex()
        {
            int carIndex = edge.Cars.IndexOf(this);
            if (carIndex == -1) // insert if not found
                carIndex = edge.Insert(this);
            
            return carIndex;
        }

        void LookForCarAhead()
        {
            float distToNextCar = edge.Cars[carIndex - 1].DistanceOnEdge - DistanceOnEdge;
            if (distToNextCar < Constants.CarMinimumSeparation)
            {
                isBraking = true;
                speed = MathF.Max(Constants.CarMinSpeed, distToNextCar / Constants.CarMinimumSeparation * edge.Cars[carIndex - 1].speed);
            }
        }

        void LookForEdgeAhead()
        {
            if (edge.Length - DistanceOnEdge < Constants.CarMinimumSeparation && nextEdge.IncomingCar == null)
                nextEdge.IncomingCar = this;
            if (nextEdge.IsBlockedFor(this))
            {
                isBraking = true;
                float distFromEnd = edge.Length - DistanceOnEdge;
                speed = MathF.Max(Constants.CarMinSpeed, speed - deltaTime * Constants.CarDeceleration / distFromEnd);
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
        Status = CarStatus.Canceled;
    }

    void Finish()
    {
        Status = CarStatus.Finished;
    }
}