using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using Unity.Mathematics;
using UnityEngine.Assertions;

public static class Graph
{
    private static AdjacencyGraph<Vertex, Path> graph = new();


    static Graph()
    {
        ApplyBinding();
    }

    public static void ApplyBinding()
    {
        graph.VertexAdded += RegisterVertex;
        graph.EdgeAdded += RegisterPath;
        graph.VertexRemoved += UnregisterVertex;
        graph.EdgeRemoved += UnregisterPath;
    }

    public static void CancelBinding()
    {
        graph.VertexAdded -= RegisterVertex;
        graph.EdgeAdded -= RegisterPath;
        graph.VertexRemoved -= UnregisterVertex;
        graph.EdgeRemoved -= UnregisterPath;
    }

    static void RegisterVertex(Vertex v)
    {
        Game.Vertices[v.Id] = v;
    }

    static void RegisterPath(Path p)
    {
        Game.Paths[p.Id] = p;
        Game.RegisterCurve(p.Curve);
    }

    static void UnregisterVertex(Vertex v)
    {
        Game.Vertices.Remove(v.Id);
    }

    static void UnregisterPath(Path p)
    {
        Game.Paths.Remove(p.Id);
        Game.RemoveCurve(p.Curve);
    }

    public static void Wipe()
    {
        graph = new();
        ApplyBinding();
    }

    public static bool AddVertex(Vertex v)
    {
        if (!Game.Vertices.ContainsKey(v.Id))
            v.Id = Game.FindNextAvailableKey(Game.Vertices.Keys);
        return graph.AddVertex(v);
    }

    public static bool AddPath(Path p)
    {
        if (!Game.Paths.ContainsKey(p.Id))
            p.Id = Game.FindNextAvailableKey(Game.Paths.Keys);
        return graph.AddEdge(p);
    }

    public static bool RemoveVertex(Vertex v)
    {
        return graph.RemoveVertex(v);
    }

    public static bool RemovePath(Path p)
    {
        return graph.RemoveEdge(p);
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

    public static void AddVerticesAndPathRange(IEnumerable<Path> paths)
    {
        graph.AddVerticesAndEdgeRange(paths);
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