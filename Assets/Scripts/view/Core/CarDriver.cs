using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class CarDriver : MonoBehaviour
{
    [SerializeField] CarHumbleObject carPrefab;
    private HashSet<CarHumbleObject> cars;
    void Awake()
    {
        Car.Drive += Drive;
        cars = new();
    }

    void Update()
    {
        foreach (CarHumbleObject carObject in cars)
        {
            Car car = carObject.Car;
            if (!car.IsTraveling && !car.SpawnBlocked())
                car.IsTraveling = true;
            if (!car.IsTraveling)
                continue;
            carObject.transform.position = car.Move(Time.deltaTime);
            if (car.ReachedDestination || car.DestinationUnreachable)
            {
                if (car.DestinationUnreachable)
                    car.ReturnDemand();
                Destroy(carObject.gameObject);
            }
        }
        cars.RemoveWhere((c) => c.Car.DestinationUnreachable || c.Car.ReachedDestination);
    }

    void OnDestroy()
    {
        Car.Drive -= Drive;
    }

    void Drive(Car car)
    { 
        CarHumbleObject carObject = Instantiate(carPrefab, transform);
        carObject.Car = car;
        cars.Add(carObject);
    }
}