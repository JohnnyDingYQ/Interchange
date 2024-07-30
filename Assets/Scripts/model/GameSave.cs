using System.Collections.Generic;
using UnityEngine;
using QuikGraph;

public class GameSave : IPersistable
{
    public readonly float versionNumber = 0.1f;
    public uint CarServiced { get; set; }
    [IPersistableDict(typeof(Node))]
    public Dictionary<uint, Node> Nodes { get; private set; }
    [IPersistableDict(typeof(Road))]
    public Dictionary<uint, Road> Roads { get; private set; }
    [IPersistableDict(typeof(Intersection))]
    public Dictionary<uint, Intersection> Intersections { get; private set; }
    [IPersistableDict(typeof(Lane))]
    public Dictionary<uint, Lane> Lanes { get; private set; }
    [IPersistableDict(typeof(Vertex))]
    public Dictionary<uint, Vertex> Vertices { get; private set; }
    [IPersistableDict(typeof(Path))]
    public Dictionary<uint, Path> Paths { get; private set; }
    [IPersistableDict(typeof(Curve))]
    public Dictionary<uint, Curve> Curves { get; private set; }
    [NotSaved]
    public Dictionary<uint, Car> Cars { get; private set; }
    public uint Id { get; set; }

    public GameSave()
    {
        Vertices = new();
        Paths = new();
        Lanes = new();
        Roads = new();
        Nodes = new();
        Intersections = new();
        Cars = new();
        Curves = new();
        CarServiced = 0;
    }

    // public void Load(Reader reader)
    // {
    //     versionNumber = reader.ReadFloat();
    //     CarServiced = reader.ReadUint();

    //     ReadCollection(reader, Nodes);
    //     ReadCollection(reader, Roads);
    //     ReadCollection(reader, Intersections);
    //     ReadCollection(reader, Lanes);
    //     ReadCollection(reader, Vertices);
    //     ReadCollection(reader, Paths);
    //     ReadCollection(reader, Curves);

    //     RestoreObjectReferences();

    //     static void ReadCollection<T>(Reader reader, Dictionary<uint, T> dict)
    //         where T: IPersistable, new()
    //     {
    //         dict.Clear();
    //         int count = reader.ReadInt();
    //         for (int i = 0; i < count; i++)
    //         {
    //             T restored = new();
    //             restored.Load(reader);
    //             dict.Add(restored.Id, restored);
    //         }
    //     }

    //     void RestoreObjectReferences()
    //     {
    //         foreach (Node node in Nodes.Values)
    //         {
    //             if (node.InLane != null)
    //                 node.InLane = Lanes[node.InLane.Id];
    //             if (node.OutLane != null)
    //                 node.OutLane = Lanes[node.OutLane.Id];
    //             node.Intersection = Intersections[node.Intersection.Id];
    //         }

    //         foreach (Road road in Roads.Values)
    //         {
    //             road.Curve = Curves[road.Curve.Id];
    //             for (int i = 0; i < road.Lanes.Count; i++)
    //                 road.Lanes[i] = Lanes[road.Lanes[i].Id];
    //             road.StartIntersection = Intersections[road.StartIntersection.Id];
    //             road.EndIntersection = Intersections[road.EndIntersection.Id];
    //         }

    //         foreach (Intersection ix in Intersections.Values)
    //         {
    //             List<Node> nodes = new();
    //             for (int i = 0; i < ix.Nodes.Count; i++)
    //                 nodes.Add(Nodes[ix.Nodes[i].Id]);
    //             ix.SetNodes(nodes);
    //             HashSet<Road> inRoad = new();
    //             foreach (Road road in ix.InRoads)
    //                 inRoad.Add(Roads[road.Id]);
    //             HashSet<Road> outRoad = new();
    //             foreach (Road road in ix.OutRoads)
    //                 outRoad.Add(Roads[road.Id]);
    //             ix.SetInRoads(inRoad);
    //             ix.SetOutRoads(outRoad);
    //         }

    //         foreach (Lane lane in Lanes.Values)
    //         {
    //             lane.Curve = Curves[lane.Curve.Id];
    //             lane.StartVertex = Vertices[lane.StartVertex.Id];
    //             lane.EndVertex = Vertices[lane.EndVertex.Id];
    //             lane.StartNode = Nodes[lane.StartNode.Id];
    //             lane.EndNode = Nodes[lane.EndNode.Id];
    //             lane.Road = Roads[lane.Road.Id];
    //             lane.InnerPath = Paths[lane.InnerPath.Id];
    //         }
            
    //         foreach (Vertex vertex in Vertices.Values)
    //             vertex.Lane = Lanes[vertex.Lane.Id];

    //         foreach (Path path in Paths.Values)
    //         {
    //             path.Curve = Curves[path.Curve.Id];
    //             path.Source = Vertices[path.Source.Id];
    //             path.Target = Vertices[path.Target.Id];
    //             if (path.InterweavingPath != null)
    //                 path.InterweavingPath = Paths[path.InterweavingPath.Id];
    //         }

    //         foreach (Curve curve in Curves.Values)
    //             if (curve.nextCurve != null)
    //                 curve.nextCurve = Curves[curve.nextCurve.Id]; 
    //     }
    // }
}