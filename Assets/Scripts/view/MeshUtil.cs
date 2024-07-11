using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class MeshUtil
{
    static readonly List<int> tris = new();
    static readonly List<Vector2> uvs = new();
    static readonly List<Vector3> normals = new();
    public static Mesh GetMesh(Road road)
    {
        int leftLength, rightLength;
        leftLength = road.LeftOutline.GetSize();
        rightLength = road.RightOutline.GetSize();
        List<float3> verts = road.LeftOutline.GetConcatenated();
        verts.AddRange(road.RightOutline.GetConcatenated());
        List<Vector3> v3Verts = verts.ConvertAll(new Converter<float3, Vector3>(ToVector3));
        Mesh mesh = new();
        tris.Clear();
        uvs.Clear();
        normals.Clear();
        for (int i = 1; i < leftLength; i++)
        {
            tris.Add(i);
            tris.Add(leftLength + i - 1 < leftLength + rightLength ? leftLength + i - 1 : leftLength + rightLength - 1);
            tris.Add(i - 1);
        }
        for (int i = 1; i < rightLength; i++)
        {
            tris.Add(leftLength + i - 1);
            tris.Add(i < leftLength ? i : leftLength - 1);
            tris.Add(leftLength + i);
        }
        float numRepeat = road.Length / (Constants.LaneWidth * 3.8f);
        for (float i = 0; i < leftLength; i++)
        {
            uvs.Add(new(0, i / (leftLength - 1) * numRepeat));
            normals.Add(Vector3.up);
        }
        for (float i = 0; i < rightLength; i++)
        {
            uvs.Add(new(1, i / (rightLength - 1) * numRepeat));
            normals.Add(Vector3.up);
        }
        
        mesh.SetVertices(v3Verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        return mesh;

        static Vector3 ToVector3(float3 pt)
        {
            return new Vector3(pt.x, pt.y, pt.z);
        }
    }
}