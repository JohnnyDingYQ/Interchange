using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class Zones : MonoBehaviour
{
    [SerializeField]
    GameObject districts;

    private const int MaxRaycastHits = 10;
    private static readonly RaycastHit[] hitResults = new RaycastHit[MaxRaycastHits];

    void Awake()
    {
        uint districtCount = 1;
        foreach (Transform district in districts.transform)
        {
            // Debug.Log(district.name);
            // Debug.Log(districtCount);
            Transform sourceZones = district.transform.GetChild(0);
            Transform targetZones = district.transform.GetChild(1);
            Assert.IsTrue(sourceZones.name.Equals("Source Zones"));
            Assert.IsTrue(targetZones.name.Equals("Target Zones"));
            uint zoneCount = 1;
            foreach (Transform sourceZone in sourceZones.transform)
            {
                uint id = (zoneCount++ << 7) + (districtCount << 1);
                Game.SourceZones.Add(id, new(id));
                sourceZone.name = id.ToString();
            }
            zoneCount = 1;
            foreach (Transform targetZone in targetZones.transform)
            {
                uint id = (zoneCount++ << 7) + (districtCount << 1) + 1;
                Game.TargetZones.Add(id, new(id));
                targetZone.name = id.ToString();
            }
            districtCount++;
        }
    }
    
    public static void UpdateHoveredZone()
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