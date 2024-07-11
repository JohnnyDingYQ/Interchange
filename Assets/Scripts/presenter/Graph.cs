using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using Unity.Mathematics;
using UnityEngine.Assertions;

public static class Graph
{
    private static AdjacencyGraph<Vertex, Path> graph = new();

    public static void Wipe()
    {
        graph = new();
    }

    /// <summary>
    /// Should not be invoked outside of Game class
    /// </summary>
    public static void AddVertex(Vertex v)
    {
        graph.AddVertex(v);
    }

    /// <summary>
    /// Should not be invoked outside of Game class
    /// </summary>
    public static void AddPath(Path p)
    {
        graph.AddEdge(p);
    }

    /// <summary>
    /// Should not be invoked outside of Game class
    /// </summary>
    public static void RemoveVertex(Vertex v)
    {
        graph.RemoveVertex(v);
    }

    /// <summary>
    /// Should not be invoked outside of Game class
    /// </summary>
    public static void RemovePath(Path p)
    {
        graph.RemoveEdge(p);
    }

    public static bool ContainsVertex(Vertex v)
    {
        return graph.ContainsVertex(v);
    }

    public static bool ContainsPath(Path p)
    {
        return graph.ContainsEdge(p);
    }

    public static bool ContainsPath(Lane from, Lane to)
    {
        return ContainsPath(from.EndVertex, to.StartVertex) || ContainsPath(to.EndVertex, from.StartVertex);
    }
    public static bool ContainsPath(Vertex from, Vertex to)
    {
        return graph.ContainsEdge(from, to);
    }

    public static void AddVerticesAndPath(Path p)
    {
        graph.AddVerticesAndEdge(p);
    }

    public static List<Path> GetOutPaths(Vertex v)
    {
        graph.TryGetOutEdges(v, out IEnumerable<Path> edges);
        if (edges == null)
            return new();
        return edges.ToList();
    }

    public static IEnumerable<Path> ShortestPathAStar(Vertex start, Vertex end)
    {
        Assert.IsNotNull(start);
        Assert.IsNotNull(end);
        TryFunc<Vertex, IEnumerable<Path>> tryFunc = graph.ShortestPathsAStar(
        (Path p) => p.Length,
        (Vertex to) => math.distance(start.Pos, to.Pos),
        start
    );
        tryFunc(end, out IEnumerable<Path> paths);
        return paths;
    }

    public static List<Path> GetInPaths(Vertex vertex)
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
        return p.ToList();
    }

    public static Path GetPath(Vertex start, Vertex end)
    {
        graph.TryGetEdge(start, end, out Path edge);
        return edge;
    }
}