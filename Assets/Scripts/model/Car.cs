using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;
using Assets.Scripts.model.Roads;
using System;
using System.Linq;
using System.Collections.Generic;


public class Car : IPersistable
{
    public uint Id { get; set; }
    float distanceOnEdge;
    int edgeIndex;
    float speed;
    public float TimeTaken { get; private set; }
    public bool IsDone { get; private set; }
    [SaveID]
    readonly SourceZone sourceZone;
    [SaveID]
    readonly TargetZone targetZone;
    [NotSaved]
    Path path;
    [NotSaved]
    public Edge CurrentEdge { get => GetCurrentEdge(); }
    [NotSaved]
    public float3 Pos { get => CurrentEdge.Curve.EvaluateDistancePos(distanceOnEdge); }

    public Car() { }

    Edge GetCurrentEdge()
    {
        if (path == null)
            return null;
        if (edgeIndex >= path.Edges.Count)
            return null;
        return path.Edges[edgeIndex];
    }

    public Car(SourceZone sourceZone, TargetZone targetZone)
    {
        this.sourceZone = sourceZone;
        this.targetZone = targetZone;
        path = GetPath();
        speed = 0;
    }

    Path GetPath()
    {
        return sourceZone.ConnectedTargets.GetValueOrDefault(targetZone);
    }

    public void UpdatePath()
    {
        path = GetPath();
    }

    void CheckForPathChange()
    {
        if (path != GetPath())
        {
            Edge currentEdge = CurrentEdge;
            UpdatePath();
            if (path == null)
            {
                Cancel();
                return;
            }
            int index = path.Edges.IndexOf(currentEdge);
            if (index == -1) // does not exist
            {
                Cancel();
                return;
            }
            Assert.IsTrue(Graph.ContainsEdge(currentEdge));
            edgeIndex = index;
        }
    }

    public void Move(float deltaTime)
    {
        CheckForPathChange();
        if (IsDone)
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

        distanceOnEdge += deltaTime * speed;

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
                speed = distToNextCar / Constants.CarMinimumSeparation * edge.Cars[carIndex - 1].speed;
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
        IsDone = true;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}