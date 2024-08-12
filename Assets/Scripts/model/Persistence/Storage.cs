using System.IO;
using UnityEngine;

public class Storage
{
    string savePath;

    public Storage(string filename)
    {
        savePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
    }
    public int Save(IPersistable o)
    {
        using var writer = new BinaryWriter(File.Open(savePath, FileMode.OpenOrCreate));
        Writer myWriter = new(writer);
        o.Save(myWriter);
        return myWriter.Offset;
    }

    public int Load(IPersistable o)
    {
        using var reader = new BinaryReader(File.Open(savePath, FileMode.Open));
        Reader myReader = new(reader);
        o.Load(myReader);
        return myReader.Offset;
    }
}