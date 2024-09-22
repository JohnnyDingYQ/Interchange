using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Model.Roads;
using Unity.Mathematics;

public class Path
{
    public List<Edge> Edges { get; set; }
    public float Cost { get; private set; }
    public Vertex StartVertex { get => Edges[0].Source; }


    public Path() { Edges = new(); }

    public Path(IEnumerable<Edge> edges)
    {
        Edges = edges.ToList();
    }
}