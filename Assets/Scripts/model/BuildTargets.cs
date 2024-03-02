using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class BuildTargets
{
    public List<Node> Nodes { get; set; }
    public Vector3 ClickPos { get; set; }
    public bool SnapNotNull { get; set; }
    public Vector3 MedianPoint { get; set; }
    public Side Side { get; set; }

    public BuildTargets(float3 clickPos, int laneCount, Side side)
    {
        Side = side;
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
        List<Candidate> candidates = new();
        foreach (Node node in Game.Nodes.Values)
        {
            AddNodeIfWithinSnap(node);
        }
        candidates.Sort();

        List<Node> nodes = UnwrapCandidates(candidates);
        nodes.Sort();

        if (nodes.Count == laneCount)
        {
            return nodes;
        }
        else if (nodes.Count > 0 && Side == Side.Start)
        {
            Lane lane = nodes.First().Lanes.First();
            float3 interpolatedPosLeft = lane.InterpolateLanePos(1, -1);
            float3 interpolatedPosRight = lane.InterpolateLanePos(1, lane.Road.LaneCount);
            Node interpolatedNodeLeft = new(interpolatedPosLeft, -1);
            Node interpolatedNodeRight = new(interpolatedPosRight, lane.Road.LaneCount);
            candidates = new();
            AddNodeIfWithinSnap(interpolatedNodeLeft);
            AddNodeIfWithinSnap(interpolatedNodeRight);
            candidates.Sort();
            if (candidates.Count + nodes.Count < laneCount)
            {
                return null;
            }
            int i = 0;
            while (nodes.Count < laneCount)
            {
                nodes.Add(candidates[i++].Node);
            }
            nodes.Sort();
            return nodes;
        }
        else
        {
            return null;
        }

        # region extracted functions
        // Attempt to copy laneCount candidates into resulting list
        List<Node> UnwrapCandidates(List<Candidate> candidates)
        {
            List<Node> result = new();
            for (int i = 0; i < Math.Min(laneCount, candidates.Count); i++)
            {
                result.Add(candidates[i].Node);
            }
            return result;
        }

        void AddNodeIfWithinSnap(Node n)
        {
            float distance = Vector3.Distance(clickPos, n.Pos);
            if (distance < snapRadius)
            {
                candidates.Add(new(distance, n));
            }
        }
        #endregion
    }
    private class Candidate : IComparable<Candidate>
    {
        public float Distance { get; set; }
        public Node Node { get; set; }
        public Candidate(float distance, Node node)
        {
            Distance = distance;
            Node = node;
        }

        public int CompareTo(Candidate other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }
}

public enum Side
{
    Start,
    End
}