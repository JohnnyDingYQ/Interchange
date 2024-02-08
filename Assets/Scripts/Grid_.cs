using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public static int Level { get; set; }
    public static int Height { get; set; }
    public static int Width { get; set; }
    public static int Dim { get; set; }
    private static Color clear = new Color(1.0f, 1.0f, 1.0f, 0.1f);
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Returns the tile instance where the cursor rests on
    /// </summary>
    public static int GetIdByCursor()
    {
        int x = (int)(Main.MouseWorldPos.x / Dim);
        int z = (int)(Main.MouseWorldPos.z / Dim);
        int id = x*Height + z;
        if (id < Height*Width && id >= 0)
        {
            Log.Info.Log($"Tile {id} selected");
            return id;
        }
        Log.Info.Log($"Tile -1 selected");
        return -1;
    }

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
