using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;


public class Node : IComparable<Node>
{
    public int Id { get; set; }
    public HashSet<Lane> Lanes { get; set; }
    public Vector3 Pos { get; set; }
    public int Order { get; set; }

    public Node() {}

    public Node(float3 pos, int order)
    {
        Pos = pos;
        Order = order;
        Id = -1;
        Lanes = new();
    }

    /// <summary>
    /// Implicit assumption: all lanes at the node has the same tangnet because of pivot adjustment
    /// </summary>
    /// <returns></returns>
    public float3 GetTangent()
    {
        if (Lanes.Count == 0)
        {
            throw new  InvalidOperationException("Node has no lane... Cannot get tangent");
        }
        Lane lane = Lanes.First();
        if (IsExitingLane(lane))
        {
            return lane.Spline.EvaluateTangent(0);
        }
        return lane.Spline.EvaluateTangent(1);
    }

    public bool IsExitingLane(Lane lane)
    {
        if (lane.StartNode == this)
        {
            return true;
        }
        return false;
    }

    public int CompareTo(Node other)
    {
        return Order.CompareTo(other.Order);
    }

    public bool IsRegistered()
    {
        return Id != -1;
    }

        public override string ToString()
    {
        return "Node " + Id;
    }

}