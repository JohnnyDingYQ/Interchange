using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;


public class DistrictObject : MonoBehaviour
{
    
    public District District { get; set; }

    public void Init(SplineContainer splineContainer)
    {
        Assert.AreEqual(1, splineContainer.Splines.Count());

        SetupMeshCollider();

        void SetupMeshCollider()
        {
            Mesh mesh = MeshUtil.GetPolygonMesh(splineContainer);
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            // gameObject.AddComponent<MeshRenderer>();
        }
    }
}