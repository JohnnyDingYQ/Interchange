using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public static class MeshUtil
{
    static readonly List<int> tris = new();
    static readonly List<Vector2> uvs = new();
    static readonly List<Vector3> normals = new();
    static readonly List<Vector3> v3Verts = new();
    private const float TangnetApproximation = 0.001f;
    public static Mesh GetRoadMesh(Road road)
    {
        int leftLength, rightLength;
        leftLength = road.LeftOutline.GetSize();
        rightLength = road.RightOutline.GetSize();
        Assert.IsTrue(leftLength == rightLength);
        Assert.AreEqual(RoadOutline.MidNumPoint + RoadOutline.EndsNumPoint * 2, leftLength, $"{leftLength}");
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
            uvs.Add(new(1, (midStart + i / (RoadOutline.MidNumPoint - 1) * (midEnd - midStart)) * numRepeat));
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

    public static Mesh GetPolygonMesh(SplineContainer splineContainer, float zoneResolution = 0.1f, float angleThreshold = 4f)
    {
        Assert.IsNotNull(splineContainer);
        Assert.AreEqual(1, splineContainer.Splines.Count);
        Spline spline = splineContainer.Splines.First();
        Mesh mesh = new();
        Polygon polygon = new();
        List<TriangleNet.Geometry.Vertex> vertices = new();
        List<Vector3> verts3D = new();
        int numPoints = (int)(spline.GetLength() * zoneResolution - 1);
        List<float> knotInterpolations = new();
        for (int i = 0; i < spline.Knots.Count(); i++)
            knotInterpolations.Add(SplineUtility.GetNormalizedInterpolation(spline, i, PathIndexUnit.Knot));
        int knotIndex = 0;
        float3 prevTangent = 0;
        for (int i = 0; i <= numPoints; i++)
        {
            float interpolation = (float)i / numPoints;
            if (knotIndex < spline.Knots.Count() && interpolation > knotInterpolations[knotIndex])
            {
                if (MyNumerics.AngleInDegrees(prevTangent, spline.EvaluateTangent(interpolation - TangnetApproximation)) > angleThreshold)
                {
                    BezierKnot knot = spline.Knots.ElementAt(knotIndex);
                    AddPoint(knot.Position);
                    vertices.Add(new(knot.Position.x, knot.Position.z));
                    prevTangent = spline.EvaluateTangent(interpolation + TangnetApproximation);
                }
                knotIndex++;
            }
            float3 tangent = spline.EvaluateTangent(interpolation);
            float3 pos = spline.EvaluatePosition(interpolation);
            float angleFromPrev = MyNumerics.AngleInDegrees(prevTangent, tangent);
            if (prevTangent.Equals(0) || angleFromPrev > angleThreshold)
            {
                AddPoint(pos);
                prevTangent = tangent;
            }
        }
        TriangleNet.Geometry.Vertex prev = null;
        foreach (TriangleNet.Geometry.Vertex vertex in vertices)
        {
            polygon.Add(vertex);
            if (prev != null)
                polygon.Add(new Segment(prev, vertex));
            else
                polygon.Add(new Segment(vertices.Last(), vertex));
            prev = vertex;
        }

        var options = new ConstraintOptions() { ConformingDelaunay = true, Convex = false };
        var qualityOptions = new QualityOptions { MinimumAngle = 10.0f };
        IMesh imesh = polygon.Triangulate(options, qualityOptions);
        var triangles = new List<int>();
        vertices = imesh.Vertices.ToList();
        foreach (var triangle in imesh.Triangles)
        {
            triangles.Add(vertices.IndexOf(triangle.GetVertex(2)));
            triangles.Add(vertices.IndexOf(triangle.GetVertex(1)));
            triangles.Add(vertices.IndexOf(triangle.GetVertex(0)));
        }
        foreach (TriangleNet.Geometry.Vertex vertex in imesh.Vertices)
            verts3D.Add(new((float)vertex.X, 0, (float)vertex.Y));

        mesh.SetVertices(verts3D);
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetNormals(Enumerable.Repeat(Vector3.up, verts3D.Count).ToArray());
        return mesh;

        void AddPoint(float3 pos)
        {
            vertices.Add(new(pos.x, pos.z));
        }
    }
}