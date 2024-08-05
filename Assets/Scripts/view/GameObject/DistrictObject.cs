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
    private const float ZoneResolution = 0.15f;
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
            List<Vector3> verts3D = new();
            float length = spline.GetLength();
            int numPoints = (int)(length * ZoneResolution - 1);
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
                    if (GetAngle(prevTangent, spline.EvaluateTangent(interpolation - TangentApproximation)) > AngleThreshold)
                    {
                        BezierKnot knot = spline.Knots.ElementAt(knotIndex);
                        AddPoint(knot.Position);
                        prevTangent = spline.EvaluateTangent(interpolation + TangentApproximation);
                    }
                    knotIndex++;
                }

                float3 tangent = spline.EvaluateTangent(interpolation);
                float3 pos = spline.EvaluatePosition(interpolation);
                float angleFromPrev = GetAngle(prevTangent, tangent);
                if (prevTangent.Equals(0) || angleFromPrev > AngleThreshold)
                {
                    AddPoint(pos);
                    prevTangent = tangent;
                }
            }
            // foreach (Vector3 pos in verts3D)
            //     DebugExtension.DebugPoint(pos, Color.black, 15, 10000);
            // return;
            ;
            var options = new ConstraintOptions() { ConformingDelaunay = true, Convex = false };
            var qualityOptions = new QualityOptions { MinimumAngle = 20.0f };
            // GenericMesher genericMesher = new(new SweepLine());
            IMesh imesh = polygon.Triangulate(options, qualityOptions);
            // IMesh imesh = genericMesher.Triangulate(polygon, options);
            var triangles = new List<int>();
            List<TriangleNet.Geometry.Vertex> vertices = imesh.Vertices.ToList();
            foreach (var triangle in imesh.Triangles)
            {
                // Get vertex indices for each triangle
                int index0 = vertices.IndexOf(triangle.GetVertex(0));
                int index1 = vertices.IndexOf(triangle.GetVertex(1));
                int index2 = vertices.IndexOf(triangle.GetVertex(2));

                triangles.Add(index2);
                triangles.Add(index1);
                triangles.Add(index0);
            }
            Debug.Log(imesh.Vertices.Count);
            Debug.Log(verts3D.Count);
            foreach (TriangleNet.Geometry.Vertex vertex in imesh.Vertices)
            {
                verts3D.Add(new((float)vertex.X, 0, (float)vertex.Y));
            }
            mesh.SetVertices(verts3D);
            mesh.SetTriangles(triangles.ToArray(), 0);
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            gameObject.AddComponent<MeshRenderer>();

            void AddPoint(float3 pos)
            {
                // DebugExtension.DebugPoint(pos, Color.cyan, 5, 10000);
                polygon.Add(new(pos.x, pos.z));
                // polygon.Add(new Segment())
            }
        }
    }

    float GetAngle(float3 a, float3 b)
    {
        float dot = math.dot(a, b) / (math.length(a) * math.length(b));
        return MathF.Acos(dot) * 180 / MathF.PI;
    }
}