using System.Collections.Generic;

public interface IZone
{
    public int Id { get; set; }
    public string Name { get; set; }
    // public HashSet<Road> InRoads { get; set; }
    // public HashSet<Road> OutRoads { get; set; }
    public int InVerticesCount {get;}
    public int OutVerticesCount {get;}
    public SortedDictionary<int, int> Demands { get; set; }
    public void AddInRoad(Road road);
    public void AddOutRoad(Road road);
    public void RemoveRoad(Road road);
    public Vertex GetRandomInVertex();
    public Vertex GetRandomOutVertex();
}