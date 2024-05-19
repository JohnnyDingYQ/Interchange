using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class Zone : MonoBehaviour
{
    readonly SplineContainer splineContainer;
    List<Mesh> convexMeshes;

    public void Init(SplineContainer splineContainer)
    {
        convexMeshes = new();
        foreach (Spline spline in splineContainer.Splines)
        {
            Mesh mesh = new();
            float3 midpoint = new(0);
            foreach (BezierKnot knot in spline.Knots)
                midpoint += knot.Position;
            midpoint /= spline.Knots.Count();
            List<Vector3> verts = new();
            List<int> tris = new();
            verts.Add(new Vector3(midpoint.x, midpoint.y, midpoint.z));
            float length = spline.GetLength();
            int numPoints = (int)(length * Constants.ZoneResolution - 1);
            for (int i = 0; i <= numPoints; i++)
            {
                float3 pos = spline.EvaluatePosition((float)i / numPoints);
                verts.Add(new Vector3(pos.x, pos.y, pos.z));
            }
            for (int i = 1; i < verts.Count - 1; i++)
                tris.AddRange(new int[] { 0, i + 1, i });
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            Debug.Log(tris.Count);
            convexMeshes.Add(mesh);
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            // meshCollider.convex = true;
            // MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            // meshFilter.mesh = mesh;
            // MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }

    void OnMouseEnter()
    {
        Debug.Log("Hi");
    }

    void OnMouseExit()
    {
        Debug.Log("Exit");
    }
}