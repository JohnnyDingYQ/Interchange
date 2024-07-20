using Unity.Mathematics;
using UnityEngine;

public class Zones : MonoBehaviour
{
    [SerializeField]
    GameObject sourceZones;
    [SerializeField]
    GameObject targetZones;

    private const int MaxRaycastHits = 10;
    private static readonly RaycastHit[] hitResults = new RaycastHit[MaxRaycastHits];

    void Awake()
    {
        Game.SourceZones ??= new();
        Game.TargetZones ??= new();

        foreach (Transform child in sourceZones.transform)
        {
            uint id = uint.Parse(child.gameObject.name);
            Game.SourceZones.Add(id, new(id));
        }

        foreach (Transform child in targetZones.transform)
        {
            uint id = uint.Parse(child.gameObject.name);
            Game.TargetZones.Add(id, new(id));
        }
    }

    void Update()
    {
        UpdateHoveredZone();

        DemandsGenerator.GenerateDemands(Time.deltaTime);
        DemandsSatisfer.SatisfyDemands(Time.deltaTime);
    
        // foreach (Zone zone in Game.SourceZones.Values)
        //     Debug.Log(zone.Vertices.Count);
    }

    void UpdateHoveredZone()
    {
        float3 mousePos = InputSystem.MouseWorldPos;
        mousePos.y = Constants.MinElevation + 1;

        // Perform the raycast and store the number of hits
        int hitCount = Physics.RaycastNonAlloc(new Ray(mousePos, new float3(0, -1, 0)), hitResults, 10);

        Game.HoveredZone = null;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hitResults[i];
            Transform parent = hit.collider.gameObject.transform.parent;
            if (parent == null)
                continue;
            if (parent.gameObject.name == "Source Zones")
            {
                Game.HoveredZone = Game.SourceZones[uint.Parse(hit.collider.gameObject.name)];
                return;
            }
            if (parent.gameObject.name == "Target Zones")
            {
                Game.HoveredZone = Game.TargetZones[uint.Parse(hit.collider.gameObject.name)];
                return;
            }
            
        }
    }
}