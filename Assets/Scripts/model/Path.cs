using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.model.Roads;
using Unity.Mathematics;

public class Path
{
    public readonly List<Edge> Edges;
    public float Length { get; private set; }
    
    public Path(IEnumerable<Edge> edges)
    {
        Edges = edges.ToList();
        foreach (Edge edge in Edges)
            Length += edge.Length;
    }
}