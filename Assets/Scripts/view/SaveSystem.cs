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
                TypeNameHandling = TypeNameHandling.Auto
            });
            Game.Graph.AddVerticesAndEdgeRange(Game.GameState.GraphSave);
            foreach (Intersection i in Game.Intersections.Values)
            {
                i.UpdateNormalAndPlane();
            }
            foreach (Road road in Game.Roads.Values)
                Game.InvokeInstantiateRoad(road);
        }
        else
        {
            throw new InvalidOperationException("Accessed save file does not exist");
        }
    }

    public static void SaveGame()
    {
        Game.GameState.GraphSave = Game.Graph.Edges.ToList();

        string s = JsonConvert.SerializeObject(Game.GameState, Formatting.None, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        });
        File.WriteAllText(Application.persistentDataPath + "/save0.json", s);
    }
}