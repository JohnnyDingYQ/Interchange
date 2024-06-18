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
    public Intersection Intersection { get; set; }

    /// <summary>
    /// Determine nodes selected by a buildcommand and its properties
    /// </summary>
    /// <param name="clickPos">The click position</param>
    /// <param name="laneCount">How many lanes the road will have</param>
    /// <param name="gameNodes">Nodes eligible for consideration</param>
    public BuildTargets(float3 clickPos, int laneCount, IEnumerable<Node> gameNodes)
    {
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
            AddNodeIfWithinSnap(node);

        if (floatContainers.Count == 0)
            return null;

        // this sorts nodes with ascending order of distance (descending proximity)
        floatContainers.Sort();

        List<Node> nodes = FloatContainer.Unwrap<Node>(floatContainers);
        while (nodes.Count > laneCount)
            nodes.Remove(nodes.Last());


        // this sorts nodes with their order in the road, left to right in the direction of the roads
        nodes.Sort();
        Intersection intersection = nodes.First().Intersection;
        foreach (Node n in nodes)
            if (n.Intersection != intersection)
                return null;
        Intersection = intersection;
        if (nodes.Count == laneCount)
            return nodes;

        else
        {
            GetInterpolatedCandidates(2);
            if (floatContainers.Count + nodes.Count < laneCount)
            {
                return null;
            }
            int i = 0;
            while (nodes.Count < laneCount)
            {
                nodes.Add((Node)floatContainers[i++].Object);
            }
            nodes.Sort();
            return nodes;
        }

        # region extracted functions
        void GetInterpolatedCandidates(int interpolationReach)
        {
            float3 normal = Intersection.Normal * Constants.LaneWidth;
            floatContainers = new();
            for (int i = 1; i <= interpolationReach; i++)
            {
                float3 left = nodes.First().Pos + i * normal;
                float3 right = nodes.Last().Pos - i * normal;
                AddNodeIfWithinSnap(new(left, nodes.First().Pos.y, nodes.First().NodeIndex - i)
                {
                    Intersection = Intersection
                });
                AddNodeIfWithinSnap(new(right, nodes.Last().Pos.y, nodes.Last().NodeIndex + i)
                {
                    Intersection = Intersection
                });
            }
            floatContainers.Sort();
        }

        void AddNodeIfWithinSnap(Node n)
        {
            if (n.Pos.y == Constants.MinElevation)
                return;
            float distance = Vector3.Distance(clickPos, n.Pos);
            if (distance < snapRadius)
            {
                floatContainers.Add(new(distance, n));
            }
        }
        #endregion
    }

}