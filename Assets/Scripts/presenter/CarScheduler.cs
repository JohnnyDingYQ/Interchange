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
                    startVertex.ScheduleCooldown = SourceZone.ScheduleInterval + deltaTime;
                }
            }
            foreach (Vertex vertex in source.Vertices)
            {
                vertex.ScheduleCooldown -= deltaTime;
            }
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
                            AddPath(source, target, pathEdges);
                            break;
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
            Progression.CheckProgression();
    }
}