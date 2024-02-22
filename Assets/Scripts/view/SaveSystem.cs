using System;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class SaveSystem : ISaveSystemGateway
{
    public void LoadGame()
    {
        string saveFile = Application.persistentDataPath + "/save0.json";

        if (File.Exists(saveFile))
        {
            string fileContents = File.ReadAllText(saveFile);
            BuildManager.RoadWatcher = JsonConvert.DeserializeObject<Dictionary<int, Road>>(fileContents);
        }
        else
        {
            throw new InvalidOperationException("Accessed save file does not exist");
        }
    }

    public void SaveGame()
    {
        var s = JsonConvert.SerializeObject(BuildManager.RoadWatcher, Formatting.Indented, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        });
        File.WriteAllText(Application.persistentDataPath + "/save0.json", s);
    }
}