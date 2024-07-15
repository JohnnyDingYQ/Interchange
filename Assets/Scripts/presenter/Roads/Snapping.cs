using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public static class Snapping
{
    static readonly List<float3> bufferList = new();
    public static BuildTargets Snap(float3 pos, int laneCount)
    {
        BuildTargets bt = new()
        {
            ClickPos = pos,
            Nodes = GetBuildNodes(pos, laneCount),
            SelectedRoad = Game.HoveredRoad,
            Pos = pos,
            Snapped = false
        };
        if (bt.Nodes == null)
        {
            if (bt.SelectedRoad != null)
            {
                float interpolation = bt.SelectedRoad.GetNearestInterpolation(bt.ClickPos);
                AttemptDivide(laneCount, bt, interpolation);
                if (!bt.DivideIsPossible)
                {
                    Debug.Log(CombineThenDivideIsValid(laneCount, bt, interpolation));
                }
            }
        }
        else
            SetupSnapInfo(bt);
        return bt;

        static void SetupDivideInfo(int laneCount, BuildTargets bt, float interpolation)
        {
            bt.Pos = bt.SelectedRoad.EvaluatePosition(interpolation);
            bt.NodesPosIfDivded = new();
            float3 offset = bt.SelectedRoad.Evaluate2DNormalizedNormal(interpolation);

            bufferList.Clear();
            for (int i = 0; i < bt.SelectedRoad.LaneCount; i++)
            {
                float3 pos = bt.SelectedRoad.Lanes[i].EvaluatePosition(interpolation);
                bufferList.Add(pos);
            }
            float3 center = bufferList.OrderBy(p => math.length(p - bt.ClickPos)).First();

            int interpolateRange = Constants.MaxLaneCount / 2 + 1;
            for (int i = interpolateRange; i >= -interpolateRange; i--)
            {
                float3 p = center + offset * i * Constants.LaneWidth;
                bt.NodesPosIfDivded.Add(p);
            }
            bt.Pos = center;
            bt.NodesPosIfDivded = bt.NodesPosIfDivded.OrderBy(p => math.length(p - bt.ClickPos)).Take(laneCount).ToList();
            bt.TangentAssigned = true;
            bt.Tangent = math.normalize(bt.SelectedRoad.EvaluateTangent(interpolation));
        }

        static void AttemptDivide(int laneCount, BuildTargets bt, float interpolation)
        {   
            bt.DivideIsPossible = Divide.RoadIsDividable(bt.SelectedRoad, interpolation);
            if (bt.DivideIsPossible)
                SetupDivideInfo(laneCount, bt, interpolation);
        }

        static bool CombineThenDivideIsValid(int laneCount, BuildTargets bt, float interpolation)
        {
            bool lookLeft = false;
            if (interpolation <= 0.5)
                lookLeft = true;
            Intersection nearestIx = lookLeft ? bt.SelectedRoad.StartIntersection : bt.SelectedRoad.EndIntersection;
            if (!Combine.CombineIsValid(nearestIx))
                return false;
            Road left = nearestIx.InRoads.Single();
            Road right = nearestIx.OutRoads.Single();
            for (int i = 0; i < left.LaneCount; i++)
            {
                float leftLength = left.Lanes[i].Length;
                float rightLength = right.Lanes[i].Length;
                float totalLength = leftLength + rightLength;
                float newDivide = lookLeft ? leftLength + rightLength * interpolation : rightLength * interpolation;
                if (newDivide < Constants.MinLaneLength || newDivide > Constants.MaxLaneLength)
                    return false;
                if (totalLength - newDivide < Constants.MinLaneLength || totalLength - newDivide > Constants.MaxLaneLength)
                    return false;
            }
            return true;
        }

        static void SetupSnapInfo(BuildTargets bt)
        {
            bt.Snapped = true;
            bt.Pos = Vector3.Lerp(bt.Nodes.First().Pos, bt.Nodes.Last().Pos, 0.5f);
            bt.Intersection = bt.Nodes.First().Intersection;
            if (!bt.Intersection.IsRoadEmpty())
            {
                bt.TangentAssigned = true;
                bt.Tangent = bt.Intersection.Tangent;
            }
        }
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