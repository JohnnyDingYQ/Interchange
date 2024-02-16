using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class DelimiterTest
{

    Delimiter delimiter;
    float3 left;
    float3 right;
    float3 up;
    
    [OneTimeSetUp]
    public void SetUp()
    {
        left = new(1, 0, 0);
        right = new(-1, 0, 0);
        up = new(0, 1, 0);
        delimiter = new(left, right, up);
    }
    // A Test behaves as an ordinary method
    [Test]
    public void ConstructorSetsProperties()
    {
        Assert.AreEqual(left, delimiter.LeftBound);
        Assert.AreEqual(right, delimiter.RightBound);
    }
    [Test]
    public void DefinesPlane()
    {
        Assert.AreEqual(new Plane(Vector3.left, Vector3.right, Vector3.up), delimiter.Plane);
    }
}
