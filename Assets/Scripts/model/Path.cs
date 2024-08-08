using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.model.Roads;
using Unity.Mathematics;

public class Path
{
    public readonly List<Edge> edges;
    
    public Path(IEnumerable<Edge> given)
    {
        edges = given.ToList();
    }
}