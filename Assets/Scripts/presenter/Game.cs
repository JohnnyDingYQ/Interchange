using System.Collections.Generic;
using Codice.CM.Triggers;

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

    public static SortedDictionary<int, Road> Roads
    {
        get
        {
            return GameState.Roads;
        }
        set
        {
            GameState.Roads = value;
        }
    }

    public static SortedDictionary<int, Node> Nodes
    {
        get
        {
            return GameState.Nodes;
        }
        set
        {
            GameState.Nodes = value;
        }
    }

    public static int NextAvailableNodeId {
        get
        {
            return GameState.NextAvailableNodeId;
        }
        set
        {
            GameState.NextAvailableNodeId = value;
        }
    }
    public static int NextAvailableRoadId {
        get
        {
            return GameState.NextAvailableRoadId;
        }
        set
        {
            GameState.NextAvailableRoadId = value;
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