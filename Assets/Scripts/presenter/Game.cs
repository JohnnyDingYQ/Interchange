using System;
using System.Collections.Generic;
using UnityEngine;
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

    public static uint FindNextAvailableKey(ICollection<uint> dict)
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
            Graph.AddVertex(lane.StartVertex);
            Graph.AddVertex(lane.EndVertex);
            if (!road.IsGhost)
                Graph.AddPath(lane.InnerPath);
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
        Assert.IsNotNull(node.Intersection);
        node.Id = FindNextAvailableKey(Nodes.Keys);
        Nodes[node.Id] = node;
    }

    public static void RemoveNode(Node node)
    {
        Nodes.Remove(node.Id);
        node.Intersection.RemoveNode(node);
    }

    public static bool RemoveRoad(Road road, RoadRemovalOption option)
    {
        if (!Roads.ContainsKey(road.Id))
            return false;
        return Remove.RemoveRoad(road, option);
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