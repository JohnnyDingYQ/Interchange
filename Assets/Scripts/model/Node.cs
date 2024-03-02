using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


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

    public int CompareTo(Node other)
    {
        return Order.CompareTo(other.Order);
    }
}