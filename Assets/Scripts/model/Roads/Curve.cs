using System;
using System.Collections;
using System.Collections.Generic;
using CurveExtensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Curve
{
    BezierCurve bCurve;
    float bCurveLength;
    float offsetDistance;
    float startDistance, endDistance;
    float startT = 0, endT = 1;
    Curve nextCurve;
    DistanceToInterpolation[] lut;
    public float SegmentLength { get => bCurveLength - startDistance - endDistance; }
    public float Length { get => GetLength(); }
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
        bCurveLength = CurveUtility.CalculateLength(bCurve);
    }

    void CreateDistanceCache()
    {
        lut = new DistanceToInterpolation[30];
        CurveUtility.CalculateCurveLengths(bCurve, lut);
    }

    float GetLength()
    {
        Assert.IsFalse(this == nextCurve);
        if (nextCurve != null)
            return SegmentLength + nextCurve.Length;
        return SegmentLength;
    }

    public Curve Duplicate()
    {
        Curve newCurve = DuplicateHelper();
        if (newCurve.nextCurve != null)
            newCurve.nextCurve = newCurve.nextCurve.Duplicate();

        return newCurve;
    }

    Curve DuplicateHelper()
    {
        Curve newCurve = new()
        {
            bCurve = bCurve,
            bCurveLength = bCurveLength,
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

    public Curve AddStartDistance(float delta)
    {
        Assert.IsTrue(delta >= 0);
        int index = 0;
        Curve curr = GetCurveByIndex(index);
        while (curr.SegmentLength == 0)
            curr = GetCurveByIndex(++index);
        while (curr.SegmentLength < delta)
        {
            delta -= curr.SegmentLength;
            curr.startDistance = curr.bCurveLength;
            curr.startT = 1;
            curr.endDistance = 0;
            curr.endT = 1;
            Assert.AreEqual(0, curr.SegmentLength);
            curr = curr.nextCurve;
        }
        curr.startDistance += delta;
        curr.startT = CurveUtility.GetDistanceToInterpolation(curr.lut, curr.startDistance);
        return this;
    }

    public Curve AddEndDistance(float delta)
    {
        Assert.IsTrue(delta >= 0);
        int index = GetChainLength() - 1;
        Curve curr = GetCurveByIndex(index);
        while (curr.SegmentLength == 0)
            curr = GetCurveByIndex(--index);
        while (curr.SegmentLength < delta)
        {
            delta -= curr.SegmentLength;
            curr.startDistance = curr.bCurveLength;
            curr.startT = 1;
            curr.endDistance = 0;
            curr.endT = 1;
            Assert.AreEqual(0, curr.SegmentLength);
            curr = GetCurveByIndex(--index);
        }
        curr.endDistance += delta;
        curr.endT = CurveUtility.GetDistanceToInterpolation(curr.lut, curr.bCurveLength - curr.endDistance);
        return this;
    }

    public Curve GetCurveByIndex(int index)
    {
        Curve curve = this;
        for (int i = 0; i < index; i++)
            curve = curve.nextCurve;
        return curve;
    }

    public int GetChainLength()
    {
        int count = 0;
        Curve curve = this;
        while (curve != null)
        {
            count++;
            curve = curve.nextCurve;
        }
        return count;
    }

    public float GetDistanceToInterpolation(float distance)
    {
        return CurveUtility.GetDistanceToInterpolation(lut, distance);
    }

    public Curve Offset(float distance)
    {
        Curve curve = this;
        while (curve != null)
        {
            curve.offsetDistance += distance;
            curve = curve.nextCurve;
        }
        return this;
    }

    Curve GetLastCurve()
    {
        Curve c = this;
        while (c.nextCurve != null && c.nextCurve.SegmentLength != 0)
            c = c.nextCurve;
        return c;
    }

    float3 GetStartPos()
    {
        if (SegmentLength == 0)
            return nextCurve.GetStartPos();
        if (offsetDistance == 0)
            return CurveUtility.EvaluatePosition(bCurve, startT);
        return CurveUtility.EvaluatePosition(bCurve, startT) + StartNormal * offsetDistance;
    }

    float3 GetEndPos()
    {
        Curve last = GetLastCurve();
        if (offsetDistance == 0)
        {
            return CurveUtility.EvaluatePosition(last.bCurve, last.endT);
        }
        return CurveUtility.EvaluatePosition(last.bCurve, last.endT) + last.EndNormal * offsetDistance;
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

        newHead = ReverseHelper(newHead);
        return newHead;

        static Curve ReverseLinkedList(Curve head)
        {
            if (head == null || head.nextCurve == null)
                return head;
            Curve prev = null;
            Curve curr = head;
            while (curr != null)
            {
                Curve next = curr.nextCurve;
                curr.nextCurve = prev;
                prev = curr;
                curr = next;
            }

            return prev;
        }
    }

    Curve ReverseHelper(Curve curve)
    {
        if (curve == null)
            return null;
        Curve reversed = new()
        {
            bCurve = curve.bCurve.GetInvertedCurve(),
            startDistance = curve.endDistance,
            endDistance = curve.startDistance,
            bCurveLength = curve.bCurveLength,
            offsetDistance = -curve.offsetDistance,
            nextCurve = curve.nextCurve
        };
        reversed.nextCurve = ReverseHelper(reversed.nextCurve);
        reversed.CreateDistanceCache();
        reversed.startT = reversed.GetDistanceToInterpolation(curve.startDistance);
        reversed.endT = reversed.GetDistanceToInterpolation(curve.bCurveLength - curve.endDistance);
        return reversed;
    }

    public void Add(Curve other)
    {
        Curve last = GetLastCurve();
        if (!MyNumerics.IsApproxEqual(last.EndPos, other.StartPos))
        {
            Debug.Log("end: " + last.EndPos);
            Debug.Log("start: " + other.StartPos);
            Assert.IsTrue(false);
        }
        last.nextCurve = other.Duplicate();
    }

    public float3 EvaluateDistancePos(float distance)
    {
        if (distance > SegmentLength + 0.01f && nextCurve != null)
            return nextCurve.EvaluateDistancePos(distance - SegmentLength);
        float t = CurveUtility.GetDistanceToInterpolation(lut, startDistance + distance);
        if (offsetDistance == 0)
            return CurveUtility.EvaluatePosition(bCurve, t);
        return CurveUtility.EvaluatePosition(bCurve, t) + bCurve.Normalized2DNormal(t) * offsetDistance;
    }

    public float3 EvaluateDistanceTangent(float distance)
    {
        if (distance > SegmentLength && nextCurve != null)
            return nextCurve.EvaluateDistanceTangent(distance - SegmentLength);
        float t = CurveUtility.GetDistanceToInterpolation(lut, startDistance + distance);
        return CurveUtility.EvaluateTangent(bCurve, t);
    }

    public float GetNearestPoint(Ray ray, out float distanceOnCurve, int resolution = 15)
    {
        float minDistance = float.MaxValue;
        float currDist = 0;
        float distanceStep = Length / resolution;
        float localMin = 0;
        while (currDist <= Length)
        {
            float3 pos = EvaluateDistancePos(currDist);
            float distance = GetDistanceToCurve(pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                localMin = currDist;
            }
            currDist += distanceStep;
        }
        float low = localMin - distanceStep >= 0 ? localMin - distanceStep : localMin;
        float high = localMin + distanceStep <= Length ? localMin + distanceStep : localMin;
        do
        {
            float mid = (low + high) / 2;
            if (GetDistanceToCurve(EvaluateDistancePos(mid - 0.01f)) < GetDistanceToCurve(EvaluateDistancePos(mid + 0.01f)))
                high = mid;
            else
                low = mid;
        } while (high - low > 0.01f);

        distanceOnCurve = low;
        return GetDistanceToCurve(EvaluateDistancePos(low));

        float GetDistanceToCurve(float3 pos)
        {
            return Vector3.Cross(ray.direction, (Vector3)pos - ray.origin).magnitude;
        }
    }


    // NOT nextCurve compatible
    public void Split(float distance, out Curve left, out Curve right)
    {
        Assert.IsTrue(distance < Length);
        int index = 0;
        Curve toSplit = this;
        float currDistance = distance;
        while (currDistance > toSplit.SegmentLength)
        {
            currDistance -= toSplit.SegmentLength;
            toSplit = toSplit.nextCurve;
            index++;
        }

        CurveUtility.Split(toSplit.bCurve, toSplit.GetDistanceToInterpolation(currDistance), out BezierCurve l, out BezierCurve r);
        left = new(l) { offsetDistance = offsetDistance };
        left.AddStartDistance(startDistance);
        right = new(r) { offsetDistance = offsetDistance };
        right.AddEndDistance(endDistance);

        if (index == 0)
            return;
        Curve newHead = Duplicate();
        Curve prev = newHead;
        while (index != 1)
        {
            prev = prev.nextCurve;
            index--;
        }
        toSplit = prev.nextCurve;
        prev.nextCurve = left;
        left = newHead;
        right.nextCurve = toSplit.nextCurve;

    }

    public OutlineEnum GetOutline(int numPoints)
    {
        return new(this, numPoints);
    }

    public class OutlineEnum : IEnumerable<float3>
    {
        readonly Curve curve;
        readonly float numPoints;

        public OutlineEnum(Curve curve, float numPoints)
        {
            this.curve = curve;
            this.numPoints = numPoints;
        }

        public IEnumerator<float3> GetEnumerator()
        {
            float pointSeparation = curve.Length / numPoints;
            int count = 0;
            float currDistance = 0;
            while (count++ <= numPoints)
            {
                yield return curve.EvaluateDistancePos(currDistance);
                currDistance += pointSeparation;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public Curve GetEndT()
    {
        return nextCurve;
    }

}