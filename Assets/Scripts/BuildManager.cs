using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QuikGraph;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class BuildManager : MonoBehaviour
{
    private static int start, pivot, end;
    [SerializeField] private const float SplineResolution = 0.4f;
    [SerializeField] private const float LaneWidth = 5f;
    [SerializeField] private GameObject roads;

    [SerializeField] private Road roadPrefab;
    private int nextAvailableId;
    private Dictionary<int, Road> roadDict = new();
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int hoveredTile = Grid.GetIdByCursor();
            if (hoveredTile == -1)
            {
                return;
            }
            if (start == -1)
            {
                start = hoveredTile;
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
                int segCount = (int)(linearLength * SplineResolution + 1.0);



                CheckConnection(start, end, out Road otherRoad, out Lane connectedLane, out bool flow);
                if (otherRoad != null)
                {
                    Spline spline = CreateSpline(connectedLane.Spline.EvaluatePosition(1), posB, posC, segCount);
                    Log.Info.Log("Road Manager: Connecting Roads");
                    int[] starts = new int[] { start };
                    Road r = InitiateRoad(spline, otherRoad.End, end, Main.BuildMode);
                    EvaluateMesh(otherRoad.End);
                }
                else
                {
                    Spline spline = CreateSpline(posA, posB, posC, segCount);
                    Road road = InitiateRoad(spline, start, end, Main.BuildMode);
                    // IntersectCheck(road, out int newNode, out Road roadToSplit);
                    // if (newNode != -1)
                    // {

                    // }
                }

                // reset build tool selections
                start = -1;
                pivot = -1;
            }
        }
    }
    Road InitiateRoad(Spline spline, int start, int end, int laneCount)
    {
        Road road = Instantiate(roadPrefab, roads.transform, true);
        road.Spline = spline;
        road.name = $"Road-{nextAvailableId}";
        road.Start = start;
        road.End = end;
        road.Id = nextAvailableId++;
        roadDict.Add(road.Id, road); // BuildManager now watches this road

        Mesh m = CreateMesh(road, laneCount);
        road.GetComponent<MeshFilter>().mesh = m;
        road.OriginalMesh = Instantiate(m);
        if (laneCount == 1)
        {
            road.Lanes = new Lane[1] {
                new() {
                    Spline = spline,
                    Start = Grid.GetIdByPos(spline.EvaluatePosition(0)),
                    End = Grid.GetIdByPos(spline.EvaluatePosition(1))
                }
            };
        }
        else
        {
            road.Lanes = CreateLanes(spline, laneCount);
        }

        road.LaneNodes = new()
        {
            [start] = new(),
            [end] = new()
        };
        foreach (Lane lane in road.Lanes)
        {
            road.LaneNodes[start].Add(lane.Start);
            road.LaneNodes[end].Add(lane.End);
        }

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

        if (DebugDrawStartEnd)
        {
            Log.ShowTile(start, Color.green, DebugDuration);
            Log.ShowTile(end, Color.red, DebugDuration);
        }

        if (DebugDrawLanes)
        {
            foreach (Lane lane in road.Lanes)
            {
                Log.DrawSpline(lane.Spline, Color.white, DebugDuration);
            }
        }

        road.CrossedTiles = GetCrossedTiles(road);

        // Create graph connection
        PathGraph.Graph.AddVertex(start);
        PathGraph.Graph.AddVertex(end);
        PathGraph.Graph.AddEdge(new TaggedEdge<int, Spline>(start, end, spline));
        // Spline reversed = new();
        // reversed.Copy(spline);
        // SplineUtility.ReverseFlow(reversed);
        // PathGraph.Graph.AddEdge(new TaggedEdge<int, Spline>(end, start, reversed));
        // PathGraph.Print();
        return road;
    }
    Spline CreateSpline(Vector3 posA, Vector3 posB, Vector3 posC, int nodeCount)
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

    HashSet<int> GetCrossedTiles(Road road)
    {
        // Calculate candidate tiles
        Spline spline = road.Spline;
        float length = spline.GetLength();
        int steps = (int)(length / SplineResolution);
        HashSet<int> candidateTiles = new();
        for (int i = 0; i <= steps; i++)
        {
            float3 position = spline.EvaluatePosition(i / (float)steps);
            int candidate = Grid.GetIdByPos(position);
            candidateTiles.Add(candidate);
            if (DebugDrawCrossedTile)
            {
                Log.ShowTile(candidate, Color.cyan, DebugDuration);
            }
        }
        candidateTiles.Remove(road.Start);
        candidateTiles.Remove(road.End);
        return candidateTiles;
    }
    void CheckConnection(int start, int end, out Road connectedRoad, out Lane connectedLane, out bool flow)
    {
        flow = false;
        connectedRoad = null;
        connectedLane = null;
        foreach (var (id, road) in roadDict)
        {
            foreach (Lane lane in road.Lanes)
            {
                if (start == lane.Start)
                {
                    flow = true;
                    connectedRoad = road;
                    connectedLane = lane;
                }
                else if (start == lane.End)
                {
                    flow = true;
                    connectedRoad = road;
                    connectedLane = lane;
                }
                else if (end == lane.Start)
                {
                    flow = false;
                    connectedRoad = road;
                    connectedLane = lane;
                }
                else if (end == lane.End)
                {
                    flow = false;
                    connectedRoad = road;
                    connectedLane = lane;
                }
            }

        }
    }

    void RemoveRoad(Road road)
    {
        DestroyImmediate(road.gameObject);
        roadDict.Remove(road.Id);
    }

    Lane[] CreateLanes(Spline spline, int laneCount)
    {
        Lane[] lanes = new Lane[laneCount];

        for (int i = 0; i < laneCount; i++)
        {
            lanes[i] = new() { Spline = new() };
        }

        int segCount = spline.Knots.Count() - 1;

        // Iterate by distance on curve
        for (int i = 0; i <= segCount; i++)
        {
            spline.Evaluate(1 / (float)segCount * i, out float3 position, out float3 forward, out float3 upVector);
            float3 normal = Vector3.Cross(forward, upVector).normalized;

            // Iterate by each lane
            for (int j = 0; j < laneCount; j++)
            {
                float3 pos = position + normal * LaneWidth * ((float)laneCount / 2 - 0.5f) - j * normal * LaneWidth;
                lanes[j].Spline.Add(new BezierKnot(pos), TangentMode.AutoSmooth);
                if (i == 0)
                {
                    int start = Grid.GetIdByPos(pos);
                    lanes[j].Start = start;
                    if (DebugDrawStartEnd)
                    {
                        Log.ShowTile(start, Color.green, DebugDuration);
                    }
                }
                else if (i == segCount)
                {
                    int end = Grid.GetIdByPos(pos);
                    lanes[j].End = end;
                    if (DebugDrawStartEnd)
                    {
                        Log.ShowTile(end, Color.red, DebugDuration);
                    }
                }
            }
        }
        return lanes;

    }

    void IntersectCheck(Road road, out int newNode, out Road roadToSplit)
    {
        int start = road.Start;
        int end = road.End;
        newNode = -1;
        roadToSplit = null;
        foreach (var (key, _road) in roadDict)
        {
            if (_road.CrossedTiles.Contains(start))
            {
                newNode = BuildManager.start;
                roadToSplit = _road;
                return;
            }
            else if (_road.CrossedTiles.Contains(end))
            {
                newNode = BuildManager.end;
                roadToSplit = _road;
                return;
            }
        }
    }

    Mesh ConnectRoadMesh(int laneCount, Spline spline, Plane guard, float3 leftBound, float3 rightBound)
    {
        int segCount = spline.Knots.Count();
        Vector3 end = spline.EvaluatePosition(1);
        // DebugExtension.DebugPoint(end, Color.magenta, 1, 100);
        List<float3> leftVs = new();
        List<float3> rightVs = new();
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
                // DebugExtension.DebugPoint(left, Color.black, 1, DebugDruation);
                // DebugExtension.DebugPoint(right, Color.black, 1, DebugDruation);
                leftVs.Add(left);
                rightVs.Add(right);
                break;
            }
            // DebugExtension.DebugPoint(left, Color.magenta, 1, DebugDruation);
            // DebugExtension.DebugPoint(right, Color.magenta, 1, DebugDruation);
            leftVs.Add(left);
            rightVs.Add(right);

        }

        leftVs.Reverse();
        rightVs.Reverse();

        return Extrude(leftVs, rightVs);
    }

    void EvaluateMesh(int node)
    {
        List<Road> roads = nodeRoads[node];
        Dictionary<Road, List<int>> d = new();
        List<int> allNodes = null;
        Road mainRoad = null;
        int max = 0;

        // find the main road
        foreach (Road r in roads)
        {
            int size = r.LaneNodes[node].Count();
            if (size > max)
            {
                max = size;
                mainRoad = r;
                allNodes = r.LaneNodes[node];
            }
            d[r] = r.LaneNodes[node];
        }
        d.Remove(mainRoad);

        // check for consecutive neighboring lanes

        List<Lane> lanes = new();
        int start = -1;
        int end = -1;
        bool inGroup = false;
        List<Road> group = new();
        for (int i = 1; i < allNodes.Count(); i++)
        {
            int nodeA = allNodes[i - 1];
            int nodeB = allNodes[i];
            Road roadA = null;
            Road roadB = null;
            foreach (var (r, nodes) in d)
            {
                if (nodes.Contains(nodeA))
                {
                    roadA = r;
                }
                if (nodes.Contains(nodeB))
                {
                    roadB = r;
                }

            }

            if (roadA != roadB && roadA != null && roadB != null)
            {
                inGroup = true;
                Log.Info.Log("Hooray");
                // Log.DrawSpline(roadA.Spline, Color.blue, 100);
                // Log.DrawSpline(roadB.Spline, Color.red, 100);
                if (!lanes.Contains(mainRoad.Lanes[i - 1]))
                {
                    lanes.Add(mainRoad.Lanes[i - 1]);
                }
                lanes.Add(mainRoad.Lanes[i]);
                if (!group.Contains(roadA))
                {
                    group.Add(roadA);
                }
                group.Add(roadB);
                if (start == -1)
                {
                    start = i - 1;
                }
                end = i;
            }
            if ((inGroup && roadB == null) || (i == allNodes.Count() - 1 && inGroup))
            {
                List<float3> intersections = new();

                // find intersection!
                for (int j = 1; j < group.Count(); j++)
                {
                    Road rA = group[j - 1];
                    Road rB = group[j];
                    for (float k = 0; k < 1.5; k += 0.025f)
                    {
                        Spline leftSpline = new();
                        Spline rightSpline = new();
                        foreach (float3 knot in rA.LeftMesh)
                        {
                            leftSpline.Add(new BezierKnot(knot), TangentMode.AutoSmooth);
                        }
                        foreach (float3 knot in rB.RightMesh)
                        {
                            rightSpline.Add(new BezierKnot(knot), TangentMode.AutoSmooth);
                        }
                        float t1 = leftSpline.GetCurveInterpolation(0, k);
                        float t2 = rightSpline.GetCurveInterpolation(0, k);
                        float3 p1 = leftSpline.EvaluatePosition(t1);
                        float3 p2 = rightSpline.EvaluatePosition(t2);
                        // Log.DrawSpline(leftSpline, Color.cyan, 100);
                        // Log.DrawSpline(rightSpline, Color.cyan, 100);
                        // Log.Info.Log(Vector3.Distance(p1, p2));
                        if (Vector3.Distance(p1, p2) > LaneWidth * 2 + 0.5f)
                        {
                            float3 intersection = Vector3.Lerp(p1, p2, 0.5f);
                            intersections.Add(intersection);
                            DebugExtension.DebugPoint(intersection, Color.black, 1, 100);
                            break;
                        }
                    }

                }

                // Draw the holy polygon (v >= 3)
                Mesh m = Instantiate(mainRoad.OriginalMesh);

                float3 leftCorner = mainRoad.LeftMesh.Last();
                float3 rightCorner = mainRoad.RightMesh.Last();
                float3 leftBound = Vector3.Lerp(leftCorner, rightCorner, (float)start / allNodes.Count());
                float3 rightBound = Vector3.Lerp(leftCorner, rightCorner, (float)(end + 1) / allNodes.Count());
                float3 midPoint = Vector3.Lerp(leftBound, rightBound, 0.5f);

                List<Vector3> v = new() { midPoint, leftBound };
                List<int> tri = new();
                List<Vector2> uvs = new() { new(0.5f, 1), new(0, 1) };
                List<Vector3> normals = new() { Vector3.up, Vector3.up };
                int offset = m.vertexCount;

                int index = 1;
                foreach (float3 pos in intersections)
                {
                    v.Add(pos);
                    uvs.Add(new(index * 1 / ((float)intersections.Count() + 1), 1));
                    normals.Add(Vector3.up);
                    index++;
                }

                v.Add(rightBound);
                uvs.Add(new(1, 1));
                normals.Add(Vector3.up);

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

                // Connect road mesh, finally
                float3 upVector = mainRoad.Spline.EvaluateUpVector(1);
                index = 1;
                foreach (var road in group)
                {
                    Plane guard = new(v[index + 1], v[index] - (Vector3)upVector, v[index] + (Vector3)upVector);

                    Spline combinedSpline = new();
                    combinedSpline.Copy(lanes[index - 1].Spline);// Spline of main road lane we are going to connect
                    foreach (BezierKnot knot in road.Spline.Knots)
                    {
                        combinedSpline.Add(knot);
                    }
                    Log.DrawSpline(combinedSpline, Color.cyan, 5);
                    road.GetComponent<MeshFilter>().mesh = ConnectRoadMesh(road.Lanes.Count(), combinedSpline, guard, v[index], v[index + 1]);
                    index++;
                }
                inGroup = false;
                group = new();
            }
            else if (!inGroup & roadA != null || (i == allNodes.Count() - 1 && roadB != null))
            {
                Log.Info.Log("Lone");
                if (i == allNodes.Count() - 1 && roadA == null)
                {
                    roadA = roadB;
                    i++;
                }
                Spline combinedSpline = new();
                combinedSpline.Copy(mainRoad.Lanes[i - 1].Spline);
                foreach (BezierKnot knot in roadA.Lanes[0].Spline)
                {
                    combinedSpline.Add(knot);
                }
                float3 upVector = mainRoad.Spline.EvaluateUpVector(1);
                Plane guard = new(mainRoad.LeftMesh.Last(), mainRoad.LeftMesh.Last() + upVector, mainRoad.RightMesh.Last());

                float3 leftCorner = mainRoad.LeftMesh.Last();
                float3 rightCorner = mainRoad.RightMesh.Last();
                float3 leftBound = Vector3.Lerp(leftCorner, rightCorner, (float)(i - 1) / allNodes.Count());
                float3 rightBound = Vector3.Lerp(leftCorner, rightCorner, (float)i / allNodes.Count());


                roadA.GetComponent<MeshFilter>().mesh = ConnectRoadMesh(roadA.Lanes.Count(), combinedSpline, guard, leftBound, rightBound);
            }
        }
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
            float f1 = 1 / (float) leftVs.Count() * (i - 1);
            float f2 = 1 / (float) leftVs.Count() * i;
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
}
