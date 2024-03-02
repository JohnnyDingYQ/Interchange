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

    public BuildTargets(float3 clickPos, int laneCount)
    {
        Nodes = GetBuildNodes(clickPos, laneCount);
        if (Nodes == null)
        {
            SnapNotNull = false;
            ClickPos = clickPos;
        }
        else
        {
            SnapNotNull = true;
            MedianPoint = Vector3.Lerp(Nodes.First().Pos, Nodes.Last().Pos, 0.5f); ;
        }

    }
    List<Node> GetBuildNodes(float3 clickPos, int laneCount)
    {
        float snapRadius = laneCount / 2 * GlobalConstants.LaneWidth + GlobalConstants.SnapTolerance;
        List<Container> candidates = new();
        foreach (Node node in Game.Nodes.Values)
        {
            float distance = Vector3.Distance(clickPos, node.Pos);
            if (distance < snapRadius)
            {
                candidates.Add(new(distance, node));
            }
        }
        candidates.Sort();
        if (candidates.Count >= laneCount)
        {

            List<Node> result = new();
            for (int i = 0; i < laneCount; i++)
            {
                result.Add(candidates[i].Node);
            }
            result.Sort();
            return result;
        }
        else
        {
            return null;
        }
    }
    private class Container: IComparable<Container>
    {
        public float Distance {get; set;}
        public Node Node {get; set;}
        public Container(float distance, Node node)
        {
            Distance = distance;
            Node = node;
        }

        public int CompareTo(Container other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }
}