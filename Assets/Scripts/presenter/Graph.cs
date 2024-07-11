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

    static void ApplyBinding()
    {
        graph.VertexAdded += (v) => {
            v.Id = Game.FindNextAvailableKey(Game.Vertices.Keys);
            Game.Vertices[v.Id] = v;
        };
        graph.EdgeAdded += (p) => {
            p.Id = Game.FindNextAvailableKey(Game.Paths.Keys);
            Game.Paths[p.Id] = p;
        };
        graph.VertexRemoved += (v) => Game.Vertices.Remove(v.Id);;
        graph.EdgeRemoved += (p) => Game.Paths.Remove(p.Id);
    }

    public static void Wipe()
    {
        graph = new();
        ApplyBinding();
    }

    public static bool AddVertex(Vertex v)
    {
        return graph.AddVertex(v);
    }

    public static bool AddPath(Path p)
    {
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