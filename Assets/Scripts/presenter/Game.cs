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
    public static event Action<uint> CarServicedUpdated;
    public static GameSave GameSave { get; set; }
    public static Dictionary<uint, Road> Roads { get => GameSave.Roads;}
    public static Dictionary<uint, Node> Nodes { get => GameSave.Nodes; }
    public static Dictionary<uint, Intersection> Intersections { get => GameSave.Intersections;}
    public static Dictionary<uint, Lane> Lanes { get => GameSave.Lanes;}
    public static Dictionary<uint, Vertex> Vertices { get => GameSave.Vertices;}
    public static Dictionary<uint, Path> Paths { get => GameSave.Paths;}
    public static Dictionary<uint, Zone> Zones { get => GameSave.Zones;}
    public static Dictionary<uint, Car> Cars { get => GameSave.Cars;}
    public static AdjacencyGraph<Vertex, Path> Graph { get => GameSave.Graph;}
    public static float Elevation { get => GameSave.Elevation;}
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
            RegisterPath(lane.InnerPath);
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

    public static void RegisterVertex(Vertex v)
    {
        Assert.IsFalse(Graph.ContainsVertex(v) ^ Vertices.ContainsValue(v));
        if (Graph.ContainsVertex(v))
            return;
        v.Id = FindNextAvailableKey(Vertices.Keys);
        Vertices[v.Id] = v;
        Graph.AddVertex(v);
    }

    public static void RemoveVertex(Vertex v)
    {
        Assert.IsTrue(Graph.ContainsVertex(v));
        Assert.IsTrue(Vertices.ContainsValue(v));
        Vertices.Remove(v.Id);
        Graph.RemoveVertex(v);
    }

    public static void RegisterPath(Path p)
    {
        // Debug.Log($"start {p.Source.Id} end {p.Target.Id}");
        Assert.IsFalse(Graph.ContainsEdge(p) ^ Paths.ContainsValue(p));
        Graph.TryGetEdge(p.Source, p.Target, out Path edge);
        if (edge != null)
            return;
        p.Id = FindNextAvailableKey(Paths.Keys);
        Paths[p.Id] = p;
        Assert.IsTrue(Graph.AddEdge(p));
    }

    public static void RemovePath(Path p)
    {
        Assert.IsTrue(Paths.ContainsKey(p.Id));
        Assert.IsTrue(Graph.ContainsEdge(p));
        Assert.IsTrue(Paths.Remove(p.Id));
        Graph.RemoveEdge(p);
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