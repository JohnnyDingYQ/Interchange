using QuikGraph;

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
                    inDegree ++;
                }
            }
            return inDegree;
        }
    }
}