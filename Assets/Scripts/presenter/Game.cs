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
    public static event Action<Car> CarAdded;
    public static event Action<Car> CarRemoved;
    public static event Action UpdateHoveredZone;
    public static event Action<float> ElevationUpdated;
    public static event Action<ulong> CarServicedUpdated;
    public static GameSave GameSave { get; set; }
    public static SortedDictionary<ulong, Road> Roads { get { return GameSave.Roads; } }
    public static SortedDictionary<ulong, Node> Nodes { get { return GameSave.Nodes; } }
    public static SortedDictionary<ulong, Zone> Zones { get { return GameSave.Zones; } }
    public static SortedDictionary<ulong, Car> Cars { get { return GameSave.Cars; } }
    public static AdjacencyGraph<Vertex, Path> Graph { get { return GameSave.Graph; } }
    public static float Elevation { get { return GameSave.Elevation; } }
    public static ulong CarServiced {
        get { return GameSave.CarServiced; }
        set { GameSave.CarServiced = value; CarServicedUpdated?.Invoke(value); } 
    }
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

    public static ulong NextAvailableNodeId
    {
        get { return GameSave.NextAvailableNodeId; }
        set { GameSave.NextAvailableNodeId = value; }
    }
    public static ulong NextAvailableRoadId
    {
        get { return GameSave.NextAvailableRoadId; }
        set { GameSave.NextAvailableRoadId = value; }
    }
    public static ulong NextAvailablePathId
    {
        get { return GameSave.NextAvailablePathId; }
        set { GameSave.NextAvailablePathId = value; }
    }
    public static ulong NextAvailableCarId
    {
        get { return GameSave.NextAvailableCarId; }
        set { GameSave.NextAvailableCarId = value; }
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

    public static bool RemoveRoad(ulong id)
    {
        if (!Roads.ContainsKey(id))
            return false;
        return RemoveRoad(Roads[id], false);
    }

    public static bool RemoveRoad(Road road, bool retainVertices)
    {
        if (!Roads.ContainsKey(road.Id))
            return false;
        return Build.RemoveRoad(road, retainVertices);
    }

    public static void RegisterCar(Car car)
    {
        Assert.IsFalse(Cars.ContainsValue(car));
        car.Id = NextAvailableCarId++;
        Cars[car.Id] = car;
        CarAdded?.Invoke(car);
    }

    public static void RemoveCar(Car car)
    {
        Assert.IsTrue(Cars.ContainsValue(car));
        Cars.Remove(car.Id);
        CarRemoved?.Invoke(car);
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

    public static void InvokeRoadRemoved(Road road)
    {
        RoadRemoved?.Invoke(road);
    }

    public static void AddVertex(Vertex vertex)
    {
        if (!Graph.Vertices.Contains(vertex))
            Graph.AddVertex(vertex);
    }

    public static void AddEdge(Path path)
    {
        path.Id = NextAvailablePathId++;
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

    public static void SetElevation(float elevation)
    {
        if (elevation < Constants.MinElevation)
            GameSave.Elevation = Constants.MinElevation;
        else if (elevation > Constants.MaxElevation)
            GameSave.Elevation = Constants.MaxElevation;
        else
            GameSave.Elevation = elevation;
        ElevationUpdated?.Invoke(elevation);
    }
}