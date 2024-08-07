using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.model.Roads;
using QuikGraph;

public static class CarScheduler
{
    public static event Action ConnectionUpdated;

    static CarScheduler()
    {
        Game.RoadRemoved += DeleteMissingConnection;
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
        {
            IEnumerable<TargetZone> targetWithoutPath = Game.TargetZones.Values.Where(z => !source.ConnectedTargets.ContainsKey(z));
            if (targetWithoutPath.Count() == 0)
                continue;
            foreach (Vertex startV in source.Vertices)
            {
                TryFunc<Vertex, IEnumerable<Edge>> tryFunc = Graph.GetAStarTryFunc(startV);
                foreach (TargetZone target in targetWithoutPath)
                    foreach (Vertex endV in target.Vertices)
                    {
                        tryFunc(endV, out IEnumerable<Edge> pathEdges);
                        if (pathEdges != null)
                        {
                            changed = true;
                            source.ConnectedTargets.Add(target, pathEdges);
                            target.ConnectedSources.Add(source);
                            break;
                        }
                    }
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
        foreach (SourceZone source in Game.SourceZones.Values.Where(z => z.ConnectedTargets.Count != 0))
        {
            HashSet<TargetZone> unfoundTargets = source.ConnectedTargets.Keys.ToHashSet();
            foreach (Vertex startV in source.Vertices)
            {
                TryFunc<Vertex, IEnumerable<Edge>> tryFunc = Graph.GetAStarTryFunc(startV);
                foreach (TargetZone target in source.ConnectedTargets.Keys)
                    foreach (Vertex endV in target.Vertices)
                    {
                        tryFunc(endV, out IEnumerable<Edge> pathEdges);
                        if (pathEdges != null)
                            unfoundTargets.Remove(target);

                    }
            }
            foreach (TargetZone unfound in unfoundTargets)
            {
                changed = true;
                source.ConnectedTargets.Remove(unfound);
                unfound.ConnectedSources.Remove(source);
            }
        }

        if (changed)
        {
            if (ConnectionUpdated == null)
                Progression.CheckProgression();
            ConnectionUpdated.Invoke();
        }
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
        IEnumerable<Edge> edges = Graph.AStar(origin, dest);
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