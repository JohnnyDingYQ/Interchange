using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class CarControl
{
    static readonly HashSet<Car> toRemove = new();
    static bool IsOnValidEdge(Car car)
    {
        return car.CurrentEdge != null;
    }

    public static void PassTime(float deltaTime)
    {
        foreach (Car car in Game.Cars.Values)
        {
            if (car.IsDone)
            {
                Game.CarServiced++;
                toRemove.Add(car);
                continue;
            }

            if (IsOnValidEdge(car))
            {
                car.Move(deltaTime);
            }
            else
            {
                car.Cancel();
                Game.CarServiced++;
                toRemove.Add(car);
                continue;
            }
            
        }
        foreach (Car car in toRemove)
            Game.RemoveCar(car);
        toRemove.Clear();
    }
}