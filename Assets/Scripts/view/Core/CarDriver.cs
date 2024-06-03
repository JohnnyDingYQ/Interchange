using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDriver: MonoBehaviour
{
    [SerializeField] Car carGameObject;
    public void Awake()
    {
        DemandsSatisfer.Drive += Drive;
    }

    public void OnDestroy()
    {
        DemandsSatisfer.Drive -= Drive;
    }

    public void Drive(IEnumerable<Path> paths)
    {
        StartCoroutine(DriveStepper(paths));
    }

    IEnumerator DriveStepper(IEnumerable<Path> paths)
    {
        Car car = Instantiate(carGameObject, transform);
        foreach (Path path in paths)
        {
            car.transform.position = path.BezierSeries.EvaluatePosition(0);
            float totalLength = path.BezierSeries.Length;
            float traveledLength = 0;
            while (traveledLength <= totalLength)
            {
                // Debug.Log("stepping");
                car.transform.position = path.BezierSeries.EvaluatePosition(traveledLength/totalLength);
                traveledLength += 15f * Time.deltaTime;
                yield return null;
            }
        }
    }
}