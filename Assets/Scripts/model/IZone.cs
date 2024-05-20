using System.Collections.Generic;

public interface IZone
{
    public int Id { get; set; }
    public string Name { get; set; }
    public HashSet<Road> InRoads { get; set; }
    public HashSet<Road> OutRoads { get; set; }
    public SortedDictionary<int, int> Demands { get; set; }
}