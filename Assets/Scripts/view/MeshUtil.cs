using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MeshUtil
{
    public static Mesh GetMesh(Road road)
    {
        int length;
        if ((length = road.LeftOutline.GetSize()) != road.RightOutline.GetSize())
            throw new InvalidOperationException("Left and right outline does not have same size");
        List<float3> leftPts = road.LeftOutline.GetConcatenated();
        List<float3> rightPts = road.RightOutline.GetConcatenated();
        return null;
    }
}