using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
using UnityEngine.Assertions;

public class CarDriver : MonoBehaviour
{
    [SerializeField] CarHumbleObject carPrefab;
    HashSet<CarHumbleObject> cars;
    ObjectPool<CarHumbleObject> carPool;
    HashSet<CarHumbleObject> toRemove;
    void Awake()
    {
        Car.Drive += Drive;
        cars = new();
        toRemove = new();
        carPool = new(
            () => Instantiate(carPrefab, transform),
            (o) => o.gameObject.SetActive(true),
            (o) => o.gameObject.SetActive(false),
            (o) => Destroy(o.gameObject),
            false,
            200,
            1000
        );
    }

    void Update()
    {
        foreach (CarHumbleObject carObject in cars)
        {
            Car car = carObject.Car;
            if (!car.IsTraveling && !car.SpawnBlocked())
                car.Start();
            if (!car.IsTraveling)
                continue;
            carObject.transform.position = car.Move(Time.deltaTime);
            if (car.ReachedDestination || car.DestinationUnreachable)
            {
                if (car.DestinationUnreachable)
                    car.Stop();
                if (car.ReachedDestination)
                    DevPanel.CarServiced.text = "Serviced: " + Game.CarServiced++;
                carPool.Release(carObject);
                toRemove.Add(carObject);
            }
        }
        cars.ExceptWith(toRemove);
        toRemove.Clear();

    }

    void OnDestroy()
    {
        Car.Drive -= Drive;
    }

    void Drive(Car car)
    {
        CarHumbleObject carObject = carPool.Get();
        carObject.Car = car;
        cars.Add(carObject);
    }
}