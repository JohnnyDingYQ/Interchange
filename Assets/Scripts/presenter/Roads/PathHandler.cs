using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Splines;

public static class PathHandler
{
    public static void BuildAllPaths(List<Lane> to, List<Node> from, Direction laneDirection)
    {
        int laneCount = to.Count;
        BuildStraightPath();
        BuildRightLaneChangePath();
        BuildLeftLaneChangePath();
        BuildSidePaths();

        void BuildStraightPath()
        {
            for (int i = 0; i < laneCount; i++)
                foreach (Lane lane in from[i].GetLanes(InvertDirection(laneDirection)))
                    BuildPath(lane, to[i], laneDirection);
        }
        void BuildRightLaneChangePath()
        {
            for (int i = 1; i < laneCount; i++)
                foreach (Lane lane in from[i - 1].GetLanes(InvertDirection(laneDirection)))
                    BuildPath(lane, to[i], laneDirection);
        }
        void BuildLeftLaneChangePath()
        {
            for (int i = 0; i < laneCount - 1; i++)
                foreach (Lane lane in from[i + 1].GetLanes(InvertDirection(laneDirection)))
                    BuildPath(lane, to[i], laneDirection);
        }
        void BuildSidePaths()
        {
            int leftNodeOrder = from.First().Order - 1;
            int rightNodeOrder = from.Last().Order + 1;
            HashSet<Road> roads = new();
            for (int i = 0; i < laneCount; i++)
                foreach (Lane lane in from[i].GetLanes(InvertDirection(laneDirection)))
                    roads.Add(lane.Road);
            foreach (Road road in roads)
                foreach (Lane lane in road.Lanes)
                {
                    Node node = laneDirection == Direction.Out ? lane.EndNode : lane.StartNode;
                    if (node.Order == leftNodeOrder)
                        BuildPath(lane, to.First(), laneDirection);
                    else if (node.Order == rightNodeOrder)
                        BuildPath(lane, to.Last(), laneDirection);
                }
        }
        static Direction InvertDirection(Direction direction)
        {
            if (direction == Direction.In)
                return Direction.Out;
            return Direction.In;
        }
    }

    static Path BuildPath(Lane l1, Lane l2, Direction l2Direction)
    {
        Path path;
        if (l2Direction == Direction.Out)
            path = BuildPath(l1.EndVertex, l2.StartVertex);
        else
            path = BuildPath(l2.EndVertex, l1.StartVertex);
        return path;
    }
    static Path BuildPath(Vertex start, Vertex end)
    {
        float3 pos1 = start.Pos + Constants.MinimumLaneLength / 4 * start.Tangent;
        float3 pos2 = end.Pos - Constants.MinimumLaneLength / 4 * end.Tangent;
        BezierCurve bezierCurve = new(start.Pos, pos1, pos2, end.Pos);
        ICurve curve = new BezierCurveAdapter(bezierCurve);
        Path p = new(curve, start, end);
        Game.AddEdge(p);
        return p;
    }
}