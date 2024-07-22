// using UnityEngine;
// using NUnit.Framework;
// using Unity.Mathematics;
// using UnityEngine.Splines;
// using System.Linq;

// public class CurveTest
// {

//     float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);

//     [SetUp]
//     public void SetUp()
//     {
//         Game.WipeState();
//     }

//     [Test]
//     public void Offset()
//     {
//         Curve bs = new(new BezierCurve(0, stride, 2 * stride));
//         Curve offsetted = bs.Offset(0);
//         Assert.AreEqual(4, offsetted.Curves.Count);
//         Assert.True(MyNumerics.AreNumericallyEqual(bs.Length, offsetted.Length));
//         float length = CurveUtility.CalculateLength(offsetted.Curves.First());
//         foreach (BezierCurve curve in offsetted.Curves)
//             Assert.True(MyNumerics.AreNumericallyEqual(length, CurveUtility.CalculateLength(curve)));
//     }

//     [Test]
//     public void Split()
//     {
//         Curve bs = new(new BezierCurve(0, stride, 2 * stride));
//         bs.Offset(5);
//         bs.Split(0.5f, out Curve left, out Curve right);
//         Assert.True(MyNumerics.AreNumericallyEqual(left.Length, right.Length));
//     }

//     [Test]
//     public void TruncateConstructor()
//     {
//         Curve bs = new(new BezierCurve(0, stride, 2 * stride));
//         bs.Offset(5);
//         Curve truncated = new(
//             bs,
//             Constants.VertexDistanceFromRoadEnds / bs.Length,
//             (bs.Length - Constants.VertexDistanceFromRoadEnds) / bs.Length
//         );
//         Assert.True(MyNumerics.AreNumericallyEqual(truncated.Length, bs.Length - Constants.VertexDistanceFromRoadEnds * 2));
//     }
// }