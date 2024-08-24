using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.model.Roads;
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
            IEnumerable<Zone> randomOrder = source.ConnectedTargets.Keys.OrderBy(x => Game.Random.Next());
            foreach (Zone target in randomOrder)
            {
                Path path = source.ConnectedTargets[target];
                Vertex startVertex = GetStartingVertex(path);
                Assert.IsTrue(source.Vertices.Contains(startVertex));
                if (startVertex.ScheduleCooldown <= 0)
                {
                    Game.RegisterCar(new(source, target));
                    startVertex.ScheduleCooldown = Vertex.ScheduleInterval + deltaTime;
                }
            }
            foreach (Vertex vertex in source.Vertices)
                vertex.ScheduleCooldown -= deltaTime;
        }

        static Vertex GetStartingVertex(Path path)
        {
            return path.Edges.First().Source;
        }
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
                        tryFunc(endV, out IEnumerable<Edge> pathEdges);
                        if (pathEdges != null)
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
            float length = 0;
            foreach (Edge edge in pathEdges)
                length += edge.Length;
            if (source.ConnectedTargets.TryGetValue(target, out Path existingPath))
            {
                if (existingPath.Length > length)
                {
                    Path shorterPath = new(pathEdges);
                    source.ConnectedTargets[target] = shorterPath;
                }
            }
            else
            {
                Path path = new(pathEdges);
                source.ConnectedTargets[target] = path;
            }
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
        foreach (Zone source in Game.Zones.Values.Where(z => z.ConnectedTargets.Count != 0))
        {
            HashSet<Zone> unfoundTargets = source.ConnectedTargets.Keys.ToHashSet();
            foreach (Vertex startV in source.Vertices)
            {
                TryFunc<Vertex, IEnumerable<Edge>> tryFunc = Graph.GetAStarTryFunc(startV);
                List<Zone> targets = new(source.ConnectedTargets.Keys);
                foreach (Zone target in targets)
                    foreach (Vertex endV in target.Vertices)
                    {
                        tryFunc(endV, out IEnumerable<Edge> pathEdges);
                        if (pathEdges != null)
                        {
                            unfoundTargets.Remove(target);
                            source.ConnectedTargets[target] = new(pathEdges);
                        }

                    }
            }
            foreach (Zone unfound in unfoundTargets)
            {
                changed = true;
                source.ConnectedTargets.Remove(unfound);
            }
        }

        if (changed)
            Progression.CheckProgression();
    }
}