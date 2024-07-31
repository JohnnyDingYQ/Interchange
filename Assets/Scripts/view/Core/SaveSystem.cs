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
        RestoreGameState();
        Assert.IsTrue(Game.GameSave.IPersistableAreInDict());
        return loadedBytes;

        static void RestoreGameState()
        {
            Graph.CancelBinding();
            Graph.AddVerticesAndPathRange(Game.Paths.Values);
            Graph.ApplyBinding();
            foreach (Curve c in Game.Curves.Values)
                c.CreateDistanceCache();

            // create road and inner outline
            foreach (Road r in Game.Roads.Values)
            {
                r.SetInnerOutline();
                Game.InvokeRoadAdded(r);
            }
            // create outline at ends for all roads
            foreach (Intersection i in Game.Intersections.Values)
                IntersectionUtil.EvaluateOutline(i);


            // update mesh
            foreach (Intersection i in Game.Intersections.Values)
                Game.UpdateIntersection(i);
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