using System;
using UnityEngine;

public class CarObject : MonoBehaviour
{
    public Car Car { get; set; }

    void Update()
    {
        if (Car.CurrentEdge != null)
            gameObject.transform.position = Car.Pos;
    }
}