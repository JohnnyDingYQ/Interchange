using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Assets.Scripts.Model.Roads;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Linq;
using KaimiraGames;

public class Cars : MonoBehaviour
{
    [SerializeField] CarObject carPrefab;
    Dictionary<uint, CarObject> carMapping;
    ObjectPool<CarObject> carPool;
    [SerializeField] CameraSettings cameraSettings;
    [SerializeField] Roads roads;
    public static float TimeScale = 1;
    List<Vertex> verticesInSight;
    List<Car> toRemove;

    void Awake()
    {
        Game.CarAdded += CarAdded;
        Game.CarRemoved += RemoveCar;
        CreatePool();
        carMapping = new();
        verticesInSight = new();
        toRemove = new();
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