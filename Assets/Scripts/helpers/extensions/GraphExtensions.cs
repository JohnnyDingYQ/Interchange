using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms;

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

        public static IEnumerable<Path> GetInEdges(this AdjacencyGraph<Vertex, Path> graph, Vertex vertex)
        {
            HashSet<Path> p = new();
            foreach (Vertex v in graph.Vertices)
            {
                foreach (Path e in graph.OutEdges(v))
                {
                    if (e.Target.GetHashCode() == vertex.GetHashCode())
                        p.Add(e);
                }
            }
            return p;
        }

        /// <summary>
        /// Can be costly to invoke!
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static IEnumerable<Path> GetPathsFromAtoB(this AdjacencyGraph<Vertex, Path> graph, Vertex start, Vertex end)
        {
            static double edgeCost(Path path) => 1;
            TryFunc<Vertex, IEnumerable<Path>> tryGetPaths = graph.ShortestPathsDijkstra(edgeCost, start);

            tryGetPaths(end, out IEnumerable<Path> paths);
            return paths;
        }

    }
}