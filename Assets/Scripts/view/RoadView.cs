using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class RoadView
{
    private const float DivergingPointLocatingPrecision = 0.025f;
    private const float LaneWidth = GlobalConstants.LaneWidth;
    public static void EvaluateIntersection(Intersection intersection)
    {
        List<int> nodes = intersection.GetNodes();
        Road mainRoad = intersection.GetMainRoad();
        float3 upVector = mainRoad.Spline.EvaluateUpVector(1);
        float3 leftCorner = mainRoad.LeftMesh.Last();
        float3 rightCorner = mainRoad.RightMesh.Last();

        if (intersection.IsRepeating())
        {
            ConnectRepeatedRoad();
            return;
        }

        List<Lane> lanes = new();
        int start = -1;
        int end = -1;
        bool inNeighboringLanes = false;
        List<Road> neighboringLanes = new();
        Road roadLeft;
        Road roadRight;
        // iterate through lanes of mainroad by pairs
        // (0,1), (1,2), (2,3) and so on
        for (int i = 1; i < mainRoad.GetLaneCount(); i++)
        {
            int nodeLeft = nodes[i - 1];
            int nodeRight = nodes[i];

            roadLeft = intersection.IsNodeConnected(nodeLeft) ? intersection.GetMinorRoadofNode(nodeLeft) : null;
            roadRight = intersection.IsNodeConnected(nodeRight) ? intersection.GetMinorRoadofNode(nodeRight) : null;

            if (intersection.IsNodeConnected(nodeRight))
            {
                roadRight = intersection.GetMinorRoadofNode(nodeRight);
            }
            if (RoadsAreDifferent())
            {
                AddNeighboringLanes(i);
            }
            if (ExitedNeighboringLanes(i))
            {
                HandleNeighboringLanes();
            }
            else if (HasIsolatedRoad(i))
            {
                ConnectIsolatedRoad(i);
            }

        }

        void ConnectLanes(Lane lesserLane, Lane mainLane, Delimiter delimiter)
        {
            Road lesserRoad = lesserLane.Road;
            Spline combinedSpline = new();
            combinedSpline.Copy(mainLane.Spline);
            foreach (BezierKnot knot in lesserLane.Spline)
            {
                combinedSpline.Add(knot);
            }

            lesserRoad.GetComponent<MeshFilter>().mesh = ConnectLaneMesh(lesserRoad.Lanes.Count, combinedSpline, delimiter);
        }

        List<float3> GetDivergePoints(List<Road> roads)
        {
            List<float3> divergePoints = new();

            // find diverge points
            for (int j = 1; j < roads.Count(); j++)
            {
                Road roadLeft = roads[j - 1];
                Road roadRight = roads[j];
                for (float k = 0; k < 1.5; k += DivergingPointLocatingPrecision)
                {
                    Spline leftSpline = new();
                    Spline rightSpline = new();
                    foreach (float3 knot in roadLeft.LeftMesh)
                    {
                        leftSpline.Add(new BezierKnot(knot), TangentMode.AutoSmooth);
                    }
                    foreach (float3 knot in roadRight.RightMesh)
                    {
                        rightSpline.Add(new BezierKnot(knot), TangentMode.AutoSmooth);
                    }
                    float t1 = leftSpline.GetCurveInterpolation(0, k);
                    float t2 = rightSpline.GetCurveInterpolation(0, k);
                    float3 p1 = leftSpline.EvaluatePosition(t1);
                    float3 p2 = rightSpline.EvaluatePosition(t2);
                    if (Vector3.Distance(p1, p2) > LaneWidth * 2 + 0.5f)
                    {
                        float3 divergePoint = Vector3.Lerp(p1, p2, 0.5f);
                        divergePoints.Add(divergePoint);
                        DebugExtension.DebugPoint(divergePoint, Color.black, 1, 100);
                        break;
                    }
                }

            }

            return divergePoints;
        }

        List<Vector3> BuildPolygon(List<float3> divergePoints)
        {
            float3 leftBound = Vector3.Lerp(leftCorner, rightCorner, (float)start / nodes.Count());
            float3 rightBound = Vector3.Lerp(leftCorner, rightCorner, (float)(end + 1) / nodes.Count());
            float3 midPoint = Vector3.Lerp(leftBound, rightBound, 0.5f);
            List<Vector3> v = new() { midPoint, leftBound };
            foreach (float3 pos in divergePoints)
            {
                v.Add(pos);
            }

            v.Add(rightBound);
            return v;
        }

        void DrawConnectorPolygon(Road mainRoad, List<float3> divergePoints, List<Vector3> v)
        {
            Mesh m = Object.Instantiate(mainRoad.OriginalMesh);

            List<int> tri = new();
            List<Vector2> uvs = new() { new(0.5f, 1), new(0, 1) };
            List<Vector3> normals = new() { Vector3.up, Vector3.up };


            int index = 1;
            foreach (float3 pos in divergePoints)
            {
                uvs.Add(new(index * 1 / ((float)divergePoints.Count() + 1), 1));
                normals.Add(Vector3.up);
                index++;
            }
            uvs.Add(new(1, 1));
            normals.Add(Vector3.up);

            int offset = m.vertexCount;
            for (int j = 2; j < v.Count(); j++)
            {
                tri.AddRange(new int[] { offset, offset + j - 1, offset + j });
            }

            foreach (float3 vertex in v)
            {
                DebugExtension.DebugPoint(vertex, Color.magenta, 1, 5);
            }

            List<Vector3> mv = new(m.vertices);
            mv.AddRange(v);

            List<int> mtri = new(m.triangles);
            mtri.AddRange(tri);

            List<Vector2> muv = new(m.uv);
            muv.AddRange(uvs);

            List<Vector3> mn = new(m.normals);
            mn.AddRange(normals);

            m.SetVertices(mv);
            m.SetUVs(0, muv);
            m.SetTriangles(mtri, 0);
            m.SetNormals(mn);

            mainRoad.GetComponent<MeshFilter>().mesh = m;
        }

        Delimiter GetBounds(int offset)
        {

            float3 leftBound = Vector3.Lerp(leftCorner, rightCorner, (float)(offset - 1) / mainRoad.GetLaneCount());
            float3 rightBound = Vector3.Lerp(leftCorner, rightCorner, (float)offset / mainRoad.GetLaneCount());
            return new(leftBound, rightBound, upVector);
        }

        bool RoadsAreDifferent()
        {
            return roadLeft != roadRight && roadLeft != null && roadRight != null;
        }

        bool ExitedNeighboringLanes(int i)
        {
            return (roadRight == null || ReachedLastLane(i)) && inNeighboringLanes;
        }

        bool HasIsolatedRoad(int i)
        {
            return !inNeighboringLanes & roadLeft != null || ReachedLastLane(i) & roadRight != null;
        }

        bool ReachedLastLane(int i)
        {
            return i == mainRoad.GetLaneCount() - 1;
        }

        void AddNeighboringLanes(int i)
        {
            inNeighboringLanes = true;
            if (!lanes.Contains(mainRoad.Lanes[i - 1]))
            {
                lanes.Add(mainRoad.Lanes[i - 1]);
            }
            lanes.Add(mainRoad.Lanes[i]);
            if (!neighboringLanes.Contains(roadLeft))
            {
                neighboringLanes.Add(roadLeft);
            }
            neighboringLanes.Add(roadRight);
            if (start == -1)
            {
                start = i - 1;
            }
            end = i;
        }

        void HandleNeighboringLanes()
        {
            List<float3> divergePoints = GetDivergePoints(neighboringLanes);
            List<Vector3> polygonVertices = BuildPolygon(divergePoints);
            DrawConnectorPolygon(mainRoad, divergePoints, polygonVertices);

            int offset = 1;
            foreach (var road in neighboringLanes)
            {

                Delimiter delimiter = new(polygonVertices[offset], polygonVertices[offset + 1], upVector);
                ConnectLanes(road.Lanes[0], lanes[offset - 1], delimiter);
                offset++;
            }
            inNeighboringLanes = false;
            neighboringLanes = new();
        }

        void ConnectIsolatedRoad(int i)
        {
            int offset = i;
            Road road = roadLeft;
            if (i == nodes.Count() - 1 && roadLeft == null)
            {
                offset += 1;
                road = roadRight;
            }
            Delimiter delimiter = GetBounds(offset);
            ConnectLanes(road.Lanes[0], mainRoad.Lanes[offset - 1], delimiter);
        }

        void ConnectRepeatedRoad()
        {
            Road road = intersection.GetMinorRoadofNode(nodes[0]);
            Delimiter delimiter = GetBounds(1);
            ConnectLanes(road.Lanes[0], mainRoad.Lanes[0], delimiter);
        }
    }

    static Mesh ConnectLaneMesh(int laneCount, Spline spline, Delimiter delimiter)
    {
        int segCount = spline.Knots.Count();
        Vector3 end = spline.EvaluatePosition(1);
        List<float3> leftVs = new();
        List<float3> rightVs = new();
        Plane guard = delimiter.Plane;
        float3 leftBound = delimiter.LeftBound;
        float3 rightBound = delimiter.RightBound;
        for (int i = segCount; i >= 0; i--)
        {
            spline.Evaluate(1 / (float)segCount * i, out float3 position, out float3 forward, out float3 upVector);
            float3 normal = Vector3.Cross(forward, upVector).normalized;
            float3 left = position + normal * LaneWidth * laneCount / 2;
            float3 right = position - normal * LaneWidth * laneCount / 2;
            bool leftOverlap = false;
            bool rightOverlap = false;
            if (!guard.SameSide(end, left))
            {
                left = leftBound;
                leftOverlap = true;
            }
            else if (Vector3.Distance(left, leftBound) < 1.5)
            {
                left = leftBound;
            }

            if (!guard.SameSide(end, right))
            {

                right = rightBound;
                rightOverlap = true;
            }
            else if (Vector3.Distance(right, rightBound) < 1.5)
            {
                right = rightBound;
            }
            if (leftOverlap && rightOverlap)
            {
                leftVs.Add(left);
                rightVs.Add(right);
                break;
            }
            leftVs.Add(left);
            rightVs.Add(right);

        }

        leftVs.Reverse();
        rightVs.Reverse();

        return Extrude(leftVs, rightVs);
    }

    public static Mesh Extrude(List<float3> leftVs, List<float3> rightVs)
    {
        Mesh m = new();
        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector2> uvs = new();
        List<Vector3> normals = new();
        int offset;
        for (int i = 1; i < leftVs.Count(); i++)
        {
            Vector3 p1 = leftVs[i - 1];
            Vector3 p2 = rightVs[i - 1];
            Vector3 p3 = leftVs[i];
            Vector3 p4 = rightVs[i];

            offset = 4 * (i - 1);
            int t1 = offset + 0;
            int t2 = offset + 2;
            int t3 = offset + 3;
            int t4 = offset + 3;
            int t5 = offset + 1;
            int t6 = offset + 0;
            float f1 = 1 / (float)leftVs.Count() * (i - 1);
            float f2 = 1 / (float)leftVs.Count() * i;
            uvs.AddRange(new List<Vector2> { new(0, f1), new(1, f1), new(0, f2), new(1, f2) });
            normals.AddRange(new List<Vector3> { Vector3.up, Vector3.up, Vector3.up, Vector3.up });
            verts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
            tris.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });
        }
        m.SetVertices(verts);
        m.SetUVs(0, uvs);
        m.SetNormals(normals);
        m.SetTriangles(tris, 0);
        return m;
    }

    public static Mesh CreateMesh(Road road, int laneCount)
    {
        Spline spline = road.Spline;
        float3 left, right;
        List<float3> leftVs = new();
        List<float3> rightVs = new();
        int segCount = spline.Knots.Count() - 1;
        for (int i = 0; i <= segCount; i++)
        {
            spline.Evaluate(1 / (float)segCount * i, out float3 position, out float3 forward, out float3 upVector);
            float3 normal = Vector3.Cross(forward, upVector).normalized;
            left = position + normal * LaneWidth * laneCount / 2;
            right = position - normal * LaneWidth * laneCount / 2;
            leftVs.Add(left);
            rightVs.Add(right);
        }
        road.LeftMesh = leftVs;
        road.RightMesh = rightVs;

        return Extrude(leftVs, rightVs);
    }
}