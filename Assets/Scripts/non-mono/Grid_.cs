using UnityEngine;

public static class Grid
{
    public static int Level { get; set; }
    public static int Height { get; set; }
    public static int Width { get; set; }
    public static int Dim { get; set; }

    public static int GetIdByPos(Vector3 pos)
    {
        int x = (int)(pos.x / Dim);
        int z = (int)(pos.z / Dim);
        int id = x*Height + z;
        if (id < Height*Width && id >= 0)
        {
            return id;
        }
        return -1;
    }

    public static Vector3 GetWorldPosByID(int id)
    {
        int x = id / Height;
        int z = id % Height;
        
        return new Vector3(x * Dim + (float) Dim / 2, Level, z * Dim + (float) Dim / 2);
    }

}
