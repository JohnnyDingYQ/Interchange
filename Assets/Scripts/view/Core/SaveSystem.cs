using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Roads;
using UnityEngine;
using UnityEngine.Assertions;

public class SaveSystem
{
    string filename;

    public SaveSystem(string fName)
    {
        filename = fName;
    }

    public int LoadGame()
    {
        Game.WipeState();
        Storage storage = new(filename);
        int loadedBytes = storage.Load(Game.GameSave);
        InitializeGameSave();
        Assert.IsTrue(Game.GameSave.IPersistableAreInDict());
        return loadedBytes;

        static void InitializeGameSave()
        {
            Game.SetupZones();

            foreach (Vertex v in Game.Vertices.Values)
                Graph.AddVertex(v);

            foreach (Edge e in Game.Edges.Values)
                Graph.AddEdge(e);

            foreach (Curve c in Game.Curves.Values)
                c.CreateDistanceCache();

            foreach (Lane l in Game.Lanes.Values)
            {
                l.InitCurve();
            }

            // set inner outline
            foreach (Road r in Game.Roads.Values)
                r.SetInnerOutline();

            // set outline at ends 
            foreach (Intersection i in Game.Intersections.Values)
                IntersectionUtil.EvaluateOutline(i);

            // paths
            CarScheduler.FindNewConnection();
        }
    }

    public int SaveGame()
    {
        Build.RemoveAllGhostRoads();
        Assert.IsTrue(Game.GameSave.IPersistableAreInDict());
        Storage storage = new(filename);
        return storage.Save(Game.GameSave);
    }

}