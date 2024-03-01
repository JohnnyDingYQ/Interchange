using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BuildTargets
{
    public List<BuildNode> BuildPoints { get; set; }
    public Road Road { get; set; }
    public NodeType NodeType { get; set; }
    public float3 ClickPos { get; set; }
    public bool SnapNotNull { get; set; }
    public float3 MedianPoint { get; set; }

    public BuildTargets(float3 clickPos, int laneCount)
    {
        BuildPoints = GetBuildTarget(clickPos, laneCount);
        if (BuildPoints == null)
        {
            SnapNotNull = false;
            ClickPos = clickPos;
        }
        else
        {
            SnapNotNull = true;
            Road = BuildPoints.First().Lane.Road;
            if (BuildPoints.First().Pos.Equals((float3) BuildPoints.First().Lane.StartPos))
            {
                NodeType = NodeType.StartNode;
            }
            else if (BuildPoints.First().Pos.Equals((float3) BuildPoints.First().Lane.EndPos))
            {
                NodeType = NodeType.EndNode;
            }
            else
            {
                throw new InvalidOperationException("WRONG");
            }
            MedianPoint = Vector3.Lerp(BuildPoints.First().Pos, BuildPoints.Last().Pos, 0.5f);;
        }
        
    }
    List<BuildNode> GetBuildTarget(float3 clickPos, int laneCount)
    {
        float snapRadius = laneCount / 2 * GlobalConstants.LaneWidth + GlobalConstants.SnapTolerance;
        List<BuildNode> candidates = new();
        foreach (Lane lane in Game.GetLaneIterator())
        {
            List<int> nodes = new() { lane.StartNode, lane.EndNode };
            List<float3> pos = new() { lane.StartPos, lane.EndPos };
            for (int i = 0; i < 2; i++)
            {
                float distance = Vector3.Distance(clickPos, pos[i]);
                if (distance < snapRadius)
                {
                    candidates.Add(new BuildNode(lane, nodes[i], distance, pos[i]));
                }
            }
        }

        candidates.Sort();
        if (candidates.Count >= laneCount)
        {
            candidates = candidates.GetRange(0, laneCount);
        }
        else
        {
            return null;
        }
        Road road = candidates.First().Lane.Road;
        foreach (BuildNode bt in candidates)
        {
            if (road == bt.Lane.Road)
            {
                road = bt.Lane.Road;
            }
            else
            {
                return null;
            }

        }
        List<BuildNode> sortedByLaneIndex = candidates.OrderBy(o => o.Lane.LaneIndex).ToList();
        return sortedByLaneIndex;

    }
}



public enum NodeType { StartNode, EndNode }

public class BuildNode : IComparable<BuildNode>
{
    public int Node { get; set; }
    public Lane Lane { get; set; }
    public float Distance { get; set; }
    public float3 Pos { get; set; }

    public BuildNode(Lane lane, int node, float distance, float3 pos)
    {
        Lane = lane;
        Node = node;
        Distance = distance;
        Pos = pos;
    }

    public BuildNode(float3 clickPos)
    {
        Lane = null;
        Node = -1;
        Distance = -1;
        Pos = clickPos;
    }

    public int CompareTo(BuildNode other)
    {
        return Distance.CompareTo(other.Distance);
    }
}