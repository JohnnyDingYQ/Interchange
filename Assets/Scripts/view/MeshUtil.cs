using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class MeshUtil
{
    public static Mesh GetMesh(Road road)
    {
        int leftLength, rightLength;
        leftLength = road.LeftOutline.GetSize();
        rightLength = road.RightOutline.GetSize();
        List<float3> verts = road.LeftOutline.GetConcatenated();
        verts.AddRange(road.RightOutline.GetConcatenated());
        List<Vector3> v3Verts = verts.ConvertAll(new Converter<float3, Vector3>(ToVector3));
        Mesh mesh = new();
        List<int> tris = new();
        List<Vector2> uvs = new();
        for (int i = 1; i < leftLength; i++)
            tris.AddRange(new int[] { i, leftLength + i - 1 < leftLength + rightLength ? leftLength + i - 1 : leftLength + rightLength - 1, i - 1 });
        for (int i = 1; i < rightLength; i++)
            tris.AddRange(new int[] { leftLength + i - 1, i < leftLength ? i : leftLength - 1, leftLength + i });
        for (int i = 0; i < leftLength; i++)
            uvs.Add(new(0, (float)i / (leftLength - 1)));
        for (int i = 0; i < rightLength; i++)
            uvs.Add(new(1, (float)i / (rightLength - 1)));
        mesh.SetVertices(v3Verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetNormals(Enumerable.Repeat(Vector3.up, v3Verts.Count).ToList());
        mesh.SetUVs(0, uvs);
        return mesh;

        static Vector3 ToVector3(float3 pt)
        {
            return new Vector3(pt.x, pt.y, pt.z);
        }
    }
}