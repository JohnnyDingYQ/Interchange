using System;
using System.IO;
using Unity.Mathematics;
using UnityEngine.Splines;

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
        Offset += 32 * 3;
        return new(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle()
        );
    }

    public BezierCurve ReadBezierCurve()
    {
        Offset += 32 * 12;
        return new(
            new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
        );
    }

    public T Read<T>()
    {
        if (typeof(T) == typeof(float))
            return (T)(object)ReadFloat();
        if (typeof(T) == typeof(float3))
            return (T)(object)ReadFloat3();
        if (typeof(T) == typeof(int))
            return (T)(object)ReadInt();
        if (typeof(T) == typeof(uint))
            return (T)(object)ReadUint();
        if (typeof(T) == typeof(bool))
            return (T)(object)ReadBool();
        if (typeof(T) == typeof(BezierCurve))
            return (T)(object)ReadBezierCurve();
        if (typeof(T).IsEnum)
            return (T)Enum.ToObject(typeof(T), ReadInt());
        throw new ArgumentException($"Type {typeof(T)} not supported by reader");

    }
}