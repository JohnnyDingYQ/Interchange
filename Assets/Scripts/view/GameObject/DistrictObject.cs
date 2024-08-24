using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;


public class DistrictObject : MonoBehaviour
{
    public float3 Center { get; private set; }
    public District District { get; set; }
    DistrictLabel districtLabel;

    const int CenterSampleCount = 20;

    public void Update()
    {
        districtLabel?.Set(District.Connectedness);
        districtLabel?.ApplyWorldPos(Center);
    }

    public void Init(DistrictLabel districtLabel)
    {
        SplineContainer splineContainer = GetComponent<SplineContainer>();
        Assert.IsNotNull(splineContainer);
        Assert.AreEqual(1, splineContainer.Splines.Count());

        this.districtLabel = districtLabel;
        SetupMesh();
        CalculateCenter();

        void SetupMesh()
        {
            Mesh mesh = MeshUtil.GetPolygonMesh(splineContainer);
            
            MeshCollider meshCollider = splineContainer.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            MeshFilter meshFilter = splineContainer.gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            // splineContainer.gameObject.AddComponent<MeshRenderer>();
        }

        void CalculateCenter()
        {
            Center = 0;
            Spline spline = splineContainer.Splines.Single();
            for (float i = 0; i < CenterSampleCount; i++)
                Center += spline.EvaluatePosition(i / CenterSampleCount);
            
            Center /= CenterSampleCount;
            Center = transform.TransformPoint(Center);
        }
    }
}