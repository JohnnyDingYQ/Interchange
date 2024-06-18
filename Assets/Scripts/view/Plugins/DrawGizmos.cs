using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    public static bool DrawCenter { get; set; }
    public static bool DrawLanes { get; set; }
    public static bool DrawPaths { get; set; }
    public static bool DrawOutline { get; set; }
    public static bool DrawPx { get; set; }
    public static bool DrawVertices { get; set; }
    public static bool DrawSupportLines { get; set; }

    private const float DrawDuration = 1f;

    void Start()
    {
        InvokeRepeating("Draw", 0f, DrawDuration);
    }

    void Draw()
    {
        if (DrawCenter)
            Gizmos.DrawRoadCenter(DrawDuration);
        if (DrawPaths)
            Gizmos.DrawPaths(DrawDuration);
        if (DrawLanes)
            Gizmos.DrawLanes(DrawDuration);
        if (DrawPx)
            Gizmos.DrawControlPoints(DrawDuration);
        if (DrawVertices)
            Gizmos.DrawVertices(DrawDuration);
        if (DrawOutline)
            Gizmos.DrawOutline(DrawDuration);
        if (DrawSupportLines)
            Gizmos.DrawSupportLine(DrawDuration);
        
    }
}