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

    [Test]
    public void NoRoadNoPath()
    {
        Zone zone0 = new(0);
        Zone zone1 = new(1);
        Car car = new(zone0, zone1);
        Assert.False(car.CanFindPath());
    }

    [Test]
    public void NoValidPath()
    {
        Zone zone0 = new(0);
        Zone zone1 = new(1);
        Car car = new(zone0, zone1);
        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone0.AddOutRoad(road);
        Assert.False(car.CanFindPath());
    }

    [Test]
    public void SimplePathFindable()
    {
        Zone zone0 = new(0);
        Zone zone1 = new(1);
        Car car = new(zone0, zone1);
        Road road = RoadBuilder.B(0, stride, 2 * stride, 1);
        zone0.AddOutRoad(road);
        zone1.AddInRoad(road);
        Assert.True(car.CanFindPath());
    }
}