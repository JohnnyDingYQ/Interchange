public static class SaveSystem
{
    public static int LoadGame()
    {
        Game.WipeState();
        Storage storage = new();
        int savedBytes = storage.Load(Game.GameSave);
        RestoreGameState();
        return savedBytes;

        static void RestoreGameState()
        {
            Graph.CancelBinding();
            Graph.AddVerticesAndPathRange(Game.Paths.Values);
            Graph.ApplyBinding();
            foreach (Intersection i in Game.Intersections.Values)
                IntersectionUtil.EvaluateOutline(i);
            foreach (Road r in Game.Roads.Values)
            {
                r.SetInnerOutline();
                Game.InvokeRoadAdded(r);
            }
        }
    }

    public static int SaveGame()
    {
        Storage storage = new();
        return storage.Save(Game.GameSave);
    }

}