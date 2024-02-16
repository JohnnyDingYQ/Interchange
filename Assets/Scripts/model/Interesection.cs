using System.Collections.Generic;
using System.Linq;

public class Intersection
{
    private Dictionary<int, List<Lane>> nodeWithLane;
    /// <summary>
    /// Key: node, Value: Lane connected to it
    /// </summary>
    public Dictionary<int, List<Lane>> NodeWithLane {
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
        foreach (List<Lane> lanes in nodeWithLane.Values)
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
        List<Lane> lanes = nodeWithLane[node];
        foreach (Lane lane in lanes)
        {
            if (lane.Road != mainRoad)
            {
                return lane.Road;
            }
        } 
        return null;
    }
}