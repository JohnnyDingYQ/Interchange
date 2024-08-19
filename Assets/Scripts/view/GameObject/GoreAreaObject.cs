using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class GoreAreaObject : MonoBehaviour
{
    public GoreArea GoreArea;

    public void Init()
    {
        
        List<float3> verts = new();
        List<float3> right = (GoreArea.Side == Side.Start ? GoreArea.Right.LeftOutline.Start: GoreArea.Right.LeftOutline.End).ToList();
        right.Reverse();
        verts.AddRange(GoreArea.Side == Side.Start ? GoreArea.Left.RightOutline.Start: GoreArea.Left.RightOutline.End);
        verts.AddRange(right);
        
    
        Mesh mesh = MeshUtil.GetPolygonMesh(verts, 2);
        mesh = MeshUtil.WorldToLocalSpace(mesh, transform);
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>();
    }
}