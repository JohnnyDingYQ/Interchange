using System.Linq;
using Assets.Scripts.model.Roads;
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
            Graph.CancelBinding();
            foreach (Vertex v in Game.Vertices.Values)
                Graph.AddVertex(v);

            foreach (Edge e in Game.Edges.Values)
                Graph.AddEdge(e);

            foreach (Curve c in Game.Curves.Values)
                c.CreateDistanceCache();

            foreach (Lane l in Game.Lanes.Values)
            {
                l.InitCurve();
                l.InitInnerEdge();

                Graph.AddEdge(l.InnerEdge);
            }

            Graph.ApplyBinding();
            
            // set inner outline
            foreach (Road r in Game.Roads.Values)
                r.SetInnerOutline();

            // set outline at ends 
            foreach (Intersection i in Game.Intersections.Values)
                IntersectionUtil.EvaluateOutline(i);

            // paths
            CarScheduler.FindNewConnection();
            foreach (Car car in Game.Cars.Values)
            {
                car.UpdatePath();
            }
        }
    }

    public int SaveGame()
    {
        Assert.IsTrue(Game.GameSave.IPersistableAreInDict());
        Build.RemoveAllGhostRoads();
        Storage storage = new(filename);
        return storage.Save(Game.GameSave);
    }

}