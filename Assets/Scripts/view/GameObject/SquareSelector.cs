using System.Collections.Generic;
using Codice.CM.SEIDInfo;
using Unity.Mathematics;
using UnityEngine;

public class SquareSelector : MonoBehaviour
{
    Mesh square;
    void Start()
    {
        List<Vector3> verts = new() {
            new(-0.5f, 0, 0.5f), new(0.5f, 0, 0.5f),
            new(-0.5f, 0, -0.5f), new(0.5f, 0 ,-0.5f),
            // new(-0.5f, 0, 0.5f), new(0.5f, 0, 0.5f),
            // new(-0.5f, 0, -0.5f), new(0.5f, 0 ,-0.5f)
        };
        List<Vector3> normals = new() {
            Vector3.up, Vector3.up, Vector3.up, Vector3.up,
            // Vector3.Normalize(new(-1, 0, 1)), Vector3.Normalize(new(1, 0, 1)), Vector3.Normalize(new(-1, 0, -1)), Vector3.Normalize(new(1, 0, -1))
        };
        List<Vector2> uvs = new() {
            new(-1, 1), new(1, 1),
            new(-1, -1), new(1, -1)
        };
        List<int> tris = new() { 0, 1, 2, 2, 1, 3 };
        square = new();
        square.SetVertices(verts);
        square.SetNormals(normals);
        square.SetTriangles(tris, 0);
        square.SetUVs(0, uvs);
        gameObject.GetComponent<MeshFilter>().sharedMesh = square;
    }

    public void SetTransform(float widthScale, float heightScale, float3 center)
    {
        gameObject.transform.localScale = new(widthScale, 1, heightScale);
        center.y = Constants.MaxElevation + 1;
        gameObject.transform.position = center;
    }
}