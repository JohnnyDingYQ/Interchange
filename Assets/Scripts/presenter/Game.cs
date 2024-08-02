using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;
using Assets.Scripts.model.Roads;

public static class Game
{
    public static GameSave GameSave { get; set; }
    public static Dictionary<uint, Road> Roads { get => GameSave.Roads; }
    public static Dictionary<uint, Node> Nodes { get => GameSave.Nodes; }
    public static Dictionary<uint, Intersection> Intersections { get => GameSave.Intersections; }
    public static Dictionary<uint, Lane> Lanes { get => GameSave.Lanes; }
    public static Dictionary<uint, Vertex> Vertices { get => GameSave.Vertices; }
    public static Dictionary<uint, Car> Cars { get => GameSave.Cars; }
    public static Dictionary<uint, Edge> Edges { get => GameSave.Edges; }
    public static Dictionary<uint, Curve> Curves { get => GameSave.Curves; }
    public static Dictionary<uint, SourceZone> SourceZones { get => GameSave.SourceZones; }
    public static Dictionary<uint, TargetZone> TargetZones { get => GameSave.TargetZones; }
    public static Road HoveredRoad { get; set; }
    public static Zone HoveredZone { get; set; }
    public static uint CarServiced { get; set; }
    public static bool BuildModeOn { get; set; }
    public static event Action<Road> RoadAdded, RoadUpdated, RoadRemoved;
    public static event Action<Intersection> IntersectionAdded, IntersectionRemoved;
    public static event Action<Car> CarAdded, CarRemoved;

    static Game()
    {
        SanityCheck();
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
                Graph.AddEdge(lane.InnerEdge);
            RegisterLane(lane);
            RegisterCurve(lane.Curve);
        }
        RegisterIntersection(road.StartIntersection);
        RegisterIntersection(road.EndIntersection);
        RegisterCurve(road.Curve);
        RoadAdded?.Invoke(road);
    }

    public static void RegisterCurve(Curve curve)
    {
        if (curve.Id != 0)
            return;
        curve.Id = FindNextAvailableKey(Curves.Keys);
        Curves[curve.Id] = curve;
        if (curve.GetNextCurve() != null)
            RegisterCurve(curve.GetNextCurve());
    }

    public static void RemoveCurve(Curve curve)
    {
        Curves.Remove(curve.Id);
        curve.Id = 0;
        if (curve.GetNextCurve() != null)
            RemoveCurve(curve.GetNextCurve());
    }

    public static void RegisterIntersection(Intersection i)
    {
        if (Intersections.Values.Contains(i))
            return;
        i.Id = FindNextAvailableKey(Intersections.Keys);
        Intersections[i.Id] = i;
        IntersectionAdded?.Invoke(i);
    }

    public static void RemoveIntersection(Intersection i)
    {
        Assert.IsTrue(Intersections.Keys.Contains(i.Id));
        Intersections.Remove(i.Id);
        i.Id = 0;
        IntersectionRemoved?.Invoke(i);
    }

    public static void UpdateIntersectionRoads(Intersection ix)
    {
        foreach (Road r in ix.Roads)
            RoadUpdated?.Invoke(r);
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
        lane.Id = 0;
        RemoveCurve(lane.Curve);
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
        node.Id = 0;
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

    public static void InvokeRoadAdded(Road road)
    {
        RoadAdded?.Invoke(road);
    }

    public static void InvokeRoadRemoved(Road road)
    {
        RoadRemoved?.Invoke(road);
    }

    public static void InvokeIntersectionAdded(Intersection ix)
    {
        IntersectionAdded?.Invoke(ix);
    }

    static void SanityCheck()
    {
        Assert.IsTrue(Constants.MinLaneLength * 2 < Constants.MaxLaneLength);
        Assert.IsTrue(Constants.MinElevation < Constants.MaxElevation);
        Assert.AreEqual(0, (Constants.MaxElevation - Constants.MinElevation) % Constants.ElevationStep);
        Assert.AreEqual(0, Constants.MinElevation);
    }
}