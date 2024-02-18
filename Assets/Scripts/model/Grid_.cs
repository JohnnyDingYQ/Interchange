using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;

public static class Grid
{
    public static int Level { get; set; }
    public static int Height { get; set; }
    public static int Width { get; set; }
    public static int Dim { get; set; }

    public static float2 GetCoordinate(int node)
    {
        if (node < 0)
            throw new ArgumentOutOfRangeException("given node cannot be negative");
        if (node > Height * Width)
            throw new ArgumentOutOfRangeException("given node overflows range");
        return new float2(node % Height, node / Height);
    }

    public static float GetDistance(int node1, int node2)
    {
        if (!IsValidNode(node1) || !IsValidNode(node2))
            throw new ArgumentOutOfRangeException("either or both nodes are out of range");
        float2 coord1 = GetCoordinate(node1);
        float2 coord2 = GetCoordinate(node2);
        return (float) Math.Sqrt(Math.Pow(coord1.x - coord2.x, 2) + Math.Pow(coord1.y - coord2.y, 2));
    }

    public static int GetIdByPos(float3 pos)
    {
        int x = (int)(pos.x / Dim);
        int z = (int)(pos.z / Dim);
        int id = x*Height + z;
        if (IsValidNode(id))
        {
            return id;
        }
        return -1;
    }

    public static float3 GetPosByID(int node)
    {
        if (!IsValidNode(node))
        {
            throw new ArgumentOutOfRangeException("Given tile Id is out of range");
        }
        int x = node / Height;
        int z = node % Height;
        
        return new float3(x * Dim + (float) Dim / 2, Level, z * Dim + (float) Dim / 2);
    }

    public static float3 SnapPosToGrid(float3 pos)
    {
        return GetPosByID(GetIdByPos(pos));
    }

    private static bool IsValidNode(int node)
    {
        return node < Height*Width && node >= 0;
    }
}
