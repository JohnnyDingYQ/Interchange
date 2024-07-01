using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class BuildTargets
{
    public List<Node> Nodes { get; set; }
    public float3 ClickPos { get; set; }
    public bool Snapped { get; set; }
    public float3 MedianPoint { get; set; }
    public Intersection Intersection { get; set; }

    /// <summary>
    /// Determine nodes selected by a buildcommand and its properties
    /// </summary>
    /// <param name="clickPos">The click position</param>
    /// <param name="laneCount">How many lanes the road will have</param>
    /// <param name="gameNodes">Nodes eligible for consideration</param>
    public BuildTargets() {}

}