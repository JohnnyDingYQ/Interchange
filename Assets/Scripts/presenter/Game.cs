using System;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.Assertions;

public static class Game
{
    public static event Action<Road> RoadAdded;
    public static event Action<Road> RoadUpdated;
    public static event Action<Road> RoadRemoved;
    public static event Action UpdateHoveredZone;
    public static GameSave GameSave { get; set; }
    public static SortedDictionary<int, Road> Roads { get { return GameSave.Roads; } }
    public static SortedDictionary<int, Node> Nodes { get { return GameSave.Nodes; } }
    public static SortedDictionary<int, Zone> Zones { get { return GameSave.Zones; } }
    public static AdjacencyGraph<Vertex, Path> Graph { get { return GameSave.Graph; } }
    public static int Elevation { get { return GameSave.Elevation; } }
    public static Road HoveredRoad { get; set; }
    private static Zone hoveredZone;
    public static Zone HoveredZone
    {
        get
        {
            UpdateHoveredZone?.Invoke();
            return hoveredZone;
        }
        set
        {
            hoveredZone = value;
        }
    }
    public static float3 MouseWorldPos { get { return InputSystem.MouseWorldPos; } }

    public static int NextAvailableNodeId
    {
        get { return GameSave.NextAvailableNodeId; }
        set { GameSave.NextAvailableNodeId = value; }
    }
    public static int NextAvailableRoadId
    {
        get { return GameSave.NextAvailableRoadId; }
        set { GameSave.NextAvailableRoadId = value; }
    }

    static Game()
    {
        GameSave = new();
        Assert.IsTrue(Constants.CarMinimumSeparation < Constants.MinimumLaneLength);
    }

    public static void WipeState()
    {
        Build.Reset();
        GameSave = new();
        hoveredZone = null;
        HoveredRoad = null;
    }

    public static void RegisterRoad(Road road)
    {
        if (road.Id == Constants.GhostRoadId)
        {
            if (Roads.ContainsKey(road.Id))
                RemoveRoad(Roads[road.Id]);
        }
        else
            road.Id = NextAvailableRoadId++;
        Roads.Add(road.Id, road);
        foreach (Lane lane in road.Lanes)
        {
            AddVertex(lane.StartVertex);
            AddVertex(lane.EndVertex);
            AddEdge(lane.InnerPath);
        }
        RoadAdded?.Invoke(road);
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

    public static bool RemoveRoad(int id)
    {
        if (!Roads.ContainsKey(id))
            return false;
        return RemoveRoad(Roads[id], false);
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
        {
            IntersectionUtil.EvaluatePaths(road.StartIntersection);
            IntersectionUtil.EvaluateOutline(road.StartIntersection);
        }

        if (!road.EndIntersection.IsEmpty())
        {
            IntersectionUtil.EvaluatePaths(road.EndIntersection);
            IntersectionUtil.EvaluateOutline(road.EndIntersection);
        }

        RoadRemoved?.Invoke(road);
        return true;
    }

    public static bool RemoveRoad(Road road)
    {
        return RemoveRoad(road, false);
    }

    public static void InvokeRoadUpdated(Road road)
    {
        RoadUpdated?.Invoke(road);
    }
    public static void InvokeRoadAdded(Road road)
    {
        RoadAdded?.Invoke(road);
    }

    public static void AddVertex(Vertex vertex)
    {
        if (!Graph.Vertices.Contains(vertex))
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
        if (HoveredRoad != null)
            DivideHandler.HandleDivideCommand(HoveredRoad, mouseWorldPos);
    }

    public static void RemoveSelectedRoad()
    {
        if (HoveredRoad != null)
            RemoveRoad(HoveredRoad);
    }

    public static void SetElevation(int elevation)
    {
        if (elevation < 0)
            GameSave.Elevation = 0;
        else if (elevation > Constants.MaxElevation)
            GameSave.Elevation = Constants.MaxElevation;
        else
            GameSave.Elevation = elevation;
        DevPanel.Elevation.text = "Elevation: " + Elevation;
    }
}