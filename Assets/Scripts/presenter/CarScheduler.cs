using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.model.Roads;

public static class CarScheduler
{
    public static event Action ConnectionUpdated;

    static CarScheduler()
    {
        Game.RoadRemoved += DeleteMissingConnection;
        Build.RoadsBuilt += FindNewConnection;
    }

    public static void Schedule(float deltaTime)
    {
        foreach (SourceZone source in Game.SourceZones.Values)
        {
            if (source.ScheduleCooldown <= 0)
            {
                if (source.ConnectedTargets.Count != 0)
                {
                    Game.RegisterCar(new(
                        new(source.ConnectedTargets.Values.ElementAt(MyNumerics.GetRandomIndex(source.ConnectedTargets.Count)))
                    ));
                    source.ScheduleCooldown = SourceZone.ScheduleInterval;
                }
            }
            else
                source.ScheduleCooldown -= deltaTime;
        }
    }

    public static void FindNewConnection()
    {
        bool changed = false;
        foreach (SourceZone source in Game.SourceZones.Values)
            foreach (TargetZone target in Game.TargetZones.Values)
                if (!source.ConnectedTargets.ContainsKey(target))
                {
                    IEnumerable<Edge> pathEdges = FindPathSourceToTarget(source, target);
                    if (pathEdges != null)
                    {
                        changed = true;
                        source.ConnectedTargets.Add(target, pathEdges);
                        target.ConnectedSources.Add(source);
                    }
                }
        if (changed)
        {
            if (ConnectionUpdated == null)
                Progression.CheckProgression();
            ConnectionUpdated.Invoke();
        }
    }

    static void DeleteMissingConnection(Road road)
    {
        if (road.IsGhost)
            return;
        bool changed = false;
        foreach (SourceZone source in Game.SourceZones.Values)
            foreach (TargetZone target in Game.TargetZones.Values)
                if (source.ConnectedTargets.ContainsKey(target))
                {
                    IEnumerable<Edge> pathEdges = FindPathSourceToTarget(source, target);
                    if (pathEdges == null)
                    {
                        changed = true;
                        source.ConnectedTargets.Remove(target);
                        target.ConnectedSources.Remove(source);
                    }
                }
        if (changed)
        {
            if (ConnectionUpdated == null)
                Progression.CheckProgression();
            ConnectionUpdated.Invoke();
        }
    }

    static IEnumerable<Edge> FindPathSourceToTarget(SourceZone source, TargetZone target)
    {
        foreach (Vertex sourceVertex in source.Vertices)
        {
            foreach (Vertex targetVertex in target.Vertices)
            {
                IEnumerable<Edge> edges = Graph.ShortestPathAStar(sourceVertex, targetVertex);
                if (edges != null)
                    return edges;
            }
        }
        return null;
    }

    public static Car AttemptSchedule(Zone source, Zone target)
    {
        if (source.Vertices.Count == 0 || target.Vertices.Count == 0)
            return null;
        Vertex startV = source.Vertices.ElementAt(MyNumerics.GetRandomIndex(source.Vertices.Count));
        Vertex endV = target.Vertices.ElementAt(MyNumerics.GetRandomIndex(target.Vertices.Count));
        return AttemptSchedule(startV, endV);
    }

    public static Car AttemptSchedule(Vertex origin, Vertex dest)
    {
        IEnumerable<Edge> edges = Graph.ShortestPathAStar(origin, dest);
        Car car = new(new(edges));
        return car;
    }

    public static Car AttemptSchedule(Road origin, Road dest)
    {
        Vertex start = origin.Lanes[MyNumerics.GetRandomIndex(origin.LaneCount)].StartVertex;
        Vertex end = dest.Lanes[MyNumerics.GetRandomIndex(dest.LaneCount)].EndVertex;
        return AttemptSchedule(start, end);
    }
}