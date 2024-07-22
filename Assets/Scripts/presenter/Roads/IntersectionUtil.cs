using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;
using UnityEngine.UIElements;

/// <summary>
/// Evaluates and updates the outlines and paths for the given intersection.
/// </summary>
public static class IntersectionUtil
{

    public static void EvaluateOutline(Intersection ix)
    {
        foreach (Road r in ix.OutRoads)
        {
            r.LeftOutline.Start = GetOutlineForRoad(r, Orientation.Left, Direction.Out, Side.Start);
            r.RightOutline.Start = GetOutlineForRoad(r, Orientation.Right, Direction.Out, Side.Start);
        }

        foreach (Road r in ix.InRoads)
        {
            r.LeftOutline.End = GetOutlineForRoad(r, Orientation.Left, Direction.In, Side.End);
            r.RightOutline.End = GetOutlineForRoad(r, Orientation.Right, Direction.In, Side.End);
        }

        foreach (Road r in ix.Roads)
            Game.InvokeRoadUpdated(r);
        

        List<float3> GetOutlineForRoad(Road road, Orientation orientation, Direction direction, Side side)
        {
            List<float3> outline = GetPath(road, orientation, direction)?.GetOutline(orientation);
            if (outline != null)
                return SeparateOutlineWithEndofRoad(outline, direction);
            else
                return GetOutLineAtTwoEnds(road, orientation, side);
            
        }

        Path GetPath(Road road, Orientation orientation, Direction direction)
        {
            List<Path> edges = null;
            if (direction == Direction.In)
                edges = Graph.GetOutPaths(road.Lanes[orientation == Orientation.Left ? 0 : road.Lanes.Count - 1].EndVertex);
            else if (direction == Direction.Out)
                edges = Graph.GetInPaths(road.Lanes[orientation == Orientation.Left ? 0 : road.Lanes.Count - 1].StartVertex);

            if (edges != null && edges.Count() != 0)
            {
                if (direction == Direction.In)
                {
                    if (orientation == Orientation.Left)
                        return edges.OrderBy(e => Component(-ix.Normal, e.Target.Pos)).First();
                    else
                        return edges.OrderBy(e => Component(-ix.Normal, e.Target.Pos)).Last();
                }
                if (direction == Direction.Out)
                {
                    if (orientation == Orientation.Left)
                        return edges.OrderBy(e => Component(-ix.Normal, e.Source.Pos)).First();
                    else
                        return edges.OrderBy(e => Component(-ix.Normal, e.Source.Pos)).Last();
                }
            }
            return null;
        }

        static float Component(float3 u, float3 v)
        {
            return math.dot(u, v) / math.length(v);
        }

        List<float3> SeparateOutlineWithEndofRoad(List<float3> interRoadOutline, Direction direction)
        {
            if (direction != Direction.In && direction != Direction.Out)
                throw new ArgumentException("direction");
            List<float3> outlineEnd = new();
            List<float3> outlineStart = new();
            bool crossed = false;
            float3 prevPt = 0;
            foreach (float3 pt in interRoadOutline)
            {
                if (!crossed)
                    if (ix.Plane.SameSide(pt, ix.PointOnInSide))
                        outlineEnd.Add(pt);
                    else
                    {
                        crossed = true;
                        Ray ray = new(pt, prevPt - pt);
                        ix.Plane.Raycast(ray, out float distance);
                        float3 commonPt = ray.GetPoint(distance);
                        outlineEnd.Add(commonPt);
                        outlineStart.Add(commonPt);
                        outlineStart.Add(pt);
                    }
                else
                    outlineStart.Add(pt);
                prevPt = pt;
            }
            return direction == Direction.In ? outlineEnd : outlineStart;
        }

        static List<float3> GetOutLineAtTwoEnds(Road road, Orientation orientation, Side side)
        {
            List<float3> results = new();
            Lane lane = orientation == Orientation.Left ? road.Lanes.First() : road.Lanes.Last();
            float normalMultiplier = Constants.RoadOutlineSeparation;
            if (orientation == Orientation.Right)
                normalMultiplier *= -1;
            int numPoints = (int)(Constants.VertexDistanceFromRoadEnds * Constants.MeshResolution) + 1;
            Curve bs = lane.Curve;
            float interpolationOfLocation;
            if (side == Side.Start)
                interpolationOfLocation = lane.Curve.GetDistanceToInterpolation(Constants.VertexDistanceFromRoadEnds);
            else
                interpolationOfLocation = lane.Curve.GetDistanceToInterpolation(lane.Curve.Length - Constants.VertexDistanceFromRoadEnds);
            for (int i = 0; i <= numPoints; i++)
            {
                float t;
                if (side == Side.Start)
                    t = (float)i / numPoints * interpolationOfLocation;
                else
                    t = interpolationOfLocation + (float)i / numPoints * (1 - interpolationOfLocation);
                float3 normal = bs.Evaluate2DNormalizedNormal(t);
                results.Add(bs.EvaluatePosition(t) + normal * normalMultiplier);
            }
            return results;
        }
    }

