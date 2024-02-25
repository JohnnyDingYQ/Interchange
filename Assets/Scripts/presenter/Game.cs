using System.Collections.Generic;

public static class Game
{
    public static ISaveSystemGateway SaveSystem { get; set; }
    private static GameState gameState;
    public static GameState GameState
    {
        get
        {
            gameState ??= new();
            return gameState;
        }
        set
        {
            gameState = value;
        }
    }

    public static SortedDictionary<int, Road> RoadWatcher
    {
        get
        {
            return GameState.RoadWatcher;
        }
        set
        {
            GameState.RoadWatcher = value;
        }
    }

    public static SortedDictionary<int, HashSet<Lane>> NodeWithLane
    {
        get
        {
            return GameState.NodeWithLane;
        }
        set
        {
            GameState.NodeWithLane = value;
        }
    }

    public static IEnumerable<Lane> GetLaneIterator()
    {
        foreach (Road r in RoadWatcher.Values)
        {
            foreach (Lane l in r.Lanes)
            {
                yield return l;
            }
        }

    }

    public static void WipeGameState()
    {
        GameState = new();
    }

    public static void SaveGame()
    {
        SaveSystem.SaveGame();
    }

    public static void LoadGame()
    {
        SaveSystem.LoadGame();
        BuildManager.ComplyToNewGameState();
    }
}