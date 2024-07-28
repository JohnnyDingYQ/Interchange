using System.IO;
using UnityEngine;

public class Storage
{
    string savePath;

    public Storage()
    {
        savePath = System.IO.Path.Combine(Application.persistentDataPath, "saveFile");
    }
    public void Save(IPersistable o)
    {
        using var writer = new BinaryWriter(File.Open(savePath, FileMode.Create));
        o.Save(new Writer(writer));
    }

    public void Load(IPersistable o)
    {
        using var reader = new BinaryReader(File.Open(savePath, FileMode.Open));
        o.Load(new Reader(reader));
    }
}