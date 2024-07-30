using System;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

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
        Offset += 32 * 3;
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }

    public void Write(BezierCurve bezierCurve)
    {
        Offset += 32 * 12;
        writer.Write(bezierCurve.P0.x);
        writer.Write(bezierCurve.P0.y);
        writer.Write(bezierCurve.P0.z);
        writer.Write(bezierCurve.P1.x);
        writer.Write(bezierCurve.P1.y);
        writer.Write(bezierCurve.P1.z);
        writer.Write(bezierCurve.P2.x);
        writer.Write(bezierCurve.P2.y);
        writer.Write(bezierCurve.P2.z);
        writer.Write(bezierCurve.P3.x);
        writer.Write(bezierCurve.P3.y);
        writer.Write(bezierCurve.P3.z);
    }

    public void Write(Enum value)
    {
        Offset += 32;
        writer.Write(Convert.ToInt32(value));
    }
}