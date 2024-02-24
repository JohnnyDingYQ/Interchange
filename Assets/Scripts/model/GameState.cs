using System.Collections.Generic;

public class GameState
{
    private SortedDictionary<int, Road> roadWatcher;
    public SortedDictionary<int, Road> RoadWatcher
    {
        get
        {
            roadWatcher ??= new();
            return roadWatcher;
        }
        set
        {
            roadWatcher = value;
        }
    }
    private SortedDictionary<int, HashSet<Lane>> nodeWithLane;
    public SortedDictionary<int, HashSet<Lane>> NodeWithLane {
        get {
            nodeWithLane ??= new();
            return nodeWithLane;
        }
        set {
            nodeWithLane = value;
        }
    }
}