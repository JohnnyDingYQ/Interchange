using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms;
using Unity.Mathematics;

namespace GraphExtensions
{
    public static class MyExtension
    {
        public static int InDegree(this AdjacencyGraph<Vertex, Path> graph, Vertex vertex)
        {
            int inDegree = 0;
            foreach (Vertex v in graph.Vertices)
            {
                foreach (IEdge<Vertex> e in graph.OutEdges(v))
                {
                    if (e.Target.GetHashCode() == vertex.GetHashCode())
                        inDegree++;
                }
            }
            return inDegree;
        }

        public static IEnumerable<Path> ShortestPathAStar(this AdjacencyGraph<Vertex, Path> graph, Vertex start, Vertex end)
        {
            TryFunc<Vertex, IEnumerable<Path>> tryFunc = graph.ShortestPathsAStar(
            (Path p) => p.Length,
            (Vertex to) => math.distance(start.Pos, to.Pos),
            start
        );
            tryFunc(end, out IEnumerable<Path> paths);
            return paths;
        }

    }
}