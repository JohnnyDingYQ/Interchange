using System;
using UnityEngine;

public class CarHumbleObject : MonoBehaviour
{
    public Car Car { get; set; }

    void Update()
    {
        gameObject.transform.position = Car.Pos;
    }
}