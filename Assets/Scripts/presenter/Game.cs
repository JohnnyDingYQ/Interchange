public static class Game
{
    public static ISaveSystemGateway SaveSystem { get; set; }

    public static void SaveGame()
    {
        SaveSystem.SaveGame();
    }

    public static void LoadGame()
    {
        SaveSystem.LoadGame();
        BuildManager.RedrawAllRoads();
    }
}