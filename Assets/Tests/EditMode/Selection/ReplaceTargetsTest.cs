using NUnit.Framework;
using Unity.Mathematics;

public class ReplaceTargetsTest
{

    float3 direction = GConsts.MinimumRoadLength * new float3(1, 0, 0);

    [SetUp]
    public void SetUp()
    {
        Game.WipeState();
    }

    [Test]
    public void NoRoadGivesEmptySnap()
    {
        ReplaceTargets rt = new(0, 1, Game.Roads.Values);
        Assert.False(rt.SnapNotNull);
    }
}