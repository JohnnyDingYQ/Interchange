using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Zones : MonoBehaviour
{
    [SerializeField]
    GameObject districts;
    [SerializeField]
    ZoneMaterial zoneMaterial;

    private const int MaxRaycastHits = 10;
    private static readonly RaycastHit[] hitResults = new RaycastHit[MaxRaycastHits];

    void Awake()
    {
        uint districtCount = 1;
        foreach (Transform district in districts.transform)
        {
            Transform sourceZones = district.transform.GetChild(0);
            Transform targetZones = district.transform.GetChild(1);
            Transform spline = district.transform.GetChild(2);
            Assert.IsTrue(sourceZones.name.Equals("Source Zones"));
            Assert.IsTrue(targetZones.name.Equals("Target Zones"));
            Assert.IsTrue(spline.name.Equals("Spline"));

            District d = new(districtCount, district.name);
            Game.Districts[districtCount] = d;
            DistrictObject districtObject = district.gameObject.AddComponent<DistrictObject>();
            districtObject.Init(spline.GetComponent<SplineContainer>());
            districtObject.District = d;

            uint zoneCount = 1;
            foreach (Transform sourceZone in sourceZones.transform)
            {
                uint id = (zoneCount++ << (Zone.DistrictBitWidth + 1)) + (districtCount << 1);
                SourceZone newZone = new(id);
                d.SourceZones.Add(newZone);
                Game.SourceZones.Add(id, newZone);
                sourceZone.name = id.ToString();
                ZoneObject zoneObject = sourceZone.gameObject.AddComponent<ZoneObject>();
                zoneObject.Zone = newZone;
                zoneObject.zoneMaterial = zoneMaterial;
            }
            zoneCount = 1;
            foreach (Transform targetZone in targetZones.transform)
            {
                uint id = (zoneCount++ << (Zone.DistrictBitWidth + 1)) + (districtCount << 1) + 1;
                TargetZone newZone = new(id);
                d.TargetZones.Add(newZone);
                Game.TargetZones.Add(id, newZone);
                targetZone.name = id.ToString();
                ZoneObject zoneObject = targetZone.gameObject.AddComponent<ZoneObject>();
                zoneObject.Zone = newZone;
                zoneObject.zoneMaterial = zoneMaterial;
            }
            d.Disable();
            districtCount++;
        }
    }

    public static void UpdateHoveredZoneAndDistrict()
    {
        float3 mousePos = InputSystem.MouseWorldPos;
        mousePos.y = Constants.MinElevation + 1;

        // Perform the raycast and store the number of hits
        int hitCount = Physics.RaycastNonAlloc(new Ray(mousePos, new float3(0, -1, 0)), hitResults, 10);

        Game.HoveredZone = null;
        Game.HoveredDistrict = null;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hitResults[i];
            if (hit.collider.gameObject.TryGetComponent<ZoneObject>(out var zoneObject))
                Game.HoveredZone = zoneObject.Zone;
            if (hit.collider.gameObject.TryGetComponent<DistrictObject>(out var districtObject))
                Game.HoveredDistrict = districtObject.District;
        }
    }
}