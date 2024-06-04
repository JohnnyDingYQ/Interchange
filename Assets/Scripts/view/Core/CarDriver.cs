using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // carObject.Car = car;
        foreach (Path path in car.paths)
        {
            while (path.IsBlocked)
                yield return null;
            
            Game.BlockPath(path);
            carObject.transform.position = path.BezierSeries.EvaluatePosition(0);
            float totalLength = path.BezierSeries.Length;
            float traveledLength = 0;
            while (traveledLength <= totalLength)
            {
                // Debug.Log("stepping");
                carObject.transform.position = path.BezierSeries.EvaluatePosition(traveledLength / totalLength);
                traveledLength += Constants.CarSpeed * Time.deltaTime;
                yield return null;
            }
        }
        Destroy(carObject.gameObject);
    }
}