public class Point
{
    public uint Id { get; set; }
    public Node Node { get; set; }
    public uint Node_ { get; set; }

    public Point(uint id)
    {
        Id = id;
    }
}