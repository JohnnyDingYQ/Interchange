using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Node : IComparable<Node>, IPersistable
{
    public uint Id { get; set; }
    public float3 Pos { get; private set; }
    public int NodeIndex { get; private set; }
    public Lane InLane { get; set; }
    public Lane OutLane { get; set; }
    public Intersection Intersection { get; set; }
    public Node() { }
    public Node(float3 pos, float elevation, int nodeIndex)
    {
        pos.y = elevation;
        Pos = pos;
        NodeIndex = nodeIndex;
        Id = 0;
    }

    public HashSet<Road> GetRoads()
    {
        HashSet<Road> r = new();
        if (InLane != null)
            r.Add(InLane.Road);
        if (OutLane != null)
            r.Add(OutLane.Road);
        return r;
    }

    public int CompareTo(Node other)
    {
        return NodeIndex.CompareTo(other.NodeIndex);
    }

    public void Save(Writer writer)
    {
        writer.Write(Id);
        writer.Write(Pos);
        writer.Write(NodeIndex);
        writer.Write(InLane == null ? 0 : InLane.Id);
        writer.Write(OutLane == null ? 0 : OutLane.Id);
        writer.Write(Intersection == null ? 0 : Intersection.Id);
    }

    public void Load(Reader reader)
    {
        Id = reader.ReadUint();
        Pos = reader.ReadFloat3();
        NodeIndex = reader.ReadInt();
        uint inLaneId = reader.ReadUint();
        InLane = inLaneId == 0 ? null : new() { Id = inLaneId };
        uint outLaneId = reader.ReadUint();
        OutLane = outLaneId == 0 ? null : new() { Id = outLaneId };
        uint ixId = reader.ReadUint();
        Intersection = ixId == 0 ? null : new() { Id = ixId};
    }

    public override bool Equals(object obj)
    {
        if (obj is Node other)
            return Id == other.Id && Pos.Equals(other.Pos) && NodeIndex == other.NodeIndex
            && IPersistable.Equals(InLane, other.InLane) && IPersistable.Equals(OutLane, other.OutLane)
            && IPersistable.Equals(Intersection, other.Intersection);
        else
        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return "Node " + Id;
    }
}