using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class CarControl
{
    static readonly HashSet<Car> toRemove = new();
    static bool IsOnValidEdge(Car car)
    {
        return Game.Edges.ContainsKey(car.CurrentEdge.Id);
    }

    public static void PassTime(float deltaTime)
    {
        foreach (Car car in Game.Cars.Values)
        {
            if (!car.IsTraveling && !car.SpawnBlocked())
                car.Start();
            if (!car.IsTraveling)
                continue;
            if (IsOnValidEdge(car))
                car.Move(deltaTime);
            else
                car.Cancel();
            if (car.IsDone)
            {
                Game.CarServiced++;
                toRemove.Add(car);
            }
        }
        foreach (Car car in toRemove)
            Game.RemoveCar(car);
        toRemove.Clear();
    }
}