using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine;


public class Car : IPersistable
{
    public uint Id { get; set; }
    public bool IsDone { get; private set; }
    public float3 Pos { get; private set; }
    [NotSaved]
    public float DistanceOnEdge { get => path.DistanceOnEdge; }
    public float Speed { get; set; }
    [SaveID]
    public Path path;

    public Car(Path path)
    {
        this.path = path;
        Speed = 0;
        Pos = new(0, -100, 0);
    }

    public void Move(float deltaTime)
    {
        path.Move(this, deltaTime);
        Pos = path.Pos;
    }

    public void Cancel()
    {
        IsDone = true;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}