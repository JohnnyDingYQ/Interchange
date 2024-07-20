using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public static class Snapping
{
    public static BuildTargets Snap(float3 pos, int laneCount, Side side)
    {
        BuildTargets bt = new()
        {
            ClickPos = pos,
            SelectedRoad = Game.HoveredRoad,
            Pos = pos,
            Snapped = false
        };

        List<Node> nodes = Game.Nodes.Values
            .Where(node => WithinSnapRange(pos, node.Pos, laneCount))
            .OrderBy(node => math.length(pos - node.Pos))
            .Take(laneCount)
            .OrderBy(node => node.NodeIndex)
            .ToList();

        if (nodes.Count == 0)
            return bt;
        
        if (side == Side.Start && nodes.Any(node => node.OutLane != null))
            return bt;
        
        
        if (side == Side.End && nodes.Any(node => node.InLane != null))
            return bt;

        if (laneCount == 1 && nodes.First().BelongsToPoint)
        {
            bt.Snapped = true;
            bt.Offset = 0;
            bt.Intersection = nodes.First().Intersection;
            return bt;
        }

        if (nodes.Any(node => node.BelongsToPoint))
            return bt;

        int index = nodes.First().NodeIndex;
        float3 offset = nodes.First().Intersection.Normal * Constants.LaneWidth;
        float3 interpolatedPos = nodes.First().Pos + offset;
        if (nodes.Count < laneCount)
            ExtrapolateToTheLeft();
        interpolatedPos -= offset;
        SetupSnapInfo();
        return bt;

        void ExtrapolateToTheLeft()
        {
            while (WithinSnapRange(pos, interpolatedPos, laneCount))
            {
                index--;
                interpolatedPos += offset;
            }
        }

        void SetupSnapInfo()
        {
            bt.Offset = index;
            bt.Snapped = true;
            bt.Pos = interpolatedPos - offset * ((float)(laneCount - 1) / 2);
            bt.Intersection = nodes.First().Intersection;
            bt.NodesPos = new();
            for (int i = 0; i < laneCount; i++)
            {
                bt.NodesPos.Add(interpolatedPos - offset * i);
            }
            if (!bt.Intersection.IsRoadEmpty())
            {
                bt.TangentAssigned = true;
                bt.Tangent = bt.Intersection.Tangent;
            }
        }
    }

    static bool WithinSnapRange(float3 center, float3 pos, int laneCount)
    {
        float snapRadius = (laneCount * Constants.LaneWidth + Constants.BuildSnapTolerance) / 2;
        return Vector3.Distance(center, pos) < snapRadius;
    }
}