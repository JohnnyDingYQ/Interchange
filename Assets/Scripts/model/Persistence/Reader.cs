using System.IO;
using Unity.Mathematics;

public class Reader
{
    readonly BinaryReader reader;

    public Reader(BinaryReader binaryReader)
    {
        reader = binaryReader;
    }

    public float ReadFloat()
    {
        return reader.ReadSingle();
    }

    public uint ReadUint()
    {
        return reader.ReadUInt32();
    }

    public int ReadInt()
    {
        return reader.ReadInt32();
    }

    public bool ReadBool()
    {
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