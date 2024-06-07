using System.Collections.Generic;

public class Zone
{
    public int Id { get; set; }
    public string Name { get; set; }
    public SortedDictionary<int, int> Demands { get; set; }
    private List<Vertex> InVertices { get; set; }
    private List<Vertex> OutVertices { get; set; }
    public int InVerticesCount { get { return InVertices.Count; } }
    public int OutVerticesCount { get { return OutVertices.Count; } }
    public float CarSpawnInterval { get; set; }

    public Zone(int id)
    {
        Id = id;
        InVertices = new();
        OutVertices = new();
        Demands = new();
        CarSpawnInterval = 0;
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
        return InVertices[MyNumerics.GetRandomIndex(InVertices.Count)];
    }

    public Vertex GetRandomOutVertex()
    {
        if (OutVertices.Count == 0)
            return null;
        return OutVertices[MyNumerics.GetRandomIndex(OutVertices.Count)];
    }

    public void RemoveRoad(Road road)
    {
        foreach (Lane lane in road.Lanes)
            foreach (Vertex v in new Vertex[] { lane.StartVertex, lane.EndVertex })
            {
                InVertices.Remove(v);
                OutVertices.Remove(v);
            }
    }
}