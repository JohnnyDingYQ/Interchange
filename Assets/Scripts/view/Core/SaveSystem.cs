using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public static class SaveSystem
{
    public static int LoadGame()
    {
        Game.WipeState();
        Storage storage = new();
        int loadedBytes = storage.Load(Game.GameSave);
        InitializeGameSave();
        Assert.IsTrue(Game.GameSave.IPersistableAreInDict());
        return loadedBytes;

        static void InitializeGameSave()
        {
            Graph.CancelBinding();
            Graph.AddVerticesAndEdgeRange(Game.Edges.Values);
            Graph.ApplyBinding();


            foreach (Curve c in Game.Curves.Values)
                c.CreateDistanceCache();
            // set inner outline
            foreach (Road r in Game.Roads.Values)
                r.SetInnerOutline();
            // set outline at ends 
            foreach (Intersection i in Game.Intersections.Values)
                IntersectionUtil.EvaluateOutline(i);
        }
    }

    public static int SaveGame()
    {
        Assert.IsTrue(Game.GameSave.IPersistableAreInDict());
        Build.RemoveAllGhostRoads();
        Storage storage = new();
        return storage.Save(Game.GameSave);
    }

}