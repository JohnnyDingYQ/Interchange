using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Assets.Scripts.Model.Roads;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

public class Cars : MonoBehaviour
{
    [SerializeField] CarObject carPrefab;
    static Dictionary<uint, CarObject> carMapping;
    ObjectPool<CarObject> carPool;
    public static float TimeScale = 1;

    JobHandle jobHandle;
    NativeArray<float3> results;
    NativeArray<CurveData> curveData;
    NativeArray<float> distanceOnCurves;
    readonly int initialCapacity = 2000;
    void Awake()
    {
        Game.CarAdded += CarAdded;
        Game.CarRemoved += RemoveCar;
        carMapping = new();
        CreatePool();

        curveData = new(initialCapacity, Allocator.Persistent);
        distanceOnCurves = new(initialCapacity, Allocator.Persistent);
        results = new(initialCapacity, Allocator.Persistent);
    }

    void Update()
    {
        if (!Game.BuildModeOn)
            CarControl.PassTime(Time.deltaTime * TimeScale);
        if (carMapping.Count > curveData.Length)
        {
            int newSize = curveData.Length * 2;
            curveData.Dispose();
            distanceOnCurves.Dispose();
            results.Dispose();
            curveData = new(newSize, Allocator.Persistent);
            distanceOnCurves = new(newSize, Allocator.Persistent);
            results = new(newSize, Allocator.Persistent);
        }
        int index = 0;
        foreach (CarObject carObject in carMapping.Values)
        {
            distanceOnCurves[index] = carObject.Car.distanceOnEdge;
            curveData[index++] = carObject.Car.CurrentEdge.Curve.GetCurveData();
        }
        jobHandle = new EvaluatePositionJob()
        {
            curveData = curveData,
            distanceOnCurve = distanceOnCurves,
            results = results
        }.Schedule(carMapping.Count, 64);
    }

    void LateUpdate()
    {
        jobHandle.Complete();
        int index = 0;
        foreach (CarObject carObject in carMapping.Values)
        {
            carObject.transform.position = results[index++];
        }
    }

    [BurstCompile]
    struct EvaluatePositionJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<CurveData> curveData;
        [ReadOnly]
        public NativeArray<float> distanceOnCurve;
        [WriteOnly]
        public NativeArray<float3> results;
        public void Execute(int index)
        {
            results[index] = EvaluatePostion(curveData[index], distanceOnCurve[index]);
        }

        struct DistanceToInterpolationPair
        {
            public float distance;
            public float interpolation;
        }

        readonly float3 EvaluatePostion(CurveData curveData, float distance)
        {
            float t = DistanceToInterpolation(curveData, distance);
            return EvaluatePositionT(curveData, t) + EvaluateNormalT(curveData, t) * curveData.offsetDistance;
        }

        readonly float DistanceToInterpolation(CurveData curveData, float distance)
        {
            DistanceToInterpolationPair low = new() { distance = 0, interpolation = 0 };
            DistanceToInterpolationPair high = new() { distance = curveData.length, interpolation = 1 };

            float midT = math.lerp(low.interpolation, high.interpolation, 0.5f);
            float midDistance = low.distance + math.length(EvaluatePositionT(curveData, midT) - EvaluatePositionT(curveData, low.interpolation));
            DistanceToInterpolationPair mid = new() { distance = midDistance, interpolation = midT };

            int maxIterations = 20;
            while (math.abs(mid.distance - distance) > 0.0001f && maxIterations-- > 0)
            {
                if (mid.distance < distance)
                    low = mid;
                else
                    high = mid;

                mid.interpolation = math.lerp(low.interpolation, high.interpolation, 0.5f);
                mid.distance = low.distance + math.length(EvaluatePositionT(curveData, mid.interpolation) - EvaluatePositionT(curveData, low.interpolation));
            }

            return mid.interpolation;
        }

        readonly float3 EvaluatePositionT(CurveData curveData, float t)
        {
            return curveData.P0 * math.pow(1 - t, 3)
                   + 3 * math.pow(1 - t, 2) * t * curveData.P1
                   + (1 - t) * 3 * math.pow(t, 2) * curveData.P2
                   + math.pow(t, 3) * curveData.P3;
        }

        readonly float3 EvaluateTangentT(CurveData curveData, float t)
        {
            return 3 * math.pow(1 - t, 2) * (curveData.P1 - curveData.P0) + 6 * (1 - t) * t * (curveData.P2 - curveData.P1) + 3 * math.pow(t, 2) * (curveData.P3 - curveData.P2);
        }

        readonly float3 EvaluateNormalT(CurveData curveData, float t)
        {
            float3 tangent = EvaluateTangentT(curveData, t);
            float3 normal = new(-tangent.z, 0, tangent.x);
            return math.normalizesafe(normal);
        }
    }

    void OnDestroy()
    {
        Game.CarAdded -= CarAdded;
        Game.CarRemoved -= RemoveCar;
    }

    void CreatePool()
    {
        carPool = new(
            () => Instantiate(carPrefab, transform),
            (o) => { o.gameObject.SetActive(true); o.gameObject.transform.position = new(0, -100, 0); },
            (o) => o.gameObject.SetActive(false),
            (o) => Destroy(o.gameObject),
            false,
            200,
            Constants.MaxCarCount
        );
    }

    void CarAdded(Car car)
    {
        CarObject carObject = carPool.Get();
        carObject.Car = car;
        carMapping[car.Id] = carObject;
        carObject.gameObject.layer = LayerMask.NameToLayer("Cars");
    }

    void RemoveCar(Car car)
    {
        carPool.Release(carMapping[car.Id]);
        carMapping.Remove(car.Id);
    }

    public void DestoryAll()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
        carMapping.Clear();
        CreatePool();
    }
}