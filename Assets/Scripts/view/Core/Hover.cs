using Unity.Mathematics;
using UnityEngine;

public class Hover
{
    private const int MaxRaycastHits = 10;
    private static readonly RaycastHit[] hitResults = new RaycastHit[MaxRaycastHits];

    public static void UpdateHovered()
    {
        float3 mousePos = InputSystem.MouseWorldPos;
        mousePos.y = Constants.MaxElevation + 1;

        // Perform the raycast and store the number of hits
        int hitCount = Physics.RaycastNonAlloc(new Ray(mousePos, new float3(0, -1, 0)), hitResults, Constants.MaxElevation + 3);

        if (Game.HoveredRoad != null)
            Roads.Unhighlight(Game.HoveredRoad);
        Game.HoveredRoad = null;
        bool roadFound = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hitResults[i];
            if (!Build.StartAssigned() && !roadFound && hit.collider.gameObject.TryGetComponent<RoadObject>(out var roadComp))
                if (!roadComp.Road.IsGhost)
                {
                    Game.HoveredRoad = roadComp.Road;
                    Roads.Highlight(Game.HoveredRoad);
                    roadFound = true;
                }

        }
    }
}