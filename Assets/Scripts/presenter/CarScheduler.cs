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
        foreach (SourceZone source in Game.SourceZones.Values)
        {
            IEnumerable<TargetZone> randomOrder = source.ConnectedTargets.Keys.OrderBy(x => Game.Random.Next());
            foreach (TargetZone target in randomOrder)
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
        foreach (SourceZone source in Game.SourceZones.Values)
        {
            foreach (Vertex startV in source.Vertices)
            {
                TryFunc<Vertex, IEnumerable<Edge>> tryFunc = Graph.GetAStarTryFunc(startV);
                foreach (TargetZone target in  Game.TargetZones.Values)
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
        if (changed)
            Progression.CheckProgression();

        static void AddPath(SourceZone source, TargetZone target, IEnumerable<Edge> pathEdges)
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
            target.ConnectedSources.Add(source);
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
        foreach (SourceZone source in Game.SourceZones.Values.Where(z => z.ConnectedTargets.Count != 0))
        {
            HashSet<TargetZone> unfoundTargets = source.ConnectedTargets.Keys.ToHashSet();
            foreach (Vertex startV in source.Vertices)
            {
                TryFunc<Vertex, IEnumerable<Edge>> tryFunc = Graph.GetAStarTryFunc(startV);
                List<TargetZone> targets = new(source.ConnectedTargets.Keys);
                foreach (TargetZone target in targets)
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
            foreach (TargetZone unfound in unfoundTargets)
            {
                changed = true;
                source.ConnectedTargets.Remove(unfound);
                unfound.ConnectedSources.Remove(source);
            }
        }

        if (changed)
            Progression.CheckProgression();
    }
}