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
        for (int i = 1; i < leftLength; i++)
            tris.AddRange(new int[] { i, leftLength + i - 1 < leftLength + rightLength ? leftLength + i - 1 : leftLength + rightLength - 1, i - 1 });
        for (int i = 1; i < rightLength; i++)
            tris.AddRange(new int[] { leftLength + i - 1, i < leftLength ? i : leftLength - 1, leftLength + i });
        mesh.SetVertices(v3Verts);
        mesh.SetTriangles(tris, 0);
        return mesh;

        static Vector3 ToVector3(float3 pt)
        {
            return new Vector3(pt.x, pt.y, pt.z);
        }
    }
}