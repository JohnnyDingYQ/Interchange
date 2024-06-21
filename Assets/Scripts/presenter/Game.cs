using System;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.Assertions;
using System.Collections.ObjectModel;

public static class Game
{
    public static event Action<Road> RoadAdded;
    public static event Action<Road> RoadUpdated;
    public static event Action<Road> RoadRemoved;
    public static event Action<Car> CarAdded;
    public static event Action<Car> CarRemoved;
    public static event Action UpdateHoveredZone;
    public static event Action<float> ElevationUpdated;
    public static event Action<uint> CarServicedUpdated;
    public static GameSave GameSave { get; set; }
    public static Dictionary<uint, Road> Roads { get { return GameSave.Roads; } }
    public static Dictionary<uint, Node> Nodes { get { return GameSave.Nodes; } }
    public static Dictionary<uint, Intersection> Intersections { get { return GameSave.Intersections; } }
    public static Dictionary<uint, Lane> Lanes { get { return GameSave.Lanes; } }
    public static Dictionary<uint, Zone> Zones { get { return GameSave.Zones; } }
    public static Dictionary<uint, Car> Cars { get { return GameSave.Cars; } }
    public static AdjacencyGraph<Vertex, Path> Graph { get { return GameSave.Graph; } }
    public static float Elevation { get { return GameSave.Elevation; } }
    public static uint CarServiced {
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

    private static uint FindNextAvailableKey(ICollection<uint> dict)
    {
        uint i = 1;
        while (dict.Contains(i))
            i++;
        return i;
    }

    public static void RegisterRoad(Road road)
    {
        if (road.Id == Constants.GhostRoadId)
        {
            if (Roads.ContainsKey(road.Id))
                RemoveRoad(Roads[road.Id]);
        }
        else
            road.Id = FindNextAvailableKey(Roads.Keys);
        Roads.Add(road.Id, road);
        foreach (Lane lane in road.Lanes)
        {
            RegisterVertex(lane.StartVertex);
            RegisterVertex(lane.EndVertex);
            RegisterEdge(lane.InnerPath);
            RegisterLane(lane);
        }
        RegisterIntersection(road.StartIntersection);
        RegisterIntersection(road.EndIntersection);
        RoadAdded?.Invoke(road);
    }

    public static void RegisterIntersection(Intersection i)
    {
        if (!Intersections.Values.Contains(i))
        {
            i.Id = FindNextAvailableKey(Intersections.Keys);
            Intersections[i.Id] = i;
        }
    }

    public static void RemoveIntersection(Intersection i)
    {
        Assert.IsTrue(Intersections.Keys.Contains(i.Id));
        Intersections.Remove(i.Id);
    }

    public static void RegisterLane(Lane lane)
    {
        if (!Lanes.Values.Contains(lane))
        {
            lane.Id = FindNextAvailableKey(Lanes.Keys);
            Lanes[lane.Id] = lane;
        }
    }

    public static void RemoveLane(Lane lane)
    {
        Assert.IsTrue(Lanes.Keys.Contains(lane.Id));
        Lanes.Remove(lane.Id);
    }

    public static void RegisterNode(Node node)
    {
        if (Nodes.ContainsKey(node.Id))
            return;
        node.Id = FindNextAvailableKey(Nodes.Keys);
        Nodes[node.Id] = node;
    }

    public static bool HasNode(Node node)
    {
        return Nodes.Values.Contains(node);
    }

    public static bool RemoveRoad(uint id)
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
        car.Id = FindNextAvailableKey(Cars.Keys);
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

    public static void RegisterVertex(Vertex vertex)
    {
        if (!Graph.Vertices.Contains(vertex))
            Graph.AddVertex(vertex);
    }

    public static void RegisterEdge(Path path)
    {
        Assert.IsFalse(Graph.ContainsEdge(path));
        path.Id = FindNextAvailableKey(Graph.Edges.Select(path => path.Id).ToList());
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