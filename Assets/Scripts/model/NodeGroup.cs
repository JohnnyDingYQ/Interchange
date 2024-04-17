using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.ObjectModel;

public class NodeGroup : IEnumerable<Node>
{
    private readonly List<Node> nodes;
    public ReadOnlyCollection<Node> Nodes { get { return nodes.AsReadOnly(); } }
    public int Count { get { return Nodes.Count; } }
    private readonly HashSet<Road> inRoads;
    public ReadOnlySet<Road> InRoads { get { return inRoads.AsReadOnly(); } }
    private readonly HashSet<Road> outRoads;
    public ReadOnlySet<Road> OutRoads { get { return outRoads.AsReadOnly(); } }
    public Plane Plane { get; private set; }
    public float3 PointOnInside { get; private set; }

    public NodeGroup(Node node)
    {
        HashSet<Node> h = new();
        foreach (Road road in node.GetRoads(Direction.In))
            foreach (Lane lane in road.Lanes)
                h.Add(lane.EndNode);
        foreach (Road road in node.GetRoads(Direction.Out))
            foreach (Lane lane in road.Lanes)
                h.Add(lane.StartNode);
        List<Node> l = new(h);
        l.Sort();
        nodes = l;

        outRoads = new();
        inRoads = new();
        foreach (Node n in nodes)
            outRoads.UnionWith(n.GetRoads(Direction.Out));
        foreach (Node n in nodes)
            inRoads.UnionWith(n.GetRoads(Direction.In));

        Road randomInRoad = InRoads.First();
        Plane = new(randomInRoad.EndPos, randomInRoad.EndPos + randomInRoad.GetNormal(1), randomInRoad.EndPos - new float3(0, 1, 0));
        PointOnInside = randomInRoad.PivotPos;
    }

    public Node FirstWithInRoad()
    {
        foreach (Node n in nodes)
            if (n.GetLanes(Direction.In).Count != 0)
                return n;
        return null;
    }

    public Node LastWithInRoad()
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
            if (nodes[i].GetLanes(Direction.In).Count != 0)
                return nodes[i];
        return null;
    }

    public IEnumerator<Node> GetEnumerator()
    {
        foreach (Node node in nodes)
            yield return node;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}