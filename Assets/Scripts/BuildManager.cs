using System;
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class BuildManager : MonoBehaviour
{
    private static int start, pivot, end;
    [SerializeField] private const float SplineResolution = 0.4f;
    [SerializeField] private const float LaneWidth = 2.5f;
    [SerializeField] private GameObject roads;

    [SerializeField] private Road roadPreFab;
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
        DebugDrawLanes = false;
        DebugDrawMeshVertices = false;
        DebugDuration = 5;
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
                    IntersectCheck(road, out int newNode, out Road roadToSplit);
                    if (newNode != -1)
                    {

                    }
                }

                start = -1;
                pivot = -1;
            }
        }
    }
    Road InitiateRoad(Spline spline, int start, int end, int laneCount)
    {
        Road road = Instantiate(roadPreFab, roads.transform, true);
        road.Spline = spline;
        road.GetComponent<MeshFilter>().mesh = CreateMesh(road, laneCount);
        road.name = $"Road-{nextAvailableId}";
        road.Start = start;
        road.End = end;
        road.Id = nextAvailableId++;
        roadDict.Add(road.Id, road); // BuildManager now watches this road

        if (laneCount == 1)
        {
            road.Lanes = new Lane[1] {
                new() {
                    Spline = new(),
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
                Log.DrawSpline(lane.Spline, DebugDuration);
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
            left = position + normal * LaneWidth * laneCount;
            right = position - normal * LaneWidth * laneCount;
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

        Mesh m = new();
        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector2> uvs = new();
        List<Vector3> normals = new();
        int offset;
        for (int i = 1; i <= segCount; i++)
        {
            Vector3 p1 = leftVs[i - 1];
            Vector3 p2 = rightVs[i - 1];
            Vector3 p3 = leftVs[i];
            Vector3 p4 = rightVs[i];

            offset = 4 * (i - 1);
            int t1 = offset + 0;
            int t2 = offset + 2;
            int t3 = offset + 3;

            // Debug.DrawLine(p1, p2, Color.green, 1);
            // Debug.DrawLine(p3, p4, Color.green, 1);
            // Debug.DrawLine(p4, p1, Color.green, 1);

            int t4 = offset + 3;
            int t5 = offset + 1;
            int t6 = offset + 0;
            float f1 = 1 / (float)segCount * (i - 1);
            // Debug.Log(f1);
            float f2 = 1 / (float)segCount * i;
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
        for (int i = 0; i <= segCount; i++)
        {
            spline.Evaluate(1 / (float)segCount * i, out float3 position, out float3 forward, out float3 upVector);
            float3 normal = Vector3.Cross(forward, upVector).normalized;
            for (int j = 0; j < laneCount; j++)
            {
                float3 pos = position + normal * LaneWidth - j * 2 * normal * LaneWidth;
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

    Mesh ConnectRoadMesh(int laneCount, Spline spline, Plane guard, float3 intersection, bool isLeft, Vector3 bound)
    {
        int segCount = spline.Knots.Count();
        Vector3 end = spline.EvaluatePosition(1);
        DebugExtension.DebugPoint(end, Color.magenta, 1, 100);
        List<float3> leftVs = new();
        List<float3> rightVs = new();
        for (int i = segCount; i >= 0; i--)
        {
            spline.Evaluate(1 / (float)segCount * i, out float3 position, out float3 forward, out float3 upVector);
            float3 normal = Vector3.Cross(forward, upVector).normalized;
            float3 left = position + normal * LaneWidth * laneCount;
            float3 right = position - normal * LaneWidth * laneCount;
            bool leftOverlap = false;
            bool rightOverlap = false;
            if (!guard.SameSide(end, left))
            {
                if (!isLeft)
                {
                    left = intersection;
                }
                else
                {
                    left = guard.ClosestPointOnPlane(left);
                }

                leftOverlap = true;
            }
            else if (!isLeft && Vector3.Distance(left, intersection) < 1.5)
            {
                left = intersection;
            }

            if (!guard.SameSide(end, right))
            {
                if (isLeft)
                {
                    right = intersection;

                }
                else
                {
                    right = guard.ClosestPointOnPlane(right);
                }

                rightOverlap = true;
            }
            else if (isLeft && Vector3.Distance(right, intersection) < 1.5)
            {
                right = intersection;
            }
            if (leftOverlap && rightOverlap)
            {
                if (isLeft)
                {
                    right = intersection;
                    left = bound;
                }
                else
                {
                    left = intersection;
                    right = bound;
                }

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
            float f1 = 1 / (float)segCount * (i - 1);
            // Debug.Log(f1);
            float f2 = 1 / (float)segCount * i;
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

    void EvaluateMesh(int node)
    {
        List<Road> roads = nodeRoads[node];
        Dictionary<Road, List<int>> d = new();
        List<int> allNodes = null;
        Road mainRoad = null;
        int max = 0;

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
                Log.Info.Log("Hooray");
                float3 intersection = new();
                // find intersection!
                for (float j = 0; j < 1.5; j += 0.025f)
                {
                    Spline leftSpline = new();
                    Spline rightSpline = new();
                    foreach (float3 k in roadA.LeftMesh)
                    {
                        leftSpline.Add(new BezierKnot(k), TangentMode.AutoSmooth);
                    }
                    foreach (float3 k in roadB.RightMesh)
                    {
                        rightSpline.Add(new BezierKnot(k), TangentMode.AutoSmooth);
                    }
                    float t1 = leftSpline.GetCurveInterpolation(0, j);
                    float t2 = rightSpline.GetCurveInterpolation(0, j);
                    float3 p1 = leftSpline.EvaluatePosition(t1);
                    float3 p2 = rightSpline.EvaluatePosition(t2);
                    // Log.Info.Log(Vector3.Distance(p1, p2));
                    if (Vector3.Distance(p1, p2) > LaneWidth * 4 + 0.5f)
                    {
                        intersection = Vector3.Lerp(p1, p2, 0.5f);
                        DebugExtension.DebugPoint(intersection, Color.magenta, 1, 100);
                        break;
                    }
                }
                // Draw the holy triangle
                Mesh m = mainRoad.GetComponent<MeshFilter>().mesh;
                Vector3[] v = new Vector3[] { mainRoad.LeftMesh.Last(), intersection, mainRoad.RightMesh.Last() };
                int[] tri = new int[] { m.vertexCount, m.vertexCount + 1, m.vertexCount + 2 };

                List<Vector3> mv = new();
                mv.AddRange(m.vertices);
                mv.AddRange(v);

                List<int> mtri = new();
                mtri.AddRange(m.triangles);
                mtri.AddRange(tri);

                List<Vector2> muv = new();
                muv.AddRange(m.uv);
                muv.AddRange(new Vector2[] { new(0, 1), new(0.5f, 1), new(1, 1) });

                List<Vector3> mn = new();
                mn.AddRange(m.normals);
                mn.AddRange(new Vector3[] { Vector3.up, Vector3.up, Vector3.up });

                m.SetVertices(mv);
                m.SetUVs(0, muv);
                m.SetTriangles(mtri, 0);
                m.SetNormals(mn);

                mainRoad.GetComponent<MeshFilter>().mesh = m;

                mainRoad.Spline.Evaluate(1, out float3 position, out float3 forward, out float3 upVector);
                Plane guard = new(mainRoad.LeftMesh.Last(), intersection - upVector, intersection + upVector);

                Spline combinedSpline = mainRoad.GetLaneByNode(nodeA).Spline;
                foreach (BezierKnot knot in roadA.Spline.Knots)
                {
                    combinedSpline.Add(knot);
                }
                roadA.GetComponent<MeshFilter>().mesh = ConnectRoadMesh(roadA.Lanes.Count(), combinedSpline, guard, intersection, true, mainRoad.LeftMesh.Last());

                mainRoad.Spline.Evaluate(1, out position, out forward, out upVector);
                guard = new(mainRoad.RightMesh.Last(), intersection - upVector, intersection + upVector);

                combinedSpline = mainRoad.GetLaneByNode(nodeB).Spline;
                foreach (BezierKnot knot in roadB.Spline.Knots)
                {
                    combinedSpline.Add(knot);
                }
                roadB.GetComponent<MeshFilter>().mesh = ConnectRoadMesh(roadB.Lanes.Count(), combinedSpline, guard, intersection, false, mainRoad.RightMesh.Last());

            }
        }


    }
}
