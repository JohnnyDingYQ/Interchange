using System;
using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms;
using Unity.Mathematics;

public class Car
{
    public static event Action<Car> TravelCoroutine;
    public bool IsAtDestination { get; set; }
    public Zone Origin { get; private set; }
    public Zone Destination { get; private set; }
    public IEnumerable<Path> paths;
    public Car(Zone origin, Zone destination)
    {
        IsAtDestination = false;
        Origin = origin;
        Destination = destination;
    }
    public void Travel()
    {
        Vertex startV = Origin.GetRandomOutVertex();
        Vertex endV = Destination.GetRandomInVertex();
        TryFunc<Vertex, IEnumerable<Path>> tryFunc = Game.Graph.ShortestPathsAStar(
            (Path p) => p.Length,
            (Vertex to) => math.distance(startV.Pos, to.Pos),
            startV
        );
        tryFunc(endV, out paths);
        if (paths != null)
            TravelCoroutine?.Invoke(this);
    }
}