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
        List<float3> left = (GoreArea.Side == Side.Start ? GoreArea.Left.RightOutline.Start: GoreArea.Left.RightOutline.End).ToList();
        if (math.length(left[^1] - right[^1]) < Constants.LaneWidth * 0.15f)
            return;
        right.Reverse();
        
        verts.AddRange(left);
        verts.AddRange(right);
        // foreach (float3 pos in verts)
        //     DebugExtension.DebugPoint(pos, Color.green, 2, 100000);
        
    
        Mesh mesh = MeshUtil.GetPolygonMesh(verts, 2, transform);
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>();
    }
}