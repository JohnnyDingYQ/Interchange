using System;
using Unity.Mathematics;

public static class Grid
{
    public static int Level { get; set; }
    public static int Height { get; set; }
    public static int Width { get; set; }
    public static int Dim { get; set; }

    public static int GetIdByPos(float3 pos)
    {
        int x = (int)(pos.x / Dim);
        int z = (int)(pos.z / Dim);
        int id = x*Height + z;
        if (IsValidId(id))
        {
            return id;
        }
        return -1;
    }

    public static float3 GetWorldPosByID(int id)
    {
        if (!IsValidId(id))
        {
            throw new ArgumentOutOfRangeException("Given tile Id is out of range");
        }
        int x = id / Height;
        int z = id % Height;
        
        return new float3(x * Dim + (float) Dim / 2, Level, z * Dim + (float) Dim / 2);
    }

    private static bool IsValidId(int id)
    {
        return id < Height*Width && id >= 0;
    }
}
