using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;
using Assets.Scripts.Model.Roads;

/// <summary>
/// Evaluates and updates the outlines and edges for the given intersection.
/// </summary>
public static class IntersectionUtil
{

    public static void EvaluateOutline(Intersection ix)
    {
        foreach (Road r in ix.OutRoads)
        {
            r.LeftOutline.StartCurve = GetOutlineCurve(r, Orientation.Left, Direction.Out);
            r.RightOutline.StartCurve = GetOutlineCurve(r, Orientation.Right, Direction.Out);
        }

        foreach (Road r in ix.InRoads)
        {
            r.LeftOutline.EndCurve = GetOutlineCurve(r, Orientation.Left, Direction.In);
            r.RightOutline.EndCurve = GetOutlineCurve(r, Orientation.Right, Direction.In);
        }

        Curve GetOutlineCurve(Road road, Orientation orientation, Direction direction)
        {
            Edge p = GetEdge(road, orientation, direction);
            if (p != null)
            {
                p.Curve.GetNearestPoint(new(ix.Nodes.Last().Pos, ix.Normal), out float distanceOnCurve);
                Curve curve = p.Curve.Duplicate();
                if (orientation == Orientation.Left)
                    curve.Offset(Constants.RoadOutlineSeparation);
                else
                    curve.Offset(-Constants.RoadOutlineSeparation);

                if (direction == Direction.Out)
                    return curve.AddStartDistance(distanceOnCurve);
                else
                    return curve.AddEndDistance(p.Curve.Length - distanceOnCurve);
            }
            else
            {
                Curve curve;

                if (orientation == Orientation.Left)
                    curve = road.Lanes.First().Curve.Duplicate().Offset(Constants.RoadOutlineSeparation);
                else
                    curve = road.Lanes.Last().Curve.Duplicate().Offset(-Constants.RoadOutlineSeparation);

                if (direction == Direction.Out)
                    return curve.AddEndDistance(curve.Length - Constants.VertexDistanceFromRoadEnds);
                else
                    return curve.AddStartDistance(curve.Length - Constants.VertexDistanceFromRoadEnds);
            }

        }

        Edge GetEdge(Road road, Orientation orientation, Direction direction)
        {
            IEnumerable<Edge> edges = null;
            if (direction == Direction.In)
                edges = Graph.OutEdges(road.Lanes[orientation == Orientation.Left ? 0 : road.Lanes.Count - 1].EndVertex);
            else if (direction == Direction.Out)
                edges = Graph.InEdges(road.Lanes[orientation == Orientation.Left ? 0 : road.Lanes.Count - 1].StartVertex);

            if (edges != null && edges.Count() != 0)
            {
                if (direction == Direction.In)
                {
                    if (orientation == Orientation.Left)
                        return edges.OrderBy(e => e.Target.Lane.LaneIndex).First();
                    else
                        return edges.OrderBy(e => e.Target.Lane.LaneIndex).Last();
                }
                if (direction == Direction.Out)
                {
                    if (orientation == Orientation.Left)
                        return edges.OrderBy(e => e.Source.Lane.LaneIndex).First();
                    else
                        return edges.OrderBy(e => e.Source.Lane.LaneIndex).Last();
                }
            }
            return null;
        }
    }

    public static void EvaluateEdges(Intersection ix)
    {
        ClearAllEdges();

        if (ix.InRoads.Count == 0 || ix.OutRoads.Count == 0)
            return;

        BuildStraightEdges(ix);
        BuildLaneChangingEdges(ix);
        AutoMergeOrExpandLanes(ix);

        void BuildStraightEdges(Intersection ix)
        {
            foreach (Node node in ix.Nodes)
                if (node.InLane != null && node.OutLane != null)
                {
                    Curve left = node.InLane.Curve.Duplicate();
                    left = left.AddStartDistance(left.Length - Constants.VertexDistanceFromRoadEnds);
                    Curve right = node.OutLane.Curve.Duplicate();
                    right = right.AddEndDistance(right.Length - Constants.VertexDistanceFromRoadEnds);
                    left.Add(right);
                    Edge edge = new(left, node.InLane.EndVertex, node.OutLane.StartVertex);
                    Graph.AddEdge(edge);
                }

        }

        void BuildLaneChangingEdges(Intersection ix)
        {
            for (int j = 1; j < ix.Nodes.Count; j++)
            {
                Node currentNode = ix.Nodes[j];
                Node previousNode = ix.Nodes[j - 1];
                if (NodesBelongToUniqueRoad(currentNode, previousNode))
                {
                    Edge leftEdge = BuildEdgeLane2Lane(currentNode.InLane, previousNode.OutLane);
                    Edge rightEdge = BuildEdgeLane2Lane(previousNode.InLane, currentNode.OutLane);
                    leftEdge.InterweavingEdge = rightEdge;
                    rightEdge.InterweavingEdge = leftEdge;
                    Assert.IsTrue(Game.Edges.Values.Contains(leftEdge));
                    Assert.IsTrue(Game.Edges.Values.Contains(rightEdge));
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
                    BuildEdgeNode2Node(n, targetNode);
                }

                if (n.InLane == null)
                {
                    if (n.NodeIndex < ix.LastNodeWithRoad(Direction.In).NodeIndex && n.NodeIndex > ix.FirstNodeWithRoad(Direction.In).NodeIndex)
                        continue;
                    Node targetNode = Math.Abs(n.NodeIndex - ix.FirstNodeWithRoad(Direction.In).NodeIndex) < Math.Abs(n.NodeIndex - ix.LastNodeWithRoad(Direction.In).NodeIndex) ?
                        ix.FirstNodeWithRoad(Direction.In) : ix.LastNodeWithRoad(Direction.In);
                    BuildEdgeNode2Node(targetNode, n);
                }
            }
        }

        void ClearAllEdges()
        {
            List<Edge> edgesToRemove = new();
            foreach (Road road in ix.InRoads)
                foreach (Lane lane in road.Lanes)
                    edgesToRemove.AddRange(Graph.OutEdges(lane.EndVertex));

            foreach (Edge edge in edgesToRemove)
                Graph.RemoveEdge(edge);

        }

        bool NodesBelongToUniqueRoad(Node node1, Node node2)
        {
            if (node1.InLane == null || node1.OutLane == null || node2.InLane == null || node2.OutLane == null)
                return false;
            return node1.InLane.Road == node2.InLane.Road && node1.OutLane.Road == node2.OutLane.Road;
        }

        void BuildEdgeNode2Node(Node node1, Node node2)
        {
            if (node1.InLane != null && node2.OutLane != null)
                BuildEdgeLane2Lane(node1.InLane, node2.OutLane);

        }

        Edge BuildEdgeLane2Lane(Lane lane1, Lane lane2)
        {
            return BuildEdge(lane1.EndVertex, lane2.StartVertex);
        }

        Edge BuildEdge(Vertex start, Vertex end)
        {
            Edge existingEdge = Graph.GetEdge(start, end);
            if (existingEdge != null)
                return null;

            float3 pos1 = start.Pos + Constants.MinLaneLength / 3 * start.Tangent;
            float3 pos2 = end.Pos - Constants.MinLaneLength / 3 * end.Tangent;
            Curve Curve = new(new BezierCurve(start.Pos, pos1, pos2, end.Pos));
            Edge newEdge = new(Curve, start, end);
            Graph.AddEdge(newEdge);
            return newEdge;
        }
    }
}