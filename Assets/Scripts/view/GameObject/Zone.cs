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

public class Zone : MonoBehaviour, IZone
{
    public int Id { get; set; }
    public string Name { get; set; }
    public SortedDictionary<int, int> Demands { get; set; }
    public HashSet<Road> InRoads { get; set; }
    public HashSet<Road> OutRoads { get; set; }

    // readonly SplineContainer splineContainer;
    // List<Mesh> convexMeshes;

    public void Init(SplineContainer splineContainer, int id)
    {
        Assert.AreEqual(1, splineContainer.Splines.Count());

        Id = id;
        InRoads = new();
        OutRoads = new();
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
            PlainShape poly = new(new NativeArray<IntVector>(verts2D.ToArray(), Allocator.Temp), true, Allocator.Temp);
            int[] tris = Triangulation.DelaunayTriangulate(poly, Allocator.Temp).ToArray();
            mesh.SetVertices(verts3D);
            mesh.SetTriangles(tris, 0);
            Debug.Log(tris.Count());
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }

    
    void OnMouseOver()
    {
        Game.HoveredZone = this;
        // Debug.Log(OutRoads.Count);
    }

    void OnMouseExit()
    {
        Game.HoveredZone = null;
    }
}