using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public static class MeshUtil
{
    static readonly List<int> tris = new();
    static readonly List<Vector2> uvs = new();
    static readonly List<Vector3> normals = new();
    static readonly List<Vector3> v3Verts = new();
    public static Mesh GetMesh(Road road)
    {
        int leftLength, rightLength;
        leftLength = road.LeftOutline.GetSize();
        rightLength = road.RightOutline.GetSize();
        Assert.IsTrue(leftLength == rightLength);
        v3Verts.Clear();
        foreach (float3 pos in road.LeftOutline)
        {
            v3Verts.Add(ToVector3(pos));
            normals.Add(Vector3.up);
        }
        foreach (float3 pos in road.RightOutline)
        {
            v3Verts.Add(ToVector3(pos));
            normals.Add(Vector3.up);
        }
        Mesh mesh = new();
        tris.Clear();
        uvs.Clear();
        normals.Clear();
        for (int i = 1; i < leftLength; i++)
        {
            tris.Add(i);
            tris.Add(leftLength + i - 1);
            tris.Add(i - 1);
        }
        for (int i = 1; i < rightLength; i++)
        {
            tris.Add(leftLength + i - 1);
            tris.Add(i);
            tris.Add(leftLength + i);
        }
        float numRepeat = road.Length / Constants.LaneWidth;

        float midStart = Constants.VertexDistanceFromRoadEnds / road.Length;
        float midEnd = 1 - midStart;
        for (float i = 0; i < RoadOutline.EndsNumPoint; i++)
            uvs.Add(new(0, i / (RoadOutline.EndsNumPoint - 1) * midStart * numRepeat));
        for (float i = 0; i < RoadOutline.MidNumPoint; i++)
            uvs.Add(new(0, (midStart + i / (RoadOutline.MidNumPoint - 1) * (midEnd - midStart)) * numRepeat));
        for (float i = 0; i < RoadOutline.EndsNumPoint; i++)
            uvs.Add(new(0, (midEnd + i / (RoadOutline.EndsNumPoint - 1) * midStart) * numRepeat));

        for (float i = 0; i < RoadOutline.EndsNumPoint; i++)
            uvs.Add(new(1, i / (RoadOutline.EndsNumPoint - 1) * midStart * numRepeat));
        for (float i = 0; i < RoadOutline.MidNumPoint; i++)
            uvs.Add(new(1, (midStart + i / (RoadOutline.MidNumPoint - 1) * (midEnd - midStart))  * numRepeat));
        for (float i = 0; i < RoadOutline.EndsNumPoint; i++)
            uvs.Add(new(1, (midEnd + i / (RoadOutline.EndsNumPoint - 1) * midStart) * numRepeat));

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