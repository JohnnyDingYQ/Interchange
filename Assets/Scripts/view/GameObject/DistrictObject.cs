using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Meshing.Algorithm;


public class DistrictObject : MonoBehaviour
{
    private const int DecimalAccuracy = 100;
    private const float AngleThreshold = 4f;
    private const float ZoneResolution = 0.1f;
    private const float TangentApproximation = 0.001f;
    public District District { get; set; }

    public void Init(SplineContainer splineContainer)
    {
        Assert.AreEqual(1, splineContainer.Splines.Count());

        SetupMeshCollider();

        void SetupMeshCollider()
        {
            Spline spline = splineContainer.Splines.First();
            Mesh mesh = new();
            Polygon polygon = new();
            List<TriangleNet.Geometry.Vertex> vertices = new();
            List<Vector3> verts3D = new();
            int numPoints = (int)(spline.GetLength() * ZoneResolution - 1);
            float3 prevTangent = 0;
            for (int i = 0; i <= numPoints; i++)
            {
                float interpolation = (float)i / numPoints;
                float3 tangent = spline.EvaluateTangent(interpolation);
                float3 pos = spline.EvaluatePosition(interpolation);
                float angleFromPrev = GetAngle(prevTangent, tangent);
                if (prevTangent.Equals(0) || angleFromPrev > AngleThreshold)
                {
                    vertices.Add(new(pos.x, pos.z));
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
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            // gameObject.AddComponent<MeshRenderer>();
        }
    }

    float GetAngle(float3 a, float3 b)
    {
        float dot = math.dot(a, b) / (math.length(a) * math.length(b));
        return MathF.Acos(dot) * 180 / MathF.PI;
    }
}