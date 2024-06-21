using System.Collections.Generic;
using System.Collections.ObjectModel;
using QuikGraph.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class Zone
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public SortedDictionary<uint, int> Demands { get; set; }
    private readonly List<Vertex> inVertices;
    private readonly List<Road> inRoads;
    private readonly List<Vertex> outVertices;
    private readonly List<Road> outRoads;

    public ReadOnlyCollection<Road> InRoads { get { return inRoads.AsReadOnly(); } }
    public ReadOnlyCollection<Road> OutRoads { get { return outRoads.AsReadOnly(); } }
    public float CarSpawnInterval { get; set; }

    public Zone(uint id)
    {
        Assert.AreNotEqual(0, id);
        Id = id;
        inVertices = new();
        inRoads = new();
        outVertices = new();
        outRoads = new();
        Demands = new();
        CarSpawnInterval = 0;
    }
    public void AddInRoad(Road road)
    {
        foreach (Lane lane in road.Lanes)
            inVertices.Add(lane.EndVertex);
        road.EndZoneId = Id;
        inRoads.Add(road);
    }

    public void AddOutRoad(Road road)
    {
        foreach (Lane lane in road.Lanes)
            outVertices.Add(lane.StartVertex);
        road.StartZoneId = Id;
        outRoads.Add(road);
    }

    public Vertex GetRandomInVertex()
    {
        if (inVertices.Count == 0)
            return null;
        return inVertices[MyNumerics.GetRandomIndex(inVertices.Count)];
    }

    public Vertex GetRandomOutVertex()
    {
        if (outVertices.Count == 0)
            return null;
        return outVertices[MyNumerics.GetRandomIndex(outVertices.Count)];
    }

    public void RemoveRoad(Road road)
    {
        foreach (Lane lane in road.Lanes)
            foreach (Vertex v in new Vertex[] { lane.StartVertex, lane.EndVertex })
            {
                inVertices.Remove(v);
                outVertices.Remove(v);
            }
        inRoads.Remove(road);
        outRoads.Remove(road);
        if (road.StartZoneId == Id)
            road.StartZoneId = 0;
        if (road.EndZoneId == Id)
            road.EndZoneId = 0;
    }

    public bool IsConsistent()
    {
        List<Vertex> inVerticesCopy = new(inVertices);
        List<Vertex> outVerticesCopy = new(outVertices);
        foreach (Road road in inRoads)
            foreach (Lane lane in road.Lanes)
                if (inVerticesCopy.Contains(lane.EndVertex))
                    inVerticesCopy.Remove(lane.EndVertex);
                else
                    return false;
        foreach (Road road in outRoads)
            foreach (Lane lane in road.Lanes)
                if (outVerticesCopy.Contains(lane.StartVertex))
                    outVerticesCopy.Remove(lane.StartVertex);
                else
                    return false;
        return inVerticesCopy.Count == 0 && outVerticesCopy.Count == 0;
    }
}