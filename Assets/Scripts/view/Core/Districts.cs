using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Districts : MonoBehaviour
{
    [SerializeField]
    GameObject districts;
    [SerializeField]
    ZoneColor zoneColor;
    [SerializeField]
    Material zoneMaterial;
    [SerializeField]
    GameUI gameUI;
    readonly Dictionary<uint, ZoneObject> zoneMapping = new();
    readonly Dictionary<uint, DistrictObject> districtMapping = new();

    void Start()
    {
        InitZoneAndDistricts();
    }

    public void InitZoneAndDistricts()
    {
        uint districtCount = 1;
        foreach (Transform districtTransform in districts.transform)
        {
            if (!districtTransform.gameObject.activeSelf)
                continue;

            District newDistrict = new(districtCount, districtTransform.name);
            Game.Districts[newDistrict.Id] = newDistrict;
            DistrictObject districtObject = districtTransform.gameObject.AddComponent<DistrictObject>();
            districtObject.Init(gameUI.AddDistrictLabel());
            DebugExtension.DebugPoint(districtObject.Center, Color.black, 50, 10000);
            districtObject.District = newDistrict;
            districtMapping[newDistrict.Id] = districtObject;

            uint zoneCount = 1;
            foreach (Transform zone in districtTransform.transform)
            {
                uint id = (zoneCount++ << (Zone.DistrictBitWidth)) + districtCount;
                Zone newZone = new(id);
                newDistrict.Zones.Add(newZone);
                Game.Zones.Add(id, newZone);
                InitZoneObject(zone.gameObject, newZone);
            }
            newDistrict.Disable();
            districtCount++;
        }
        Game.SetupZones();
        
        void InitZoneObject(GameObject gameObject, Zone newZone)
        {
            ZoneObject zoneObject = gameObject.AddComponent<ZoneObject>();
            gameObject.name = newZone.Id.ToString();
            zoneObject.Zone = newZone;
            zoneObject.zoneColor = zoneColor;
            zoneMapping[newZone.Id] = zoneObject;
            zoneObject.Init(gameObject.GetComponent<SplineContainer>());
            zoneObject.meshRenderer.material = zoneMaterial;
        }

    }

    public void UpdateZoneObjectReferences()
    {
        foreach (ZoneObject zoneObject in zoneMapping.Values)
        {
            if (zoneObject.Zone is Zone)
                zoneObject.Zone = Game.Zones[zoneObject.Zone.Id];
            else
                zoneObject.Zone = Game.Zones[zoneObject.Zone.Id];
        }

        foreach (DistrictObject districtObject in districtMapping.Values)
        {
            districtObject.District = Game.Districts[districtObject.District.Id];
        }
    }
}