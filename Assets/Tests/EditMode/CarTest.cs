using NUnit.Framework;
using Unity.Mathematics;

public class CarTest
{
    float3 stride = Constants.MinimumLaneLength * new float3(1, 0, 0);
    
    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }
}