    public static void EvaluatePaths(Intersection ix)
    {
        ClearAllPaths();

        if (ix.InRoads.Count == 0 || ix.OutRoads.Count == 0)
            return;

        BuildStraightPaths(ix);
        BuildLaneChangingPaths(ix);
        AutoMergeOrExpandLanes(ix);

        void BuildStraightPaths(Intersection ix)
        {
            foreach (Node node in ix.Nodes)
                if (node.InLane != null && node.OutLane != null)
                {
                    Curve left = node.InLane.Curve.Duplicate();
                    left.AddStartDistance(node.InLane.Curve.Length - Constants.VertexDistanceFromRoadEnds);
                    Curve right = node.OutLane.Curve.Duplicate();
                    right.AddEndDistance(node.InLane.Curve.Length - Constants.VertexDistanceFromRoadEnds);
                    left.Add(right);
                    Path path = new(left, node.InLane.EndVertex, node.OutLane.StartVertex);
                    Graph.AddPath(path);
                }
            
        }

        void BuildLaneChangingPaths(Intersection ix)
        {
            for (int j = 1; j < ix.Nodes.Count; j++)
            {
                Node currentNode = ix.Nodes[j];
                Node previousNode = ix.Nodes[j - 1];
                if (NodesBelongToUniqueRoad(currentNode, previousNode))
                {
                    Path leftPath = BuildPathLane2Lane(currentNode.InLane, previousNode.OutLane);
                    Path rightPath = BuildPathLane2Lane(previousNode.InLane, currentNode.OutLane);
                    leftPath.InterweavingPath = rightPath;
                    rightPath.InterweavingPath = leftPath;
                }
            }
        }

        void AutoMergeOrExpandLanes(Intersection ix)
        {
            foreach (Node n in ix.Nodes)
            {
                if (n.OutLane == null)
                {
                    if (n.NodeIndex < ix.LastNodeWithRoad(Direction.Out).NodeIndex && n.NodeIndex > ix.FirstNodeWithRoad(Direction.Out).NodeIndex)
                        continue;
                    Node targetNode = Math.Abs(n.NodeIndex - ix.FirstNodeWithRoad(Direction.Out).NodeIndex) < Math.Abs(n.NodeIndex - ix.LastNodeWithRoad(Direction.Out).NodeIndex) ?
                        ix.FirstNodeWithRoad(Direction.Out) : ix.LastNodeWithRoad(Direction.Out);
                    BuildPathNode2Node(n, targetNode);
                }

                if (n.InLane == null)
                {
                    if (n.NodeIndex < ix.LastNodeWithRoad(Direction.In).NodeIndex && n.NodeIndex > ix.FirstNodeWithRoad(Direction.In).NodeIndex)
                        continue;
                    Node targetNode = Math.Abs(n.NodeIndex - ix.FirstNodeWithRoad(Direction.In).NodeIndex) < Math.Abs(n.NodeIndex - ix.LastNodeWithRoad(Direction.In).NodeIndex) ?
                        ix.FirstNodeWithRoad(Direction.In) : ix.LastNodeWithRoad(Direction.In);
                    BuildPathNode2Node(targetNode, n);
                }
            }
        }

        void ClearAllPaths()
        {
            List<Path> pathsToRemove = new();
            foreach (Road road in ix.InRoads)
                foreach (Lane lane in road.Lanes)
                    pathsToRemove.AddRange(Graph.GetOutPaths(lane.EndVertex));
                
            foreach (Path path in pathsToRemove)
                Graph.RemovePath(path);
            
        }

        bool NodesBelongToUniqueRoad(Node node1, Node node2)
        {
            if (node1.InLane == null || node1.OutLane == null || node2.InLane == null || node2.OutLane == null)
                return false;
            return node1.InLane.Road == node2.InLane.Road && node1.OutLane.Road == node2.OutLane.Road;
        }

        void BuildPathNode2Node(Node node1, Node node2)
        {
            if (node1.InLane != null && node2.OutLane != null)
                BuildPathLane2Lane(node1.InLane, node2.OutLane);
            
        }

        Path BuildPathLane2Lane(Lane lane1, Lane lane2)
        {
            return BuildPath(lane1.EndVertex, lane2.StartVertex);
        }

        Path BuildPath(Vertex start, Vertex end)
        {
            Path existingPath = Graph.GetPath(start, end);
            if (existingPath != null)
                return null;

            float3 pos1 = start.Pos + Constants.MinLaneLength / 3 * start.Tangent;
            float3 pos2 = end.Pos - Constants.MinLaneLength / 3 * end.Tangent;
            Curve Curve = new(new BezierCurve(start.Pos, pos1, pos2, end.Pos));
            Path newPath = new(Curve, start, end);
            Graph.AddPath(newPath);
            return newPath;
        }
    }
}