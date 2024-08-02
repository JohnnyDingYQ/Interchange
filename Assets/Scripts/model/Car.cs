using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;
using Interchange;

public class Car
{
    public uint Id { get; set; }
    public bool IsTraveling { get; set; }
    public bool IsDone { get; private set; }
    public float3 Pos { get; private set; }
    private readonly Edge[] edges;
    public float DistanceOnEdge { get; set; }
    private int edgeIndex;
    public Edge CurrentEdge {get => edges[edgeIndex];}
    private float speed;
    private bool isBraking;

    public Car(Edge[] edges)
    {
        Assert.IsNotNull(edges);
        this.edges = edges;
        IsTraveling = false;
        DistanceOnEdge = 0;
        edgeIndex = 0;
        speed = 0;
        Pos = new(0, -100, 0);
    }
    public void Start()
    {
        IsTraveling = true;
        edges.First().Source.ScheduledCars--;
    }
    public void Move(float deltaTime)
    {
        if (IsDone)
            return;
        isBraking = false;
        Edge edge = edges[edgeIndex];
        Edge nextEdge = edgeIndex + 1 < edges.Count() ? edges[edgeIndex + 1] : null;
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
            edgeIndex++;
            if (nextEdge != null)
            {
                nextEdge.IncomingCar = null;
                nextEdge.AddCar(this);
                Pos = nextEdge.Curve.StartPos;
            }
            else
            {
                IsDone = true;
            }
            return;
        }
        Pos = edge.Curve.EvaluateDistancePos(DistanceOnEdge);

        # region EXTRACTED FUNCTIONS
        int GetCarIndex()
        {
            int carIndex = edge.Cars.IndexOf(this);
            if (carIndex == -1)
            {
                carIndex = edge.Cars.Count();
                edge.Cars.Add(this);
            }
            return carIndex;
        }

        void LookForCarAhead()
        {
            float distToNextCar = edge.Cars[carIndex - 1].DistanceOnEdge - DistanceOnEdge;
            if (distToNextCar < Constants.CarMinimumSeparation)
            {
                isBraking = true;
                speed = distToNextCar / Constants.CarMinimumSeparation * edge.Cars[carIndex - 1].speed;
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
        # endregion
    }

    public bool SpawnBlocked()
    {
        Edge edge = edges.First();
        if (edge.Cars.Count > 0)
            if (edge.Cars.Last().DistanceOnEdge < Constants.CarMinimumSeparation)
                return true;
        return false;
    }

    public void Cancel()
    {
        IsDone = true;
    }
}