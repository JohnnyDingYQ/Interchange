using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    public static bool DrawCenter { get; set; }
    public static bool DrawLanes { get; set; }
    public static bool DrawEdges { get; set; }
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
        // return;
        if (DrawCenter)
            Gizmos.DrawRoadCenter(DrawDuration);
        if (DrawEdges)
            Gizmos.DrawEdges(DrawDuration);
        if (DrawLanes)
            Gizmos.DrawLanes(DrawDuration);
        if (DrawPx)
            Gizmos.DrawControlPoints(DrawDuration);
        if (DrawVertices)
            Gizmos.DrawVertices(DrawDuration);
        if (DrawOutline)
            Gizmos.DrawOutline(DrawDuration);
        
    }
}