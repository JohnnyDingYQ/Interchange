using System.IO;
using Unity.Mathematics;

public class Reader
{
    readonly BinaryReader reader;
    public int Offset { get; private set; }

    public Reader(BinaryReader binaryReader)
    {
        reader = binaryReader;
        Offset = 0;
    }

    public float ReadFloat()
    {
        Offset += 32;
        return reader.ReadSingle();
    }

    public uint ReadUint()
    {
        Offset += 32;
        return reader.ReadUInt32();
    }

    public int ReadInt()
    {
        Offset += 32;
        return reader.ReadInt32();
    }

    public bool ReadBool()
    {
        Offset += 8;
        return reader.ReadBoolean();
    }

    public float3 ReadFloat3()
    {
        return new(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );
    }
}