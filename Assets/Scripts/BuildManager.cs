using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class BuildManager : MonoBehaviour
{
    private static int start, pivot, end;
    [SerializeField] private const float SplineResolution = 0.4f;
    [SerializeField] private const float LaneWidth = 5f;
    private const float DivergingPointLocatingPrecision = 0.025f;
    [SerializeField] private GameObject roads;
    [SerializeField] private GameObject snapPointPrefab;
    private GameObject snapPoint;
    private const float SnapDistance = 5;
    [SerializeField] private Road roadPrefab;
    private int nextAvailableId;
    private Dictionary<int, Road> roadWatcher = new();
    /// <summary>
    /// Key: node (int), Value: All roads that have lanes that start or end at such node
    /// </summary>
    private Dictionary<int, List<Road>> nodeRoads = new();
    private bool DebugDrawStartEnd;
    private bool DebugDrawCrossedTile;
    private bool DebugDrawCenter;
    private bool DebugDrawLanes;
    private bool DebugDrawMeshVertices;
    private int DebugDuration;

    // Start is called before the first frame update
    void Start()
    {
        start = -1;
        pivot = -1;
        DebugDrawStartEnd = true;
        DebugDrawCrossedTile = false;
        DebugDrawCenter = false;
        DebugDrawLanes = true;
        DebugDrawMeshVertices = false;
        DebugDuration = 1000;
        snapPoint = Instantiate(snapPointPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        int snappedTo = SnapToLaneNodes(Main.MouseWorldPos, SnapDistance);
        if (snappedTo != 1)
        {
            snapPoint.GetComponent<Renderer>().enabled = true;
            snapPoint.transform.position = Grid.GetWorldPosByID(snappedTo);
            snapPoint.transform.position = new Vector3(snapPoint.transform.position.x, Grid.Level + 0.1f, snapPoint.transform.position.z);
        }
        else
        {
            snapPoint.GetComponent<Renderer>().enabled = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            int hoveredTile = Grid.GetIdByPos(Main.MouseWorldPos);
            if (hoveredTile == -1)
            {
                return;
            }
            if (start == -1)
            {
                if (snappedTo != -1)
                {
                    start = snappedTo;
                }
                else
                {
                    start = hoveredTile;
                }

                Log.Info.Log($"Road Manager: Tile A loaded");
            }
            else if (pivot == -1)
            {
                pivot = hoveredTile;
                Log.Info.Log($"Road Manager: Tile B loaded");
            }
            else
            {
                end = hoveredTile;
                Log.Info.Log($"Road Manager: Tile C loaded");
                Vector3 posA = Grid.GetWorldPosByID(start);
                Vector3 posB = Grid.GetWorldPosByID(pivot);
                Vector3 posC = Grid.GetWorldPosByID(end);

                float linearLength = Vector3.Distance(posA, posB) + Vector3.Distance(posB, posC);
                int segCount = (int)(linearLength * SplineResolution + 1);

                Lane connectedLane = CheckConnection(start);
                // CheckConnection(start);
                if (connectedLane != null)
                {
                    Spline spline = BuildSpline(connectedLane.Spline.EvaluatePosition(1), posB, posC, segCount);
                    Log.Info.Log("Road Manager: Connecting Roads");
                    Road road = InitiateRoad(spline);
                    Road connectedRoad = connectedLane.Road;
                    Intersection intersection = null;
                    if (start == connectedLane.Start)
                    {
                        intersection = connectedRoad.Start;
                        intersection.NodeWithLane[start].Add(road.Lanes[0]);

                    }
                    else if (start == connectedLane.End)
                    {
                        intersection = connectedRoad.End;
                        connectedLane.Road.End.NodeWithLane[start].Add(road.Lanes[0]);
                    }
                    intersection.Roads.Add(road);
                    road.InitiateEndIntersection();
                    EvaluateIntersection(intersection);
                }
                // if (otherRoadB != null)
                // {
                //     EvaluateMesh(otherRoadB.Start);
                // }
                if (connectedLane == null)
                {
                    Spline spline = BuildSpline(posA, posB, posC, segCount);
                    Road road = InitiateRoad(spline);
                    road.InitiateStartIntersection();
                    road.InitiateEndIntersection();
                }

                // reset build tool selections
                start = -1;
                pivot = -1;
            }
        }
    }
    Road InitiateRoad(Spline spline)
    {
        Road road = Instantiate(roadPrefab, roads.transform, true);
        road.Spline = spline;
        road.name = $"Road-{nextAvailableId}";
        road.Id = nextAvailableId++;
        roadWatcher.Add(road.Id, road);

        int laneCount = Main.BuildMode;

        Mesh mesh = CreateMesh(road, laneCount);
        road.GetComponent<MeshFilter>().mesh = mesh;
        road.OriginalMesh = Instantiate(mesh);

        List<Lane> lanes = InitiateLanes(road, laneCount);
        road.Lanes = lanes;

        if (nodeRoads.ContainsKey(start))
        {
            nodeRoads[start].Add(road);
        }
        else
        {
            nodeRoads[start] = new() { road };
        }

        if (nodeRoads.ContainsKey(end))
        {
            nodeRoads[end].Add(road);
        }
        else
        {
            nodeRoads[end] = new() { road };
        }

        // road.CrossedTiles = GetCrossedTiles(road);

        // Create graph connection
        PathGraph.Graph.AddVertex(start);
        PathGraph.Graph.AddVertex(end);
        PathGraph.Graph.AddEdge(new TaggedEdge<int, Spline>(start, end, spline));
        return road;
    }

    Spline BuildSpline(Vector3 posA, Vector3 posB, Vector3 posC, int nodeCount)
    {
        // posA = Grid.GetWorldPosByID(Grid.GetIdByPos(posA));
        // posC = Grid.GetWorldPosByID(Grid.GetIdByPos(posC));
        Spline spline = new();
        Vector3 AB, BC, AB_BC, prev = new();
        nodeCount -= 1;
        for (int i = 0; i <= nodeCount; i++)
        {
            AB = Vector3.Lerp(posA, posB, 1 / (float)nodeCount * i);
            BC = Vector3.Lerp(posB, posC, 1 / (float)nodeCount * i);
            AB_BC = Vector3.Lerp(AB, BC, 1 / (float)nodeCount * i);
            spline.Add(new BezierKnot(AB_BC), TangentMode.AutoSmooth);
            if (DebugDrawCenter)
            {
                if (i == 0)
                {
                    prev = AB_BC + new Vector3(0, 0.1f, 0);
                }
                else
                {
                    Debug.DrawLine(prev, AB_BC, Color.blue, DebugDuration);
                    prev = AB_BC + new Vector3(0, 0.1f, 0); ;
                }
            }
        }
        return spline;
    }

    Mesh CreateMesh(Road road, int laneCount)
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
            if (DebugDrawMeshVertices)
            {
                DebugExtension.DebugPoint(left, Color.black, 1, DebugDuration);
                DebugExtension.DebugPoint(right, Color.black, 1, DebugDuration);
            }

        }
        road.LeftMesh = leftVs;
        road.RightMesh = rightVs;

        return Extrude(leftVs, rightVs);
    }

    Lane CheckConnection(int node)
    {
        Lane connectedLane = null;
        foreach (var (id, road) in roadWatcher)
        {
            foreach (Lane lane in road.Lanes)
            {
                if (node == lane.Start)
                {
                    connectedLane = lane;
                }
                else if (node == lane.End)
                {
                    connectedLane = lane;
                }
            }

        }
        return connectedLane;
    }

    void RemoveRoad(Road road)
    {
        DestroyImmediate(road.gameObject);
        roadWatcher.Remove(road.Id);
    }

    List<Lane> InitiateLanes(Road road, int laneCount)
    {
        Spline spline = road.Spline;

        List<Lane> lanes = new();

        for (int i = 0; i < laneCount; i++)
        {
            lanes.Add(new()
            {
                Spline = new(),
                Road = road,
                Start = Grid.GetIdByPos(GetLanePosition(spline, 0, laneCount, i)),
                End = Grid.GetIdByPos(GetLanePosition(spline, 1, laneCount, i))
            });
        }

        int segCount = spline.Knots.Count() - 1;

        // Iterate by distance on curve
        for (int i = 0; i <= segCount; i++)
        {
            float t = 1 / (float)segCount * i;

            // Iterate by each lane
            for (int j = 0; j < laneCount; j++)
            {
                float3 pos = GetLanePosition(spline, t, laneCount, j);
                lanes[j].Spline.Add(new BezierKnot(pos), TangentMode.AutoSmooth);
            }
        }
        return lanes;

        float3 GetLanePosition(Spline spline, float t, int laneCount, int lane)
        {
            float3 normal = GetNormal(spline, t);
            float3 position = spline.EvaluatePosition(t);
            return position + normal * LaneWidth * ((float)laneCount / 2 - 0.5f) - lane * normal * LaneWidth;
        }
        float3 GetNormal(Spline spline, float t)
        {
            float3 tangent = spline.EvaluateTangent(t);
            float3 upVector = spline.EvaluateUpVector(t);
            return Vector3.Cross(tangent, upVector).normalized;
        }

    }

    Mesh ConnectLaneMesh(int laneCount, Spline spline, Delimiter delimiter)
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

    void EvaluateIntersection(Intersection intersection)
    {
        List<int> nodes = intersection.GetNodes();
        Road mainRoad = intersection.GetMainRoad();
        float3 upVector = mainRoad.Spline.EvaluateUpVector(1);
        float3 leftCorner = mainRoad.LeftMesh.Last();
        float3 rightCorner = mainRoad.RightMesh.Last();

        if (intersection.IsRepeating())
        {
            Road road = intersection.GetMinorRoadofNode(nodes[0]);
            Delimiter delimiter = GetBounds(mainRoad.LeftMesh.Last(), mainRoad.RightMesh.Last(), 1);
            ConnectLanes(road.Lanes[0], mainRoad.Lanes[0], delimiter);
        }

        List<Lane> lanes = new();
        int start = -1;
        int end = -1;
        bool inGroup = false;
        List<Road> neighboringLanes = new();
        // iterate through lanes of mainroad by pairs
        // (0,1), (1,2), (2,3) and so on
        for (int i = 1; i < mainRoad.GetLaneCount(); i++)
        {
            int nodeLeft = nodes[i - 1];
            int nodeRight = nodes[i];
            Road roadLeft = null;
            Road roadRight = null;
            if (intersection.IsNodeConnected(nodeLeft))
            {
                roadLeft = intersection.GetMinorRoadofNode(nodeLeft);
            }
            if (intersection.IsNodeConnected(nodeRight))
            {
                roadRight = intersection.GetMinorRoadofNode(nodeRight);
            }
            bool roadAreDifferent = roadLeft != roadRight && roadLeft != null && roadRight != null;
            bool exitedGroup = (inGroup && roadRight == null) || (i == mainRoad.GetLaneCount() - 1 && inGroup);
            bool isolatedLane = !inGroup & roadLeft != null || (i == mainRoad.GetLaneCount() - 1 && roadRight != null);
            if (roadAreDifferent)
            {
                inGroup = true;
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
            if (exitedGroup)
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
                inGroup = false;
                neighboringLanes = new();
            }
            else if (isolatedLane)
            {
                int offset = i;
                Road road = roadLeft;
                if (i == nodes.Count() - 1 && roadLeft == null)
                {
                    offset += 1;
                    road = roadRight;
                }
                Delimiter delimiter = GetBounds(leftCorner, rightCorner, offset);
                ConnectLanes(road.Lanes[0], mainRoad.Lanes[offset - 1], delimiter);
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
            Mesh m = Instantiate(mainRoad.OriginalMesh);


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

        Delimiter GetBounds(float3 leftCorner, float3 rightCorner, int offset)
        {

            float3 leftBound = Vector3.Lerp(leftCorner, rightCorner, (float)(offset - 1) / mainRoad.GetLaneCount());
            float3 rightBound = Vector3.Lerp(leftCorner, rightCorner, (float)offset / mainRoad.GetLaneCount());
            return new(leftBound, rightBound, upVector);
        }
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

    Mesh Extrude(List<float3> leftVs, List<float3> rightVs)
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

    int SnapToLaneNodes(float3 worldPos, float snapDistance)
    {
        float minDistance = float.MaxValue;
        int closest = -1;
        foreach (Road road in roadWatcher.Values)
        {
            foreach (Lane lane in road.Lanes)
            {
                float distance = Vector3.Distance(Grid.GetWorldPosByID(lane.Start), worldPos);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    closest = lane.Start;
                }

                distance = Vector3.Distance(Grid.GetWorldPosByID(lane.End), worldPos);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    closest = lane.End;
                }
            }
        }
        if (minDistance > snapDistance)
        {
            return -1;
        }
        return closest;
    }
}
