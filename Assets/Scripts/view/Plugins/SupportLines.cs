using UnityEngine;

public class SupportLines : MonoBehaviour
{
    LineRenderer lineRenderer;
    Vector3[] points = new Vector3[3];
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 3;
    }

    void OnEnable()
    {
        Build.SupportedLineUpdated += DrawSupportLine;
    }

    void OnDisable()
    {
        
        Build.SupportedLineUpdated -= DrawSupportLine;
    }

    void DrawSupportLine(SupportLine supportLine)
    {
        supportLine.ReplaceYCoord(Main.GetHUDObjectHeight(HUDLayer.SupportLines));
        if (!supportLine.Segment1Set)
            lineRenderer.enabled = false;
        else if (supportLine.Segment1Set && !supportLine.Segment2Set)
        {
            lineRenderer.enabled = true;
            points[0] = supportLine.Segment1.start;
            points[2] = supportLine.Segment1.end;
            points[1] = Vector3.Lerp(points[0], points[2], 0.5f);
            lineRenderer.SetPositions(points);
        }
        else
        {
            lineRenderer.enabled = true;
            points[0] = supportLine.Segment1.start;
            points[1] = supportLine.Segment1.end;
            points[2] = supportLine.Segment2.end;
            lineRenderer.SetPositions(points);
        }
    }
}