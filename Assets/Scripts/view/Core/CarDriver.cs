using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class CarDriver : MonoBehaviour
{
    [SerializeField] CarHumbleObject carGameObject;
    public void Awake()
    {
        Car.TravelCoroutine += Drive;
    }

    public void OnDestroy()
    {
        Car.TravelCoroutine -= Drive;
    }

    public void Drive(Car car)
    {
        StartCoroutine(DriveStepper(car));
    }

    IEnumerator DriveStepper(Car car)
    {
        CarHumbleObject carObject = Instantiate(carGameObject, transform);
        float3 pos;
        while (car.SpawnBlocked())
            yield return null;
        while (!car.ReachedDestination)
        {
            pos = car.Move(Time.deltaTime);
            carObject.transform.position = pos;
            yield return null;
        }
        Destroy(carObject.gameObject);
    }
}