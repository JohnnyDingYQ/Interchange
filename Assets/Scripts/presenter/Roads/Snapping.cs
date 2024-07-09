using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public static class Snapping
{
    public static BuildTargets Snap(float3 pos, int laneCount)
    {
        BuildTargets bt = new()
        {
            ClickPos = pos,
            Nodes = GetBuildNodes(pos, laneCount),
            SelectedRoad = Game.HoveredRoad
        };
        if (bt.Nodes == null)
        {
            bt.Snapped = false;
            if (bt.SelectedRoad != null)
            {
                float interpolation = Divide.GetInterpolation(bt.SelectedRoad, bt.ClickPos);
                bt.DividePossible = Divide.RoadDividable(bt.SelectedRoad, interpolation);
                if (bt.DividePossible)
                {
                    bt.NodesIfDivded = new();
                    float3 center = bt.SelectedRoad.BezierSeries.EvaluatePosition(interpolation);
                    float3 offset = bt.SelectedRoad.BezierSeries.Evaluate2DNormalizedNormal(interpolation);
                    for (int i = 2; i >= -2; i--)
                    {
                        float3 p = center + offset * i * Constants.LaneWidth;
                        bt.NodesIfDivded.Add(p);
                    }
                    bt.MedianPoint = center;
                    bt.NodesIfDivded = bt.NodesIfDivded.OrderBy(p => math.length(p - bt.ClickPos)).Take(laneCount).ToList();
                    bt.TangentAssigned = true;
                    bt.Tangent = math.normalize(bt.SelectedRoad.BezierSeries.EvaluateTangent(interpolation));
                }
            }
        }
        else
        {
            bt.Snapped = true;
            bt.MedianPoint = Vector3.Lerp(bt.Nodes.First().Pos, bt.Nodes.Last().Pos, 0.5f);
            bt.Intersection = bt.Nodes.First().Intersection;
            if (!bt.Intersection.IsRoadEmpty())
            {
                bt.TangentAssigned = true;
                bt.Tangent = bt.Intersection.Tangent;
            }
        }
        return bt;
    }

    static List<Node> GetBuildNodes(float3 clickPos, int laneCount)
    {
        float snapRadius = (laneCount * Constants.LaneWidth + Constants.BuildSnapTolerance) / 2;
        List<Tuple<float, Node>> candidates = new();
        foreach (Node node in Game.Nodes.Values)
            AddNodeIfWithinSnap(node);

        if (candidates.Count == 0)
            return null;

        // this sorts nodes with ascending order of distance (descending proximity)
        List<Node> nodes = candidates.OrderBy(t => t.Item1).Select(t => t.Item2).ToList();
        while (nodes.Count > laneCount)
            nodes.Remove(nodes.Last());

        // this sorts nodes with their order in the road, left to right in the direction of the road
        nodes.Sort();

        // check all snapped nodes belong to the same intersection
        Intersection intersection = nodes.First().Intersection;
        foreach (Node n in nodes)
        {
            Assert.IsNotNull(n.Intersection);
            if (n.Intersection != intersection)
                return null;
        }

        if (nodes.Count == laneCount)
            return nodes;
        else if (nodes.Count == 1 && nodes.First().BelongsToPoint)
            return null;
        else
        {
            candidates = new();
            GetInterpolatedCandidates(2, intersection);
            if (candidates.Count + nodes.Count < laneCount)
                return null;

            nodes.AddRange(candidates.Select(t => t.Item2).Take(laneCount - nodes.Count));
            nodes.Sort();
            return nodes;
        }

        # region extracted functions
        void GetInterpolatedCandidates(int interpolationReach, Intersection intersection)
        {
            float3 normal = intersection.Normal * Constants.LaneWidth;
            for (int i = 1; i <= interpolationReach; i++)
            {
                float3 left = nodes.First().Pos + i * normal;
                float3 right = nodes.Last().Pos - i * normal;
                AddNodeIfWithinSnap(new(left, nodes.First().Pos.y, nodes.First().NodeIndex - i)
                {
                    Intersection = intersection
                });
                AddNodeIfWithinSnap(new(right, nodes.Last().Pos.y, nodes.Last().NodeIndex + i)
                {
                    Intersection = intersection
                });
            }
        }

        void AddNodeIfWithinSnap(Node n)
        {
            float distance = Vector3.Distance(clickPos, n.Pos);
            if (distance < snapRadius)
                candidates.Add(new(distance, n));
        }
        #endregion
    }
}