using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class BuildManager : MonoBehaviour
{
    private static int A, B, C;
    [SerializeField] private const float SplineResolution = 0.2f;
    [SerializeField] private const float LaneWidth = 2.5f;
    [SerializeField] private GameObject roads;

    [SerializeField] private Road roadPreFab;
    private int nextAvailableId;
    private Dictionary<int, Road> roadDict = new();
    private bool DebugShowStartEnd;
    private bool DebugShowCandidateTile;
    private bool DebugDrawCenter;
    private bool DebugDrawLanes;
    private int DebugDuration;

    // Start is called before the first frame update
    void Start()
    {
        A = -1;
        B = -1;
        DebugShowStartEnd = true;
        DebugShowCandidateTile = false;
        DebugDrawCenter = true;
        DebugDrawLanes = true;
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
            if (A == -1)
            {
                A = hoveredTile;
                Log.Info.Log($"Road Manager: Tile A loaded");
            }
            else if (B == -1)
            {
                B = hoveredTile;
                Log.Info.Log($"Road Manager: Tile B loaded");
            }
            else
            {
                C = hoveredTile;
                Log.Info.Log($"Road Manager: Tile C loaded");
                Vector3 posA = Grid.GetWorldPosByID(A);
                Vector3 posB = Grid.GetWorldPosByID(B);
                Vector3 posC = Grid.GetWorldPosByID(C);

                float linearLength = Vector3.Distance(posA, posB) + Vector3.Distance(posB, posC);
                int segCount = (int)(linearLength * SplineResolution + 1.0);
                


                CheckConnection(A, C, out Lane otherLane, out bool flow);
                if (otherLane != null)
                {
                    Spline spline = CreateSpline(otherLane.Spline.EvaluatePosition(1) , posB, posC, segCount);
                    Log.Info.Log("Road Manager: Connecting Roads");
                    int[] start = new int[] { A };

                    otherLane.Spline.Evaluate(1, out float3 position, out float3 forward, out float3 upVector);
                    float3 normal = Vector3.Cross(forward, upVector).normalized;
                    float3 left = position + normal;
                    float3 right = position - normal;
                    Plane guard = new(left, right, position + upVector);

                    if (flow)
                    {
                        int index = 0;
                        foreach (BezierKnot knot in otherLane.Spline.Knots)
                        {
                            spline.Insert(index++, knot);
                        }
                    }
                    else
                    {
                        foreach (BezierKnot knot in otherLane.Spline.Knots)
                        {
                            spline.Add(knot);
                        }
                    }

                    int count = 1;
                    IEnumerable<BezierKnot> k = spline.Knots;
                    float3 d = new(0, 0.1f, 0);
                    while (count < k.Count())
                    {
                        Debug.DrawLine(k.ElementAt(count).Position + d, k.ElementAt(count - 1).Position + d, Color.white, DebugDuration);
                        count += 1;
                    }

                    Debug.DrawLine(left, right, Color.cyan, 100);
                    Debug.DrawLine(left, position + upVector, Color.cyan, 100);
                    Debug.DrawLine(right, position + upVector, Color.cyan, 100);

                    Road r = InitiateRoad(spline, A, C, Main.BuildMode);
                    r.GetComponent<MeshFilter>().mesh = ConnectRoadMesh(start, spline, guard);
                }
                else
                {
                    Spline spline = CreateSpline(posA, posB, posC, segCount);
                    Road road = InitiateRoad(spline, A, C, Main.BuildMode);
                    IntersectCheck(road, out int newNode, out Road roadToSplit);
                    if (newNode != -1)
                    {

                    }
                }

                A = -1;
                B = -1;
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

        road.Lanes = CreateLanes(spline, laneCount);

        if (DebugShowStartEnd)
        {
            Log.ShowTile(start, Color.green, DebugDuration);
            Log.ShowTile(end, Color.red, DebugDuration);
        }

        if (DebugDrawLanes)
        {
            foreach (Lane lane in road.Lanes)
            {
                int count = 1;
                IEnumerable<BezierKnot> k = lane.Spline.Knots;
                float3 d = new(0, 0.1f, 0);
                while (count < k.Count())
                {
                    Debug.DrawLine(k.ElementAt(count).Position + d, k.ElementAt(count - 1).Position + d, Color.white, DebugDuration);
                    count += 1;
                }
            }
        }

        road.CrossedTiles = GetCrossedTiles(road);

        // Create graph connection
        PathGraph.Graph.AddVertex(start);
        PathGraph.Graph.AddVertex(end);
        PathGraph.Graph.AddEdge(new TaggedEdge<int, Spline>(start, end, spline));
        Spline reversed = new();
        reversed.Copy(spline);
        SplineUtility.ReverseFlow(reversed);
        PathGraph.Graph.AddEdge(new TaggedEdge<int, Spline>(end, start, reversed));
        // PathGraph.Print();
        return road;
    }
    Spline CreateSpline(Vector3 posA, Vector3 posB, Vector3 posC, int nodeCount)
    {

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
            Debug.DrawLine(left, right, Color.white, 2);
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
            if (DebugShowCandidateTile)
            {
                Log.ShowTile(candidate, Color.cyan, DebugDuration);
            }
        }
        candidateTiles.Remove(road.Start);
        candidateTiles.Remove(road.End);
        return candidateTiles;
    }
    void CheckConnection(int start, int end, out Lane connectedLane, out bool flow)
    {
        flow = false;
        connectedLane = null;
        foreach (var (id, road) in roadDict)
        {
            foreach (Lane lane in road.Lanes)
            {
                if (start == lane.Start)
                {
                    flow = true;
                    connectedLane = lane;
                }
                else if (start == lane.End)
                {
                    flow = true;
                    connectedLane = lane;
                }
                else if (end == lane.Start)
                {
                    flow = false;
                    connectedLane = lane;
                }
                else if (end == lane.End)
                {
                    flow = false;
                    connectedLane = lane;
                }
            }

        }
    }

    Mesh ConnectRoadMesh(int[] start, Spline spline, Plane guard)
    {
        int laneCount = start.Count();
        Lane[] lanes = CreateLanes(spline, laneCount);

        // justify branch starting positions
        for (int i = 0; i < laneCount; i++)
        {
            lanes[i].Start = start[i];
        }

        int segCount = spline.Knots.Count()*2;
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
                left = guard.ClosestPointOnPlane(left);
                leftOverlap = true;
            }
            if (!guard.SameSide(end, right))
            {
                right = guard.ClosestPointOnPlane(right);
                rightOverlap = true;
            }
            if (leftOverlap && rightOverlap)
            {
                left = guard.ClosestPointOnPlane(left);
                right = guard.ClosestPointOnPlane(right);
                DebugExtension.DebugPoint(left, Color.magenta, 1, 100);
                DebugExtension.DebugPoint(right, Color.magenta, 1, 100);
                leftVs.Add(left);
                rightVs.Add(right);
                Debug.DrawLine(left, right, Color.white, 2);
                break;
            }
            DebugExtension.DebugPoint(left, Color.magenta, 1, 100);
            DebugExtension.DebugPoint(right, Color.magenta, 1, 100);
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

            // Debug.DrawLine(p1, p2, Color.green, 100);
            // Debug.DrawLine(p3, p4, Color.green, 100);
            // Debug.DrawLine(p4, p1, Color.green, 100);

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

    void RemoveRoad(Road road)
    {
        DestroyImmediate(road.gameObject);
        roadDict.Remove(road.Id);
    }

    Lane[] CreateLanes(Spline spline, int laneCount)
    {
        if (laneCount == 1)
        {
            return new Lane[1] { new() { Spline = new() } };
        }

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
                float3 pos = position - normal * LaneWidth + j * 2 * normal * LaneWidth;
                lanes[j].Spline.Add(new BezierKnot(pos), TangentMode.AutoSmooth);
                if (i == 0)
                {
                    int start = Grid.GetIdByPos(pos);
                    lanes[j].Start = start;
                    if (DebugShowStartEnd)
                    {
                        Log.ShowTile(start, Color.green, DebugDuration);
                    }
                }
                else if (i == segCount)
                {
                    int end = Grid.GetIdByPos(pos);
                    lanes[j].End = end;
                    if (DebugShowStartEnd)
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
                newNode = A;
                roadToSplit = _road;
                return;
            }
            else if (_road.CrossedTiles.Contains(end))
            {
                newNode = C;
                roadToSplit = _road;
                return;
            }
        }
    }

}
