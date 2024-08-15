using System;
using System.Collections;
using System.Collections.Generic;
using CurveExtensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Curve : IPersistable
{
    public uint Id { get; set; }
    BezierCurve bCurve;
    float bCurveLength;
    float offsetDistance;
    float startDistance, endDistance;
    float startT = 0, endT = 1;
    [SaveID]
    Curve nextCurve;
    [NotSaved]
    DistanceToInterpolation[] lut;
    [NotSaved]
    public float SegmentLength { get => bCurveLength - startDistance - endDistance; }
    [NotSaved]
    public float Length { get => GetLength(); }
    [NotSaved]
    public float3 StartPos { get => GetStartPos(); }
    [NotSaved]
    public float3 EndPos { get => GetEndPos(); }
    [NotSaved]
    public float3 StartTangent { get => GetStartTangent(); }
    [NotSaved]
    public float3 EndTangent { get => GetEndTangent(); }
    [NotSaved]
    public float3 StartNormal { get => GetStartNormal(); }
    [NotSaved]
    public float3 EndNormal { get => GetEndNormal(); }
    [NotSaved]
    const float GetNearestPointTolerance = 0.001f;
    [NotSaved]
    const float minimumCurveLength = 0.005f;

    public Curve() { }

    public Curve(BezierCurve curve)
    {
        bCurve = curve;
        bCurveLength = CurveUtility.CalculateLength(bCurve);
        CreateDistanceCache();
    }

    public Curve GetNextCurve()
    {
        return nextCurve;
    }

    public void CreateDistanceCache()
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
        while (curr.SegmentLength < delta)
        {
            delta -= curr.SegmentLength;
            curr = curr.nextCurve;
        }
        curr.startDistance += delta;
        curr.startT = CurveUtility.GetDistanceToInterpolation(curr.lut, curr.startDistance);
        return curr;
    }

    public Curve AddEndDistance(float delta)
    {
        Assert.IsTrue(delta >= 0);
        int index = GetChainLength() - 1;
        Curve curr = GetCurveByIndex(index);
        while (curr.SegmentLength < delta)
        {
            delta -= curr.SegmentLength;
            curr = GetCurveByIndex(--index);
            curr.nextCurve = null;
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

    public int GetChainDepth()
    {
        int chainDepth = 1;
        Curve current = this;
        while (current.nextCurve != null)
        {
            chainDepth++;
            current = current.nextCurve;
        }
        return chainDepth;
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
        return CurveUtility.EvaluatePosition(bCurve, startT) + StartNormal * offsetDistance;
    }

    float3 GetEndPos()
    {
        Curve last = GetLastCurve();
        return CurveUtility.EvaluatePosition(last.bCurve, last.endT) + last.EndNormal * last.offsetDistance;
    }

    float3 GetStartTangent()
    {
        return math.normalize(CurveUtility.EvaluateTangent(bCurve, startT));
    }

    float3 GetEndTangent()
    {
        Curve last = GetLastCurve();
        return math.normalize(CurveUtility.EvaluateTangent(last.bCurve, last.endT));
    }

    float3 GetStartNormal()
    {
        return bCurve.Normalized2DNormal(startT);
    }

    float3 GetEndNormal()
    {
        Curve last = GetLastCurve();
        return last.bCurve.Normalized2DNormal(last.endT);
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
        if (!MyNumerics.IsApproxEqual(EndPos, other.StartPos))
        {
            Debug.Log("Left curve misaligns with right curve... Diff: " + math.length(EndPos - other.StartPos));
        }
        last.nextCurve = other.Duplicate();
    }

    public float3 EvaluateDistancePos(float distance)
    {
        if (distance > SegmentLength && nextCurve != null)
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
        return math.normalize(CurveUtility.EvaluateTangent(bCurve, t));
    }

    public float GetNearestPoint(Ray ray, out float distanceOnCurve, int resolution = 10)
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
            if (GetDistanceToCurve(EvaluateDistancePos(mid - GetNearestPointTolerance))
                < GetDistanceToCurve(EvaluateDistancePos(mid + GetNearestPointTolerance)))
                high = mid;
            else
                low = mid;
        } while (high - low > GetNearestPointTolerance);

        distanceOnCurve = low;
        return GetDistanceToCurve(EvaluateDistancePos(low));

        float GetDistanceToCurve(float3 pos)
        {
            return Vector3.Cross(ray.direction, (Vector3)pos - ray.origin).magnitude;
        }
    }

    public void Split(float distance, out Curve left, out Curve right)
    {
        Assert.IsTrue(distance < Length);
        int index = 0;
        Curve toSplit = this;
        float currDistance = distance;

        while (true)
        {
            if (Math.Abs(currDistance - toSplit.SegmentLength) < minimumCurveLength)
            {
                left = this;
                right = toSplit.nextCurve;
                toSplit.nextCurve = null;
                return;
            }
            if (currDistance >= toSplit.SegmentLength)
            {
                currDistance -= toSplit.SegmentLength;
                toSplit = toSplit.nextCurve;
                index++;
            }
            else
                break;
        }

        CurveUtility.Split(toSplit.bCurve, toSplit.GetDistanceToInterpolation(currDistance), out BezierCurve l, out BezierCurve r);
        left = new(l) { offsetDistance = offsetDistance };
        left = left.AddStartDistance(startDistance);
        right = new(r) { offsetDistance = offsetDistance };
        right = right.AddEndDistance(endDistance);

        if (index == 0)
        {
            right.nextCurve = nextCurve;
            return;
        }
        Curve newHead = Duplicate();
        Curve prev = newHead;
        while (index != 1)
        {
            prev = prev.nextCurve;
            index--;
        }
        Curve next = prev.nextCurve.nextCurve;
        prev.nextCurve = left;
        left = newHead;
        right.nextCurve = next;
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
            float pointSeparation = curve.Length / (numPoints - 1);
            int count = 0;
            float currDistance = 0;
            while (count++ < numPoints)
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

    public Curve SplitInToSegments(int segmemtCount)
    {
        Assert.IsNull(nextCurve);
        if (segmemtCount <= 1)
            return this;
        
        float segmentLength = SegmentLength / segmemtCount;
        Split(segmentLength, out Curve head, out Curve right);
        Curve current = head;
        for (int i = 0; i < segmemtCount - 2; i++)
        {
            right.Split(segmentLength, out Curve left, out right);
            current.nextCurve = left;
            current = current.nextCurve;
        }
        current.nextCurve = right;
        return head;
    }

    public BezierCurve GetCurve()
    {
        return bCurve;
    }


    public override bool Equals(object obj)
    {
        if (obj is Curve other)
        {
            return Id == other.Id && bCurve.P0.Equals(other.bCurve.P0) && bCurve.P1.Equals(other.bCurve.P1)
                && bCurve.P2.Equals(other.bCurve.P2) && bCurve.P3.Equals(other.bCurve.P3) && bCurveLength == other.bCurveLength
                && offsetDistance == other.offsetDistance && startDistance == other.startDistance && endDistance == other.endDistance
                && startT == other.startT && endT == other.endT && !(nextCurve == null ^ other.nextCurve == null)
                && ((nextCurve == null && other.nextCurve == null) || nextCurve.Equals(other.nextCurve));
        }
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}