using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public static class Game
{
    public static event Action<Road> RoadAdded;
    public static event Action<Road> RoadUpdated;
    public static event Action<Road> RoadRemoved;
    public static event Action<Car> CarAdded;
    public static event Action<Car> CarRemoved;
    public static GameSave GameSave { get; set; }
    public static Dictionary<uint, Road> Roads { get => GameSave.Roads; }
    public static Dictionary<uint, Node> Nodes { get => GameSave.Nodes; }
    public static Dictionary<uint, Intersection> Intersections { get => GameSave.Intersections; }
    public static Dictionary<uint, Lane> Lanes { get => GameSave.Lanes; }
    public static Dictionary<uint, Vertex> Vertices { get => GameSave.Vertices; }
    public static Dictionary<uint, Path> Paths { get => GameSave.Paths; }
    public static Dictionary<uint, Car> Cars { get => GameSave.Cars; }
    public static Dictionary<uint, Point> Targets { get => GameSave.Targets; }
    public static Dictionary<uint, SourcePoint> Sources { get => GameSave.Sources; }
    public static Road HoveredRoad { get; set; }
    public static uint CarServiced { get; set; }
    public static bool BuildModeOn { get; set; }

    static Game()
    {
        GameSave = new();
        BuildModeOn = true;
    }

    public static void WipeState()
    {
        Build.Reset();
        Graph.Wipe();
        GameSave = new();
        BuildModeOn = true;
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
        road.Id = FindNextAvailableKey(Roads.Keys);
        Roads.Add(road.Id, road);
        foreach (Lane lane in road.Lanes)
        {
            RegisterVertex(lane.StartVertex);
            RegisterVertex(lane.EndVertex);
            if (!road.IsGhost)
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
        Assert.IsFalse(Graph.ContainsPath(p) ^ Paths.ContainsValue(p));
        if (Graph.ContainsPath(p.Source, p.Target))
            return;
        p.Id = FindNextAvailableKey(Paths.Keys);
        Paths[p.Id] = p;
        Graph.AddPath(p);
    }

    public static void RemovePath(Path p)
    {
        Assert.IsTrue(Paths.ContainsKey(p.Id));
        // Assert.IsTrue(Graph.ContainsEdge(p));
        Paths.Remove(p.Id);
        Graph.RemovePath(p);
    }

    public static void RegisterNode(Node node)
    {
        if (Nodes.ContainsKey(node.Id))
            return;
        Assert.IsNotNull(node.Intersection);
        node.Id = FindNextAvailableKey(Nodes.Keys);
        Nodes[node.Id] = node;
    }

    public static bool HasNode(Node node)
    {
        return Nodes.Values.Contains(node);
    }

    public static bool RemoveRoad(Road road, RoadRemovalOption option)
    {
        if (!Roads.ContainsKey(road.Id))
            return false;
        return Build.RemoveRoad(road, option);
    }
    public static bool RemoveRoad(Road road)
    {
        return RemoveRoad(road, RoadRemovalOption.Default);
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
}