using System.Collections.Generic;
using Codice.CM.SEIDInfo;
using Unity.Mathematics;
using UnityEngine;

public class SquareSelector : MonoBehaviour
{
    public bool Performed;
    public float3 StartPos;
    Mesh square;
    List<Vector3> unitSquareVerts;
    List<Vector3> updatedVerts;
    void Start()
    {
        updatedVerts = new();
        unitSquareVerts = new() {
            new(-0.5f, 0, 0.5f), new(0.5f, 0, 0.5f),
            new(-0.5f, 0, -0.5f), new(0.5f, 0 ,-0.5f),
        };
        List<Vector3> normals = new() {
            Vector3.up, Vector3.up, Vector3.up, Vector3.up,
        };
        List<Vector2> uvs = new() {
            new(-1, 1), new(1, 1),
            new(-1, -1), new(1, -1)
        };
        List<int> tris = new() { 0, 1, 2, 2, 1, 3 };
        square = new();
        square.SetVertices(unitSquareVerts);
        square.SetNormals(normals);
        square.SetTriangles(tris, 0);
        square.SetUVs(0, uvs);
        gameObject.GetComponent<MeshFilter>().sharedMesh = square;
    }

    public void SetTransform(float widthScale, float heightScale, float3 center)
    {
        updatedVerts.Clear();
        foreach (Vector3 vector in unitSquareVerts)
        {
            Vector3 copy = vector;
            copy.x *= widthScale;
            copy.z *= heightScale;
            updatedVerts.Add(copy);
        }
        square.SetVertices(updatedVerts);
        center.y = Constants.MaxElevation + 1;
        gameObject.transform.SetPositionAndRotation(center, Quaternion.identity);
    }
}