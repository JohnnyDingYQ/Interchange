using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BuildTargets
{
    public List<Node> Nodes { get; set; }
    public float3 ClickPos { get; set; }
    public bool SnapNotNull { get; set; }
    public float3 MedianPoint { get; set; }
    public Side Side { get; set; }

    /// <summary>
    /// Determine nodes selected by a buildcommand and its properties
    /// </summary>
    /// <param name="clickPos">The click position</param>
    /// <param name="laneCount">How many lanes the road will have</param>
    /// <param name="side">Which side of the raod does the clickPos define</param>
    /// <param name="gameNodes">Nodes eligible for consideration</param>
    public BuildTargets(float3 clickPos, int laneCount, Side side, IEnumerable<Node> gameNodes)
    {
        Side = side;
        Nodes = GetBuildNodes(clickPos, laneCount, gameNodes);
        if (Nodes == null)
        {
            SnapNotNull = false;
            ClickPos = clickPos;
        }
        else
        {
            SnapNotNull = true;
            MedianPoint = Vector3.Lerp(Nodes.First().Pos, Nodes.Last().Pos, 0.5f);
        }
    }
    List<Node> GetBuildNodes(float3 clickPos, int laneCount, IEnumerable<Node> GameNodes)
    {
        float snapRadius = (laneCount * Constants.LaneWidth + Constants.BuildSnapTolerance) / 2;
        List<FloatContainer> floatContainers = new();
        foreach (Node node in GameNodes)
        {
            AddNodeIfWithinSnap(node);
        }

        // this sorts nodes with ascending order of distance (descending proximity)
        floatContainers.Sort();

        List<Node> nodes = FloatContainer.Unwrap<Node>(floatContainers);
        while (nodes.Count > laneCount)
        {
            nodes.Remove(nodes.Last());
        }

        // this sorts nodes with their order in the road, left to right in the direction of the roads
        nodes.Sort();
        if (nodes.Count == laneCount)
        {
            return nodes;
        }
        else if (nodes.Count > 0)
        {
            Road road = nodes.First().Lanes.First().Road;
            GetInterpolatedCandidates(road, 2, Side);
            if (floatContainers.Count + nodes.Count < laneCount)
            {
                return null;
            }
            int i = 0;
            while (nodes.Count < laneCount)
            {
                nodes.Add((Node) floatContainers[i++].Object);
            }
            nodes.Sort();
            return nodes;
        }
        else
        {
            return null;
        }

        # region extracted functions
        void GetInterpolatedCandidates(Road road, int interpolationReach, Side side)
        {
            floatContainers = new();
            float t = side == Side.Start ? 1 : 0;
            for (int i = 0; i < interpolationReach; i++)
            {
                float3 left = road.InterpolateLanePos(t, -(1 + i));
                float3 right = road.InterpolateLanePos(t, road.LaneCount + i);
                AddNodeIfWithinSnap(new(left, -(1 + i)));
                AddNodeIfWithinSnap(new(right, road.LaneCount + i));
            }
            floatContainers.Sort();
        }

        void AddNodeIfWithinSnap(Node n)
        {
            float distance = Vector3.Distance(clickPos, n.Pos);
            if (distance < snapRadius)
            {
                floatContainers.Add(new(distance, n));
            }
        }
        #endregion
    }
    
}