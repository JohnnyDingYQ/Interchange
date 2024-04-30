using System;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using Unity.Mathematics;
using System.Linq;

public static class Game
{
    public static event Action<Road> InstantiateRoad;
    public static event Action<Road> UpdateRoadMesh;
    public static event Action<Road> DestroyRoad;
    public static GameState GameState { get; set; }
    public static SortedDictionary<int, Road> Roads { get { return GameState.Roads; } }
    public static SortedDictionary<int, Node> Nodes { get { return GameState.Nodes; } }
    public static AdjacencyGraph<Vertex, Path> Graph { get { return GameState.Graph; } }
    public static Road SelectedRoad { get; set; }

    public static int NextAvailableNodeId
    {
        get { return GameState.NextAvailableNodeId; }
        set { GameState.NextAvailableNodeId = value; }
    }
    public static int NextAvailableRoadId
    {
        get { return GameState.NextAvailableRoadId; }
        set { GameState.NextAvailableRoadId = value; }
    }

    static Game()
    {
        GameState = new();
    }

    public static void WipeState()
    {
        Build.Reset();
        GameState = new();
    }

    public static void RegisterRoad(Road road)
    {
        if (Roads.ContainsKey(road.Id))
            return;
        if (road.IsGhost)
            road.Id = -2;
        else
            road.Id = NextAvailableRoadId++;
        Roads.Add(road.Id, road);
        foreach (Lane lane in road.Lanes)
        {
            AddVertex(lane.StartVertex);
            AddVertex(lane.EndVertex);
            AddEdge(lane.InnerPath);
        }
        InstantiateRoad?.Invoke(road);
    }

    public static void RegisterNode(Node node)
    {
        if (Nodes.ContainsKey(node.Id))
            return;
        node.Id = NextAvailableNodeId++;
        Nodes[node.Id] = node;
    }

    public static bool HasNode(Node node)
    {
        return Nodes.Values.Contains(node);
    }

    public static bool RemoveRoad(Road road, bool retainVertices)
    {
        if (!Roads.ContainsKey(road.Id))
            return false;
        Roads.Remove(road.Id);
        foreach (Lane lane in road.Lanes)
        {
            Graph.RemoveEdgeIf(e => e.Source == lane.StartVertex && e.Target == lane.EndVertex);
            if (!retainVertices)
            {
                Graph.RemoveVertex(lane.StartVertex);
                Graph.RemoveVertex(lane.EndVertex);
            }
            foreach (Node node in new List<Node>() { lane.StartNode, lane.EndNode })
            {
                node.RemoveLane(lane);
                if (node.Lanes.Count == 0)
                {
                    Nodes.Remove(node.Id);
                    road.StartIntersection.RemoveNode(node);
                    road.EndIntersection.RemoveNode(node);
                }
            }
        }
        road.StartIntersection.RemoveRoad(road, Side.Start);
        road.EndIntersection.RemoveRoad(road, Side.End);
        if (!road.StartIntersection.IsEmpty())
            road.StartIntersection.ReevaluatePaths();

        if (!road.EndIntersection.IsEmpty())
            road.EndIntersection.ReevaluatePaths();

        DestroyRoad?.Invoke(road);
        return true;
    }

    public static bool RemoveRoad(Road road)
    {
        return RemoveRoad(road, false);
    }

    public static void InvokeUpdateRoadMesh(Road road)
    {
        UpdateRoadMesh?.Invoke(road);
    }
    public static void InvokeInstantiateRoad(Road road)
    {
        InstantiateRoad?.Invoke(road);
    }

    public static void AddVertex(Vertex vertex)
    {
        Graph.AddVertex(vertex);
    }

    public static void AddEdge(Path path)
    {
        Graph.AddEdge(path);
    }

    public static bool HasEdge(Lane from, Lane to)
    {
        return HasEdge(from.EndVertex, to.StartVertex) || HasEdge(to.EndVertex, from.StartVertex);
    }

    public static bool HasEdge(Vertex from, Vertex to)
    {
        if (Graph.ContainsEdge(from, to))
            return true;
        return false;
    }

    public static void DivideSelectedRoad(float3 mouseWorldPos)
    {
        if (SelectedRoad != null)
            DivideHandler.HandleDivideCommand(SelectedRoad, mouseWorldPos);
    }

    public static void RemoveSelectedRoad()
    {
        if (SelectedRoad != null)
            RemoveRoad(SelectedRoad);
    }
}