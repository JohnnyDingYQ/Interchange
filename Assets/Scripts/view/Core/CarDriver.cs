using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
using UnityEngine.Assertions;

public class CarDriver : MonoBehaviour
{
    [SerializeField] CarHumbleObject carPrefab;
    static Dictionary<uint, CarHumbleObject> carMapping;
    ObjectPool<CarHumbleObject> carPool;
    public static float TimeScale = 1;
    void Awake()
    {
        Game.CarAdded += CarAdded;
        Game.CarRemoved += RemoveCar;
        carMapping = new();
        carPool = new(
            () => Instantiate(carPrefab, transform),
            (o) => { o.gameObject.SetActive(true); o.gameObject.transform.position = new(0, -100, 0); },
            (o) => o.gameObject.SetActive(false),
            (o) => Destroy(o.gameObject),
            false,
            200,
            1000
        );
    }

    void Update()
    {
        CarControl.PassTime(Time.deltaTime * TimeScale);
    }

    void OnDestroy()
    {
        Game.CarAdded -= CarAdded;
        Game.CarRemoved -= RemoveCar;
    }

    void CarAdded(Car car)
    {
        CarHumbleObject carObject = carPool.Get();
        carObject.Car = car;
        carMapping[car.Id] = carObject;
        carObject.gameObject.layer = LayerMask.NameToLayer("Cars");
    }

    void RemoveCar(Car car)
    {
        carPool.Release(carMapping[car.Id]);
    }
}