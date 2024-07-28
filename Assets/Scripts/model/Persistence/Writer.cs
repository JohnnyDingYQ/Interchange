using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class Writer
{
    readonly BinaryWriter writer;

    public Writer(BinaryWriter binaryWriter)
    {
        writer = binaryWriter;
    }

    public void Write(float value)
    {
        writer.Write(value);
    }

    public void Write(uint value)
    {
        writer.Write(value);
    }

    public void Write(int value)
    {
        writer.Write(value);
    }

    public void Write(bool value)
    {
        writer.Write(value);
    }

    public void Write(float3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }
}