using System.Collections.Generic;

public class Zone
{
    public int Id { get; set; }
    public string Name { get; set; }
    public SortedDictionary<int, int> Demands { get; set; }
    // public HashSet<Road> InRoads { get; set; }
    // public HashSet<Road> OutRoads { get; set; }
    private List<Vertex> InVertices { get; set; }
    private List<Vertex> OutVertices { get; set; }
    public int InVerticesCount { get { return InVertices.Count; } }
    public int OutVerticesCount { get { return OutVertices.Count; } }

    public Zone(int id)
    {
        Id = id;
        // InRoads = new();
        // OutRoads = new();
        InVertices = new();
        OutVertices = new();
        Demands = new();
    }
    public void AddInRoad(Road road)
    {
        foreach (Lane lane in road.Lanes)
            InVertices.Add(lane.EndVertex);
    }

    public void AddOutRoad(Road road)
    {
        foreach (Lane lane in road.Lanes)
            OutVertices.Add(lane.StartVertex);
    }

    public Vertex GetRandomInVertex()
    {
        if (InVertices.Count == 0)
            return null;
        int randomIndex = (int) (UnityEngine.Random.value * InVertices.Count);
        if (randomIndex == InVertices.Count) // technically possible
            return GetRandomInVertex();
        return InVertices[randomIndex];
    }

    public Vertex GetRandomOutVertex()
    {
        if (OutVertices.Count == 0)
            return null;
        int randomIndex = (int) (UnityEngine.Random.value * OutVertices.Count);
        if (randomIndex == OutVertices.Count)
            return GetRandomOutVertex();
        return OutVertices[randomIndex];
    }

    public void RemoveRoad(Road road)
    {
        foreach (Lane lane in road.Lanes)
            foreach (Vertex v in new Vertex[] {lane.StartVertex, lane.EndVertex})
            {
                InVertices.Remove(v);
                OutVertices.Remove(v);
            }
    }
}