using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
using UnityEngine.Assertions;

public class Cars : MonoBehaviour
{
    [SerializeField] CarObject carPrefab;
    static Dictionary<uint, CarObject> carMapping;
    ObjectPool<CarObject> carPool;
    public static float TimeScale = 1;
    void Awake()
    {
        Game.CarAdded += CarAdded;
        Game.CarRemoved += RemoveCar;
        carMapping = new();
        CreatePool();
    }

    void Update()
    {
        if (!Game.BuildModeOn)
            CarControl.PassTime(Time.deltaTime * TimeScale);
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
            10000
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
    }

    public void DestoryAll()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
        carMapping.Clear();
        CreatePool();
    }
}