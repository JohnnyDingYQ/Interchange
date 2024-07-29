using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class Writer
{
    readonly BinaryWriter writer;
    public int Offset { get; private set; }

    public Writer(BinaryWriter binaryWriter)
    {
        writer = binaryWriter;
    }

    public void Write(float value)
    {
        Offset += 32;
        writer.Write(value);
    }

    public void Write(uint value)
    {
        Offset += 32;
        writer.Write(value);
    }

    public void Write(int value)
    {
        Offset += 32;
        writer.Write(value);
    }

    public void Write(bool value)
    {
        Offset += 8;
        writer.Write(value);
    }

    public void Write(float3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }
}