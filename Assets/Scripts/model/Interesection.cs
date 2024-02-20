using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;

public class Intersection
{
    private Dictionary<int, HashSet<Lane>> nodeWithLane;
    /// <summary>
    /// Key: node, Value: Lane connected to it
    /// </summary>
    public Dictionary<int, HashSet<Lane>> NodeWithLane {
        get {
            nodeWithLane ??= new();
            return nodeWithLane;
        }
        set {
            nodeWithLane = value;
        }
    }

    public List<Road> Roads {get; set;}

    public Road GetMainRoad()
    {
        int max = 0;
        Road roadWithMostLane = null;
        foreach (Road road in Roads)
        {
            int size = road.Lanes.Count();
            if (size > max)
            {
                max = size;
                roadWithMostLane = road;
            }
        }
        return roadWithMostLane;
    }

    public bool IsNodeConnected(int node)
    {
        return NodeWithLane[node].Count > 1;
    }

    public List<int> GetNodes()
    {
        List<int> keys = new();
        foreach (int key in nodeWithLane.Keys)
        {
            keys.Add(key);
        }
        return keys;
    }
    /// <summary>
    /// When true: Intersection connects two road with completely aligned lanes
    /// </summary>
    /// <returns></returns>
    public bool IsRepeating()
    {
        if (Roads.Count != 2)
        {
            return false;
        }
        foreach (HashSet<Lane> lanes in nodeWithLane.Values)
        {
            if (lanes.Count < 2)
            {
                return false;
            }
            
        }
        return true;
    }

    public Road GetMinorRoadofNode(int node)
    {
        Road mainRoad = GetMainRoad();
        HashSet<Lane> lanes = nodeWithLane[node];
        foreach (Lane lane in lanes)
        {
            if (lane.Road != mainRoad)
            {
                return lane.Road;
            }
        } 
        return null;
    }

    public bool RoadEntersIntersection(Road road)
    {
        foreach (var(key, value) in nodeWithLane)
        {
            if (key == road.Lanes[0].End)
            {
                return true;
            }
            if (key == road.Lanes[0].Start)
            {
                return false;
            }
        }
        throw new InvalidOperationException("Main Road must either enter or exit the intersection");
    }
}