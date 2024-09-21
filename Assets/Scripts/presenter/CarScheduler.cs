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
        Game.RoadRemoved += FindNewConnection;
    }

    public static void Schedule(float deltaTime)
    {
        if (Game.Cars.Count >= Constants.MaxCarCount)
            return;
        foreach (Zone source in Game.Zones.Values)
        {
            foreach (Zone target in source.ConnectedZones)
            {
                List<Path> paths = source.GetPathsTo(target);
                Assert.IsNotNull(paths);
                foreach (Path path in paths)
                {
                    Vertex startVertex = path.StartVertex;
                    Assert.IsTrue(source.Vertices.Contains(startVertex));
                    if (startVertex.ScheduleCooldown <= 0)
                    {
                        Game.RegisterCar(new(source, target, path));
                        startVertex.ScheduleCooldown = GenerateVertexInterval() + deltaTime;
                        break;
                    }
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
        foreach (Zone source in Game.Zones.Values)
        {
            source.ClearPath();
            foreach (Vertex startV in source.Vertices)
            {
                TryFunc<Vertex, IEnumerable<Edge>> tryFunc = Graph.GetAStarTryFunc(startV);
                foreach (Zone target in Game.Zones.Values)
                {
                    if (target == source)
                        continue;
                    foreach (Vertex endV in target.Vertices.OrderBy(v => v.Id)) // order for testing purposes
                        if (tryFunc(endV, out IEnumerable<Edge> pathEdges))
                            AddPath(source, target, pathEdges);
                }
            }
        }
        Progression.CheckProgression();

        static void AddPath(Zone source, Zone target, IEnumerable<Edge> pathEdges)
        {
            Path newPath = new(pathEdges);
            IEnumerable<Path> paths = source.GetPathsTo(target).Where(p => p.StartVertex == newPath.StartVertex);
            if (paths.Count() != 0)
            {
                Path existingPath = paths.Single();
                if (existingPath.Cost > newPath.Cost)
                {
                    source.RemovePathTo(target, existingPath);
                    source.AddPathTo(target, newPath);
                }
            }
            else
            {
                source.AddPathTo(target, newPath);
            }
        }
    }

    public static void FindNewConnection(Road road)
    {
        if (!road.IsGhost)
            FindNewConnection();
    }

}