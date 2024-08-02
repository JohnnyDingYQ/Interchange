using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using Unity.Mathematics;
using UnityEngine.Assertions;
using Interchange;

public static class Graph
{
    private static AdjacencyGraph<Vertex, Edge> graph = new();


    static Graph()
    {
        ApplyBinding();
    }

    public static void ApplyBinding()
    {
        graph.VertexAdded += RegisterVertex;
        graph.EdgeAdded += RegisterEdge;
        graph.VertexRemoved += UnregisterVertex;
        graph.EdgeRemoved += UnregisterEdge;
    }

    public static void CancelBinding()
    {
        graph.VertexAdded -= RegisterVertex;
        graph.EdgeAdded -= RegisterEdge;
        graph.VertexRemoved -= UnregisterVertex;
        graph.EdgeRemoved -= UnregisterEdge;
    }

    static void RegisterVertex(Vertex v)
    {
        Game.Vertices[v.Id] = v;
    }

    static void RegisterEdge(Edge edge)
    {
        Game.Edges[edge.Id] = edge;
        Game.RegisterCurve(edge.Curve);
    }

    static void UnregisterVertex(Vertex v)
    {
        Game.Vertices.Remove(v.Id);
    }

    static void UnregisterEdge(Edge p)
    {
        Game.Edges.Remove(p.Id);
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

    public static bool AddEdge(Edge p)
    {
        if (!Game.Edges.ContainsKey(p.Id))
            p.Id = Game.FindNextAvailableKey(Game.Edges.Keys);
        return graph.AddEdge(p);
    }

    public static bool RemoveVertex(Vertex v)
    {
        return graph.RemoveVertex(v);
    }

    public static bool RemoveEdge(Edge p)
    {
        return graph.RemoveEdge(p);
    }


    public static bool ContainsVertex(Vertex v)
    {
        return graph.ContainsVertex(v);
    }

    public static bool ContainsEdge(Edge p)
    {
        return graph.ContainsEdge(p);
    }

    public static bool ContainsEdge(Lane from, Lane to)
    {
        return ContainsEdge(from.EndVertex, to.StartVertex) || ContainsEdge(to.EndVertex, from.StartVertex);
    }
    public static bool ContainsEdge(Vertex from, Vertex to)
    {
        return graph.ContainsEdge(from, to);
    }

    public static void AddVerticesAndEdgeRange(IEnumerable<Edge> edges)
    {
        graph.AddVerticesAndEdgeRange(edges);
    }

    public static List<Edge> GetOutEdges(Vertex v)
    {
        graph.TryGetOutEdges(v, out IEnumerable<Edge> edges);
        if (edges == null)
            return new();
        return edges.ToList();
    }

    public static IEnumerable<Edge> ShortestPathAStar(Vertex start, Vertex end)
    {
        Assert.IsNotNull(start);
        Assert.IsNotNull(end);
        TryFunc<Vertex, IEnumerable<Edge>> tryFunc = graph.ShortestPathsAStar(
            (Edge p) => p.Length,
            (Vertex to) => math.distance(start.Pos, to.Pos),
            start
        );
        tryFunc(end, out IEnumerable<Edge> edges);
        return edges;
    }

    public static List<Edge> GetInEdges(Vertex vertex)
    {
        HashSet<Edge> p = new();
        foreach (Vertex v in graph.Vertices)
        {
            foreach (Edge e in graph.OutEdges(v))
            {
                if (e.Target.GetHashCode() == vertex.GetHashCode())
                    p.Add(e);
            }
        }
        return p.ToList();
    }

    public static Edge GetEdge(Vertex start, Vertex end)
    {
        graph.TryGetEdge(start, end, out Edge edge);
        return edge;
    }
}