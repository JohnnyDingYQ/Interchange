using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public static class RoadMeshAssist
{
    public static void OutlineMesh(Road road)
    {
        Lane leftmost = road.Lanes.First();
        Lane rightmost = road.Lanes.Last();
        OutlineInnerRoad();
        OutlineInterRoad();

        # region extracted functions
        void OutlineInnerRoad()
        {
            int numPoints = (int)((road.Length - Constants.MinimumLaneLength) * Constants.MeshResolution);
            
            road.LeftOutline.Mid = GetOutline(leftmost.StartVertex, leftmost.EndVertex, numPoints, true);
            road.RightOutline.Mid = GetOutline(rightmost.StartVertex, rightmost.EndVertex, numPoints, false);
        }

        void OutlineInterRoad()
        {
            if (Game.Graph.TryGetOutEdges(leftmost.EndVertex, out IEnumerable<Path> lEdges))
            {
                int numPoints = (int) (Constants.MinimumLaneLength * Constants.MeshResolution);
                List<Path> l = new(lEdges);
                l.Sort();
                road.LeftOutline.Right = GetOutline(leftmost.EndVertex, l.First().Target, numPoints, true);
            }
            if (Game.Graph.TryGetOutEdges(leftmost.EndVertex, out IEnumerable<Path> rEdges))
            {
                int numPoints = (int) (Constants.MinimumLaneLength * Constants.MeshResolution);
                List<Path> l = new(rEdges);
                l.Sort();
                road.LeftOutline.Right = GetOutline(leftmost.EndVertex, l.First().Target, numPoints, true);
            }
        }

        static List<float3> GetOutline(Vertex start, Vertex end, int numPoints, bool isLeft)
        {
            List<float3> results = new();
            if (Game.Graph.TryGetEdge(start, end, out Path left))
            {
                for (int i = 0; i <= numPoints; i++)
                {
                    float t = (float)i / numPoints;
                    float3 normal = left.Evaluate2DNormal(t) * Constants.LaneWidth / 2;
                    normal.y = 0;
                    if (isLeft)
                        results.Add(left.EvaluatePosition(t) + normal);
                    else
                        results.Add(left.EvaluatePosition(t) - normal);
                }
                return results;
            }
            else
                throw new InvalidOperationException("fatal error: path not found");
        }
        # endregion
    }
}