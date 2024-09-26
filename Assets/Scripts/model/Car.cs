using Unity.Mathematics;
using Assets.Scripts.Model.Roads;
using System;


public class Car
{
    public uint Id { get; set; }
    readonly Path path;
    public float3 Pos { get; private set; }
    public CarStatus Status;
    float speed;
    public float TimeTaken;
    int edgeIndex;
    public float DistanceOnEdge { get; private set; }

    public Car(Path path)
    {
        this.path = path;
        DistanceOnEdge = UnityEngine.Random.value * path.Edges[0].Length * 0.8f;
    }

    public void Move(float deltaTime)
    {
        if (Status != CarStatus.Traveling)
            return;

        TimeTaken += deltaTime;
        Edge edge = path.Edges[edgeIndex];
        Edge nextEdge = edgeIndex < path.Edges.Count - 1 ? path.Edges[edgeIndex + 1] : null;

        speed = Constants.CarMaxSpeed;

        DistanceOnEdge += deltaTime * speed;

        if (DistanceOnEdge > edge.Length)
        {
            if (nextEdge != null)
            {
                DistanceOnEdge = 0;
                nextEdge.IncomingCar = null;
                edgeIndex++;
            }
            else
                Finish();
            return;
        }

        Pos = edge.Curve.EvaluatePosition(DistanceOnEdge);

    }

    void Finish()
    {
        Status = CarStatus.Finished;
    }
}