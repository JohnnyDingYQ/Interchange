using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using Unity.Mathematics;
using UnityEngine.Assertions;
using Assets.Scripts.Model.Roads;

public static class Graph
{
    private static BidirectionalGraph<Vertex, Edge> graph = new();
    public static IEnumerable<Edge> Edges { get => graph.Edges; }
    public static int EdgeCount { get => graph.EdgeCount; }
    public static int VertexCount { get => graph.VertexCount; }


    static Graph()
    {
        ApplyBinding();
    }

    static void ApplyBinding()
    {
        graph.VertexAdded += RegisterVertex;
        graph.EdgeAdded += RegisterEdge;
        graph.VertexRemoved += UnregisterVertex;
        graph.EdgeRemoved += UnregisterEdge;
    }

    static void RegisterVertex(Vertex v)
    {
        if (Game.Vertices.ContainsKey(v.Id))
            return;
        Game.Vertices.Add(v.Id, v);
    }

    static void RegisterEdge(Edge edge)
    {
        if (Game.Edges.ContainsKey(edge.Id))
            return;
        Game.Edges.Add(edge.Id, edge);
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
        if (v.Id == 0)
            v.Id = Game.FindNextAvailableKey(Game.Vertices.Keys);
        bool status = graph.AddVertex(v);
        return status;
    }

    public static bool AddEdge(Edge p)
    {
        if (p.Id == 0)
            p.Id = Game.FindNextAvailableKey(Game.Edges.Keys);
        return graph.AddEdge(p);
    }

    public static bool RemoveVertex(Vertex v)
    {
        if (v.Id == 0)
            return false;
        return graph.RemoveVertex(v);
    }

    public static bool RemoveEdge(Edge p)
    {
        if (p.Id == 0)
            return false;
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

    public static IEnumerable<Edge> OutEdges(Vertex v)
    {
        return graph.OutEdges(v);
    }

    public static IEnumerable<Edge> AStar(Vertex start, Vertex end)
    {
        Assert.IsNotNull(start);
        Assert.IsNotNull(end);
        TryFunc<Vertex, IEnumerable<Edge>> tryFunc = GetAStarTryFunc(start);
        tryFunc(end, out IEnumerable<Edge> edges);
        return edges;
    }

    public static TryFunc<Vertex, IEnumerable<Edge>> GetAStarTryFunc(Vertex start)
    {
        Assert.IsNotNull(start);
        return graph.ShortestPathsAStar(
            (Edge p) => p.Length,
            (Vertex to) => math.distance(start.Pos, to.Pos),
            start
        );
    }

    public static IEnumerable<Edge> InEdges(Vertex vertex)
    {
        return graph.InEdges(vertex);
    }

    public static int InDegree(Vertex vertex)
    {
        return graph.InDegree(vertex);
    }

    public static Edge GetEdge(Vertex start, Vertex end)
    {
        graph.TryGetEdge(start, end, out Edge edge);
        return edge;
    }
}