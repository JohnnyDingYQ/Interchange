using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using iShape.Geometry;
using iShape.Triangulation.Shape.Delaunay;
using iShape.Geometry.Container;
using Unity.Collections;
using UnityEngine.Assertions;
using QuikGraph.Collections;
using System;

public class ZoneHumbleObject : MonoBehaviour
{
    private const int DecimalAccuracy = 10;
    private const float AngleThreshold = 4f;
    private const float ZoneResolution = 0.17f;
    private const float TangnetApproximation = 0.001f;
    public Zone Zone { get; private set; }

    public void Init(uint id, SplineContainer splineContainer)
    {
        Assert.AreEqual(1, splineContainer.Splines.Count());

        Zone = new Zone(id);
        Game.Zones[id] = Zone;
        SetupMeshCollider();

        void SetupMeshCollider()
        {
            Spline spline = splineContainer.Splines.First();
            Mesh mesh = new();
            List<IntVector> verts2D = new();
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
                    if (GetAngle(prevTangent, spline.EvaluateTangent(interpolation - TangnetApproximation)) > AngleThreshold)
                    {
                        BezierKnot knot = spline.Knots.ElementAt(knotIndex);
                        AddPoint(knot.Position);
                        prevTangent = spline.EvaluateTangent(interpolation + TangnetApproximation);
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
            // return;
            NativeArray<IntVector> nativeArray = new(verts2D.ToArray(), Allocator.Temp);
            PlainShape poly = new(nativeArray, true, Allocator.Temp);
            NativeArray<int> tris = Triangulation.DelaunayTriangulate(poly, Allocator.Temp);
            mesh.SetVertices(verts3D);
            mesh.SetTriangles(tris.ToArray(), 0);
            nativeArray.Dispose();
            tris.Dispose();
            poly.Dispose();
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            // gameObject.AddComponent<MeshRenderer>();

            void AddPoint(float3 pos)
            {
                // DebugExtension.DebugPoint(pos, Color.cyan, 5, 10000);
                verts2D.Add(new((long)(pos.x * DecimalAccuracy), (long)(pos.z * DecimalAccuracy)));
                verts3D.Add(new Vector3(pos.x, Constants.ZoneHeight, pos.z));
            }
        }
    }

    float GetAngle(float3 a, float3 b)
    {
        float dot = math.dot(a, b) / (math.length(a) * math.length(b));
        return MathF.Acos(dot) * 180 / MathF.PI;
    }
}