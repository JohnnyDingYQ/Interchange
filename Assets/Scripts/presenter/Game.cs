using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;
using Assets.Scripts.Model.Roads;

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
    public static Dictionary<uint, Zone> Zones { get => GameSave.Zones; }
    public static Dictionary<uint, District> Districts { get => GameSave.Districts; }
    public static uint CarServiced { get => GameSave.CarServiced; set => GameSave.CarServiced = value; }
    public static Road HoveredRoad { get; set; }
    public static Zone HoveredZone { get; set; }
    public static District HoveredDistrict { get; set; }
    public static bool BuildModeOn { get; set; }
    private static readonly HashSet<Road> selectedRoads = new();
    public static ReadOnlySet<Road> SelectedRoads { get => selectedRoads.AsReadOnly(); }
    public static event Action<Road> RoadAdded, RoadUpdated, RoadRemoved, RoadSelected, RoadUnselected;
    public static event Action<Intersection> IntersectionAdded, IntersectionUpdated, IntersectionRemoved;
    public static event Action<Car> CarAdded, CarRemoved;
    public static System.Random Random = new();

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
        HoveredZone = null;
        HoveredDistrict = null;
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
            {
                Graph.AddEdge(lane.InnerEdge);
            }
            RegisterLane(lane);
        }
        RegisterIntersection(road.StartIntersection);
        RegisterIntersection(road.EndIntersection);
        RegisterCurve(road.Curve);
        RoadAdded?.Invoke(road);
    }

    public static void RegisterCurve(Curve curve)
    {
        if (curve.Id != 0)
        {
            if (curve.GetNextCurve() != null)
                RegisterCurve(curve.GetNextCurve());
            return;
        }
        curve.Id = FindNextAvailableKey(Curves.Keys);
        Curves[curve.Id] = curve;
        if (curve.GetNextCurve() != null)
            RegisterCurve(curve.GetNextCurve());
    }

    public static void RemoveCurve(Curve curve)
    {
        bool success = Curves.Remove(curve.Id);
        if (curve.Id != 0)
            Assert.IsTrue(success);
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
        IntersectionRemoved?.Invoke(i);
        i.Id = 0;
    }

    public static void UpdateIntersection(Intersection ix)
    {
        UpdateSafetyAhead(ix, 0, Constants.MaxLaneCount - 1);

        foreach (Road r in ix.Roads)
            RoadUpdated?.Invoke(r);
        IntersectionUpdated?.Invoke(ix);

        static void UpdateSafetyAhead(Intersection ix, int depth, int maxDepth)
        {
            ix.DetermineSafety();
            IntersectionUpdated?.Invoke(ix);
            if (depth == maxDepth)
                return;
            HashSet<Intersection> nextDepth = new();
            foreach (Road outRoad in ix.OutRoads)
            {
                foreach (Lane lane in outRoad.Lanes)
                    nextDepth.Add(lane.EndNode.Intersection);
            }
            foreach (Intersection intersection in nextDepth)
                UpdateSafetyAhead(intersection, depth + 1, maxDepth);

        }
    }

    public static void RegisterLane(Lane lane)
    {
        if (lane.Id != 0)
            return;
        lane.Id = FindNextAvailableKey(Lanes.Keys);
        Lanes[lane.Id] = lane;
    }

    public static void RemoveLane(Lane lane)
    {
        Assert.IsTrue(Lanes.Keys.Contains(lane.Id));
        Lanes.Remove(lane.Id);
        lane.Id = 0;
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
        if (road.IsMajorRoad && option == RoadRemovalOption.Default && !road.IsGhost)
            return false;
        return Remove.RemoveRoad(road, option);
    }

    public static bool RemoveRoad(Road road)
    {
        return RemoveRoad(road, RoadRemovalOption.Default);
    }

    public static void BulkRemoveSelected()
    {
        foreach (Road road in SelectedRoads)
            if (!road.IsMajorRoad)
                RemoveRoad(road);

        foreach (Road road in SelectedRoads)
            if (Roads.ContainsKey(road.Id))
                RemoveRoad(road);
    }

    public static void SelectRoad(Road road)
    {
        selectedRoads.Add(road);
        RoadSelected?.Invoke(road);
    }

    public static void UnselectRoad(Road road)
    {
        selectedRoads.Remove(road);
        RoadUnselected?.Invoke(road);
    }

    public static void RegisterCar(Car car)
    {
        car.Id = FindNextAvailableKey(Cars.Keys);
        Cars[car.Id] = car;
        CarAdded?.Invoke(car);
    }

    public static void RemoveCar(Car car)
    {
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

    public static void InvokeCarAdded(Car car)
    {
        CarAdded?.Invoke(car);
    }

    static void SanityCheck()
    {
        Assert.IsTrue(Constants.MinElevation < Constants.MaxElevation);
        Assert.AreEqual(0, (Constants.MaxElevation - Constants.MinElevation) % Constants.ElevationStep);
        Assert.AreEqual(0, Constants.MinElevation);
    }

    public static void SetupZones()
    {
        foreach (Zone zone in Zones.Values)
            zone.InitConnectedTargets(Zones.Values);
    }
}