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

public class Zone : MonoBehaviour, IZone
{
    public int Id { get; set; }
    public string Name { get; set; }
    public SortedDictionary<int, int> Demands { get; set; }
    // public HashSet<Road> InRoads { get; set; }
    // public HashSet<Road> OutRoads { get; set; }
    private List<Vertex> InVertices { get; set; }
    private List<Vertex> OutVertices { get; set; }
    public int InVerticesCount { get { return InVertices.Count; } }
    public int OutVerticesCount { get { return OutVertices.Count; } }
    public void AddInRoad(Road road)
    {
        foreach (Lane lane in road.Lanes)
            InVertices.Add(lane.EndVertex);
    }

    public void AddOutRoad(Road road)
    {
        foreach (Lane lane in road.Lanes)
            OutVertices.Add(lane.StartVertex);
    }

    public Vertex GetRandomInVertex()
    {
        int randomIndex = (int) (UnityEngine.Random.value * InVertices.Count());
        if (randomIndex == InVertices.Count) // technically possible
            return GetRandomInVertex();
        return InVertices[randomIndex];
    }

    public Vertex GetRandomOutVertex()
    {
        int randomIndex = (int) (UnityEngine.Random.value * OutVertices.Count());
        if (randomIndex == OutVertices.Count)
            return GetRandomOutVertex();
        return OutVertices[randomIndex];
    }

    public void Init(SplineContainer splineContainer, int id)
    {
        Assert.AreEqual(1, splineContainer.Splines.Count());

        Id = id;
        // InRoads = new();
        // OutRoads = new();
        InVertices = new();
        OutVertices = new();
        Demands = new();
        Game.Zones[id] = this;
        SetupMeshCollider();

        void SetupMeshCollider()
        {
            Spline spline = splineContainer.Splines.First();
            Mesh mesh = new();
            List<IntVector> verts2D = new();
            List<Vector3> verts3D = new();
            float length = spline.GetLength();
            int numPoints = (int)(length * Constants.ZoneResolution - 1);
            for (int i = 0; i <= numPoints; i++)
            {
                float3 pos = spline.EvaluatePosition((float)i / numPoints);
                verts2D.Add(new((long)pos.x, (long)pos.z));
                verts3D.Add(new Vector3(pos.x, 32, pos.z)); // MAGIC NUMBER HERE
            }
            NativeArray<IntVector> nativeArray = new(verts2D.ToArray(), Allocator.Temp);
            PlainShape poly = new(nativeArray, true, Allocator.Temp);
            int[] tris = Triangulation.DelaunayTriangulate(poly, Allocator.Temp).ToArray();
            mesh.SetVertices(verts3D);
            mesh.SetTriangles(tris, 0);
            nativeArray.Dispose();
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }

    public void RemoveRoad(Road road)
    {
        throw new System.NotImplementedException();
    }
}