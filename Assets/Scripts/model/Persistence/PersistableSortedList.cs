using System.Collections.Generic;

public class PersistableSortedList : SortedList<int, Node>, IPersistable
{
    public uint Id { get ; set ; }
    
    public void Save(Writer writer)
    {
        writer.Write(Count);
        foreach (var n in this)
        {
            writer.Write(n.Key);
            writer.Write(n.Value.Id);
        }
    }

    public void Load(Reader reader)
    {
        int nodeCount = reader.ReadInt();
        for (int i = 0; i < nodeCount; i++)
        {
            int index = reader.ReadInt();
            this[index] = (Node) reader.CreateInstance(typeof(Node), reader.ReadUint());
        }
    }
}