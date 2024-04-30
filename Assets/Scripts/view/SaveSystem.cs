using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuikGraph;
using QuikGraph.Collections;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public static class SaveSystem
{
    public static void LoadGame()
    {
        Game.WipeState();
        string saveFile = Application.persistentDataPath + "/save0.json";

        if (File.Exists(saveFile))
        {
            string fileContents = File.ReadAllText(saveFile);
            // There is no server, whatever
            Game.GameState = JsonConvert.DeserializeObject<GameState>(fileContents, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });
            RestoreGameState();
        }
        else
        {
            throw new InvalidOperationException("Accessed save file does not exist");
        }

        static void RestoreGameState()
        {
            Game.Graph.AddVerticesAndEdgeRange(Game.GameState.GraphSave);
            EvaluteIntersections();
            foreach (Road r in Game.Roads.Values)
            {
                r.EvaluateBodyOutline();
                Game.InvokeInstantiateRoad(r);
            }
        }

        static void EvaluteIntersections()
        {
            HashSet<Intersection> evaluated = new();
            foreach (Road r in Game.Roads.Values)
                foreach (Intersection i in new Intersection[] {r.StartIntersection, r.EndIntersection})
                    if (!evaluated.Contains(i))
                    {
                        evaluated.Add(i);
                        i.EvaluateOutline();
                    }
        }
    }

    public static void SaveGame()
    {
        Game.GameState.GraphSave = Game.Graph.Edges.ToList();

        string s = JsonConvert.SerializeObject(Game.GameState, Formatting.None, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.Auto,
        });
        File.WriteAllText(Application.persistentDataPath + "/save0.json", s);
    }
}