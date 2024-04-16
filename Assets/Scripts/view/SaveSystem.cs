using System;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public static class SaveSystem
{
    public static void LoadGame()
    {
        string saveFile = Application.persistentDataPath + "/save0.json";

        if (File.Exists(saveFile))
        {
            string fileContents = File.ReadAllText(saveFile);
            // Potential security vulnerability here when deserializing from an external source
            // However, this is an offline game and the player can tamper as they see fit
            Game.GameState = JsonConvert.DeserializeObject<GameState>(fileContents, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            foreach (Road road in Game.Roads.Values)
                road.RestoreFromDeserialization();
        }
        else
        {
            throw new InvalidOperationException("Accessed save file does not exist");
        }
    }

    public static void SaveGame()
    {
        string s = JsonConvert.SerializeObject(Game.GameState, Formatting.Indented, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            TypeNameHandling = TypeNameHandling.Auto
        });
        File.WriteAllText(Application.persistentDataPath + "/save0.json", s);
    }
}