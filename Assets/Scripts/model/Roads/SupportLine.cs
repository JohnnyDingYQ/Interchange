using UnityEngine;

public class SupportLine
{
    public Line Segment1;
    public Line Segment2;
    public bool Segment1Set;
    public bool Segment2Set;

    public void ReplaceYCoord(float y)
    {
        Segment1.start.y = y;
        Segment1.end.y = y;
        Segment2.start.y = y;
        Segment2.end.y = y;
    }
}

public struct Line
{
    public Vector3 start;
    public Vector3 end;

    public Line(Vector3 s, Vector3 e)
    {
        start = s;
        end = e;
    }
}