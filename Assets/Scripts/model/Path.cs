using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.model.Roads;
using Unity.Mathematics;

public class Path : IPersistable
{
    public uint Id { get; set; }
    [SaveIDCollection]
    public List<Edge> Edges { get; set; }
    public float Length { get; private set; }

    public Path() { Edges = new(); }

    public Path(IEnumerable<Edge> edges)
    {
        Edges = edges.ToList();
        foreach (Edge edge in Edges)
            Length += edge.Length;
    }
}