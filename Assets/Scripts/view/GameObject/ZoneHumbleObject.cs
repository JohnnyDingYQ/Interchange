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
    public Zone zone { get; private set; }

    public void Init(int id, SplineContainer splineContainer)
    {
        Assert.AreEqual(1, splineContainer.Splines.Count());

        zone = new Zone(id);
        Game.Zones[id] = zone;
        SetupMeshCollider();

        void SetupMeshCollider()
        {
            Spline spline = splineContainer.Splines.First(); // closed spline
            Mesh mesh = new();
            List<IntVector> verts2D = new();
            List<Vector3> verts3D = new();
            float length = spline.GetLength();
            int numPoints = (int)(length * Constants.ZoneResolution - 1);
            for (int i = 0; i <= numPoints; i++)
            {
                float3 pos = spline.EvaluatePosition((float)i / numPoints);
                verts2D.Add(new((long)pos.x, (long)pos.z));
                verts3D.Add(new Vector3(pos.x, 32, pos.z)); // MAGIC NUMBER due to height of terrain
            }
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
            gameObject.AddComponent<MeshRenderer>();
        }
    }


}