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
        for (float i = 0; i < road.LeftOutline.MidNumPoint; i++)
            uvs.Add(new(0, (midStart + i / (road.LeftOutline.MidNumPoint - 1) * (midEnd - midStart)) * numRepeat));
        for (float i = 0; i < RoadOutline.EndsNumPoint; i++)
            uvs.Add(new(0, (midEnd + i / (RoadOutline.EndsNumPoint - 1) * midStart) * numRepeat));

        for (float i = 0; i < RoadOutline.EndsNumPoint; i++)
            uvs.Add(new(1, i / (RoadOutline.EndsNumPoint - 1) * midStart * numRepeat));
        for (float i = 0; i < road.RightOutline.MidNumPoint; i++)
            uvs.Add(new(1, (midStart + i / (road.RightOutline.MidNumPoint - 1) * (midEnd - midStart)) * numRepeat));
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

    public static Mesh GetPolygonMesh(SplineContainer splineContainer, float resolution = 0.1f, float angleThreshold = 4f)
    {
        Assert.IsNotNull(splineContainer);
        Assert.AreEqual(1, splineContainer.Splines.Count);
        Spline spline = splineContainer.Splines.First();
        List<float3> verts3D = new();
        int numPoints = (int)(spline.GetLength() * resolution - 1);
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
                    verts3D.Add(knot.Position);
                    prevTangent = spline.EvaluateTangent(interpolation + TangnetApproximation);
                }
                knotIndex++;
            }
            float3 tangent = spline.EvaluateTangent(interpolation);
            float3 pos = spline.EvaluatePosition(interpolation);
            float angleFromPrev = MyNumerics.AngleInDegrees(prevTangent, tangent);
            if (prevTangent.Equals(0) || angleFromPrev > angleThreshold)
            {
                verts3D.Add(pos);
                prevTangent = tangent;
            }
        }
        return GetPolygonMesh(verts3D, 0);
    }

    public static Mesh GetPolygonMesh(List<float3> points, float newY, Transform transform)
    {
        return WorldToLocalSpace(GetPolygonMesh(points, newY), transform);
    }
    
    public static Mesh GetPolygonMesh(List<float3> points, float newY)
{
    // Initialize the mesh and polygon objects
    Mesh mesh = new();
    Polygon polygon = new();
    
    int pointCount = points.Count;
    List<TriangleNet.Geometry.Vertex> vertices = new(pointCount);

    // Convert points to TriangleNet vertices and add to the polygon
    for (int i = 0; i < pointCount; i++)
    {
        var pos = points[i];
        var vertex = new TriangleNet.Geometry.Vertex(pos.x, pos.z);
        vertices.Add(vertex);
        polygon.Add(vertex);

        // Add a segment to close the polygon loop
        if (i > 0)
            polygon.Add(new Segment(vertices[i - 1], vertex));
    }
    // Add the last segment to close the loop
    polygon.Add(new Segment(vertices[pointCount - 1], vertices[0]));

    // Define triangulation options
    var options = new ConstraintOptions() { ConformingDelaunay = true, Convex = false };
    var qualityOptions = new QualityOptions { MinimumAngle = 1.0f };

    // Perform triangulation
    IMesh imesh = polygon.Triangulate(options, qualityOptions);

    // Prepare triangles and vertices for the Unity mesh
    int vertexCount = imesh.Vertices.Count;
    int triangleCount = imesh.Triangles.Count;

    List<Vector3> verts3D = new(vertexCount);
    Dictionary<TriangleNet.Geometry.Vertex, int> vertexIndexMap = new(vertexCount);

    int index = 0;
    foreach (var vertex in imesh.Vertices)
    {
        verts3D.Add(new Vector3((float)vertex.X, newY, (float)vertex.Y));
        vertexIndexMap[vertex] = index++;
    }

    List<int> triangles = new(triangleCount * 3);
    foreach (var triangle in imesh.Triangles)
    {
        triangles.Add(vertexIndexMap[triangle.GetVertex(2)]);
        triangles.Add(vertexIndexMap[triangle.GetVertex(1)]);
        triangles.Add(vertexIndexMap[triangle.GetVertex(0)]);
    }

    // Set mesh data
    mesh.SetVertices(verts3D);
    mesh.SetTriangles(triangles, 0);
    mesh.SetNormals(Enumerable.Repeat(Vector3.up, verts3D.Count).ToArray());

    return mesh;
}


    public static Mesh WorldToLocalSpace(Mesh mesh, Transform transform)
    {
        Vector3[] local = new Vector3[mesh.vertices.Count()];
        for (int i = 0; i < mesh.vertexCount; i++)
            local[i] = transform.InverseTransformPoint(mesh.vertices[i]);
        mesh.vertices = local;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}