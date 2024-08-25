using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Model.Roads;
using QuikGraph;
using UnityEngine.Assertions;

public static class CarScheduler
{
    static CarScheduler()
    {
        Game.RoadRemoved += DeleteMissingConnection;
    }

    public static void Schedule(float deltaTime)
    {
        foreach (Zone source in Game.Zones.Values)
        {
            IEnumerable<Zone> randomOrder = source.ConnectedZones.OrderBy(x => Game.Random.Next());
            foreach (Zone target in randomOrder)
            {
                source.TryGetPathTo(target, out Path path);
                Vertex startVertex = path.StartVertex;
                Assert.IsTrue(source.Vertices.Contains(startVertex));
                if (startVertex.ScheduleCooldown <= 0)
                {
                    Game.RegisterCar(new(source, target));
                    startVertex.ScheduleCooldown = GenerateVertexInterval() + deltaTime;
                }
            }
            foreach (Vertex vertex in source.Vertices)
                vertex.ScheduleCooldown -= deltaTime;
        }
    }

    static float GenerateVertexInterval()
    {
        return Vertex.ScheduleInterval * (1 + 0.4f * Game.Random.Next() / int.MaxValue);
    }


    public static void FindNewConnection()
    {
        bool changed = false;
        foreach (Zone source in Game.Zones.Values)
        {
            foreach (Vertex startV in source.Vertices)
            {
                TryFunc<Vertex, IEnumerable<Edge>> tryFunc = Graph.GetAStarTryFunc(startV);
                foreach (Zone target in Game.Zones.Values)
                {
                    if (target == source)
                        continue;
                    foreach (Vertex endV in target.Vertices.OrderBy(v => v.Id)) // order for testing purposes
                    {
                        if (tryFunc(endV, out IEnumerable<Edge> pathEdges))
                        {
                            changed = true;
                            AddPath(source, target, pathEdges);
                        }
                    }
                }
            }
        }
        if (changed)
            Progression.CheckProgression();

        static void AddPath(Zone source, Zone target, IEnumerable<Edge> pathEdges)
        {
            Path newPath = new(pathEdges);
            if (source.TryGetPathTo(target, out Path existingPath))
            {
                if (existingPath.Cost > newPath.Cost)
                {
                    Path shorterPath = newPath;
                    source.SetPathTo(target, shorterPath);
                }
            }
            else
                source.SetPathTo(target, newPath);
        }
    }

    public static void DeleteMissingConnection(Road road)
    {
        if (!road.IsGhost)
            DeleteMissingConnection();
    }


    public static void DeleteMissingConnection()
    {
        bool changed = false;
        foreach (Zone source in Game.Zones.Values.Where(z => z.ConnectedZones.Count() != 0))
        {
            HashSet<Zone> unfoundTargets = source.ConnectedZones.ToHashSet();
            foreach (Vertex startV in source.Vertices)
            {
                TryFunc<Vertex, IEnumerable<Edge>> tryFunc = Graph.GetAStarTryFunc(startV);
                List<Zone> targets = new(source.ConnectedZones);
                foreach (Zone target in targets)
                    foreach (Vertex endV in target.Vertices)
                    {
                        if (tryFunc(endV, out IEnumerable<Edge> pathEdges))
                        {
                            unfoundTargets.Remove(target);
                            source.SetPathTo(target, new(pathEdges));
                        }

                    }
            }
            foreach (Zone unfound in unfoundTargets)
            {
                changed = true;
                source.RemovePathTo(unfound);
            }
        }

        if (changed)
            Progression.CheckProgression();
    }
}