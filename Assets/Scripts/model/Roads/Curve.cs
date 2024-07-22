using System.Collections;
using System.Collections.Generic;
using CurveExtensions;
using Unity.Mathematics;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Curve
{
    BezierCurve bCurve;
    float offsetDistance;
    float startDistance, endDistance;
    float startT = 0, endT = 1;
    Curve nextCurve;
    DistanceToInterpolation[] lut;
    public float Length { get => CurveUtility.CalculateLength(bCurve) - startDistance - endDistance; }
    public float3 StartPos { get => GetStartPos(); }
    public float3 EndPos { get => GetEndPos(); }
    public float3 StartTangent { get => GetStartTangent(); }
    public float3 EndTangent { get => GetEndTangent(); }
    public float3 StartNormal { get => GetStartNormal(); }
    public float3 EndNormal { get => GetEndNormal(); }

    public Curve() { }

    public Curve(BezierCurve curve)
    {
        bCurve = curve;
        CreateDistanceCache();
    }

    void CreateDistanceCache()
    {
        lut = new DistanceToInterpolation[30];
        CurveUtility.CalculateCurveLengths(bCurve, lut);
    }

    public Curve Duplicate()
    {
        Curve newCurve = new()
        {
            bCurve = bCurve,
            offsetDistance = offsetDistance,
            startDistance = startDistance,
            endDistance = endDistance,
            startT = startT,
            endT = endT,
            nextCurve = nextCurve,
            lut = lut
        };
        return newCurve;
    }

    // Use new crve but keep other attributes
    public Curve Duplicate(BezierCurve curve)
    {
        Curve newCurve = new(curve)
        {
            offsetDistance = offsetDistance,
            startDistance = startDistance,
            endDistance = endDistance,
            startT = startT,
            endT = endT,
            nextCurve = nextCurve
        };
        newCurve.CreateDistanceCache();
        return newCurve;
    }

    public void AddStartDistance(float startDistance)
    {
        this.startDistance += startDistance;
        startT = CurveUtility.GetDistanceToInterpolation(lut, startDistance);
    }

    public void AddEndDistance(float endDistance)
    {
        this.endDistance += endDistance;
        endT = CurveUtility.GetDistanceToInterpolation(lut, endDistance);
    }

    public void Offset(float distance, Orientation orientation)
    {
        if (orientation == Orientation.Left)
            offsetDistance += distance;
        if (orientation == Orientation.Right)
            offsetDistance -= distance;
    }

    Curve GetLastCurve()
    {
        Curve c = this;
        while (c.nextCurve != null)
            c = c.nextCurve;
        return c;
    }

    float3 GetStartPos()
    {
        return CurveUtility.EvaluatePosition(bCurve, startT);
    }

    float3 GetEndPos()
    {
        Curve last = GetLastCurve();
        return CurveUtility.EvaluatePosition(last.bCurve, last.endT);
    }

    float3 GetStartTangent()
    {
        return CurveUtility.EvaluateTangent(bCurve, startT);
    }

    float3 GetEndTangent()
    {
        Curve last = GetLastCurve();
        return CurveUtility.EvaluateTangent(last.bCurve, last.endT);
    }

    float3 GetStartNormal()
    {
        return bCurve.Normalized2DNormal(startT);
    }

    float3 GetEndNormal()
    {
        Curve last = GetLastCurve();
        return last.bCurve.Normalized2DNormal(endT);
    }

    public Curve ReverseChain()
    {
        Curve newHead = ReverseLinkedList(this);

        ReverseHelper(newHead);
        return newHead;

        static Curve ReverseLinkedList(Curve head)
        {
            if (head.nextCurve == null)
                return head;
            Curve prev = head;
            Curve curr = head.nextCurve;
            Curve next = head.nextCurve.nextCurve;
            while (next != null)
            {
                curr.nextCurve = prev;
                prev = curr;
                curr = next;
                next = next.nextCurve;
            }
            curr.nextCurve = prev;
            return curr;
        }
    }

    void ReverseHelper(Curve reversed)
    {
        if (reversed == null)
            return;
        reversed = Duplicate(reversed.bCurve.GetInvertedCurve());
        (reversed.startT, reversed.endT) = (reversed.endT, reversed.startT);
        (reversed.startDistance, reversed.endDistance) = (reversed.endDistance, reversed.startDistance);
        ReverseHelper(reversed.nextCurve);
    }

    public void Add(Curve other)
    {
        Curve last = GetLastCurve();
        Assert.IsTrue(MyNumerics.AreNumericallyEqual(last.EndPos, other.StartPos));
        last.nextCurve = other.Duplicate();
    }

    public float3 EvaluateDistance(float distance)
    {
        float t = CurveUtility.GetDistanceToInterpolation(lut, startDistance + distance);
        return CurveUtility.EvaluatePosition(bCurve, t);
    }

    public OutlineEnum GetOutline(float pointSeparation)
    {
        return new(this, pointSeparation);
    }

    public class OutlineEnum : IEnumerable<float3>
    {
        readonly Curve curve;
        readonly float pointSeparation;
        float currDistance;

        public OutlineEnum(Curve curve, float pointSeparation)
        {
            this.curve = curve;
            this.pointSeparation = pointSeparation;
            currDistance = 0;
        }

        public IEnumerator<float3> GetEnumerator()
        {
            do
            {
                yield return curve.EvaluateDistance(currDistance);
                currDistance += pointSeparation;
            } while (currDistance < curve.Length);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    // class OutlineEnumerator : IEnumerator<float3>
    // {

    // }
    // private float DenormalizeInterpolation(float t)
    // {
    //     return (endT - startT) * t;
    // }

    // float3 EvaluatePosition(float t)
    // {
    //     t = DenormalizeInterpolation(t);
    //     if (offsetDistance == 0)
    //         return CurveUtility.EvaluatePosition(curve, t);
    //     return CurveUtility.EvaluatePosition(curve, t) + offsetDistance * curve.Normalized2DNormal(t);
    // }

    // float3 EvaluateTangent(float t)
    // {
    //     t = DenormalizeInterpolation(t);
    //     return CurveUtility.EvaluateTangent(curve, t);
    // }

}