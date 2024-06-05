using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Path[] arrayPaths = car.paths.ToArray();
        while (arrayPaths.First().EntranceOccupied())
            yield return null;
        for (int i = 0; i < arrayPaths.Count(); i++)
        {
            Path path = arrayPaths[i];
            Path nextPath = i + 1 < arrayPaths.Count() ? arrayPaths[i + 1] : null;
            path.AddCar(car);
            float totalLength = path.Length;
            float traveledLength = 0;
            while (traveledLength <= totalLength && traveledLength >= 0)
            {
                carObject.transform.position = path.BezierSeries.EvaluatePosition(traveledLength / totalLength);
                traveledLength = path.MoveCar(car, Time.deltaTime, nextPath);
                yield return null;
            }
            // if (nextPath != null)
            // {
            //     if (nextPath.IncomingCar != car)
            //         Debug.Log(nextPath.IncomingCar);
            //     nextPath.IncomingCar = null;
            // }
            // path.PopCar();
        }
        Destroy(carObject.gameObject);
    }
}