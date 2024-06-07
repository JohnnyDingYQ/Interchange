using Unity.Mathematics;

public static class CarControl
{
    public static void CheckPathValid(Car car)
    {
        if (!Game.Graph.ContainsEdge(car.CurrentPath))
           car.DestinationUnreachable = true;
    }
}