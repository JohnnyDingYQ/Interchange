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
    ZoneMaterial zoneMaterial;
    readonly Dictionary<uint, ZoneObject> zoneMapping = new();

    void Awake()
    {
        InitZoneAndDistricts();
    }

    public void InitZoneAndDistricts()
    {
        uint districtCount = 1;
        foreach (Transform districtTransform in districts.transform)
        {
            Transform sourceZones = districtTransform.transform.GetChild(0);
            Transform targetZones = districtTransform.transform.GetChild(1);
            Transform spline = districtTransform.transform.GetChild(2);
            Assert.IsTrue(sourceZones.name.Equals("Source Zones"));
            Assert.IsTrue(targetZones.name.Equals("Target Zones"));
            Assert.IsTrue(spline.name.Equals("Spline"));

            District newDistrict = new(districtCount, districtTransform.name);
            Game.Districts[newDistrict.Id] = newDistrict;
            DistrictObject districtObject = districtTransform.gameObject.AddComponent<DistrictObject>();
            districtObject.Init(spline.GetComponent<SplineContainer>());
            districtObject.District = newDistrict;

            uint zoneCount = 1;
            foreach (Transform sourceZone in sourceZones.transform)
            {
                uint id = (zoneCount++ << (Zone.DistrictBitWidth + 1)) + (districtCount << 1);
                SourceZone newZone = new(id);
                newDistrict.SourceZones.Add(newZone);
                Game.SourceZones.Add(id, newZone);
                InitZoneObject(sourceZone.gameObject, newZone);
            }
            zoneCount = 1;
            foreach (Transform targetZone in targetZones.transform)
            {
                uint id = (zoneCount++ << (Zone.DistrictBitWidth + 1)) + (districtCount << 1) + 1;
                TargetZone newZone = new(id);
                newDistrict.TargetZones.Add(newZone);
                Game.TargetZones.Add(id, newZone);
                InitZoneObject(targetZone.gameObject, newZone);
            }
            newDistrict.Disable();
            districtCount++;
        }

        void InitZoneObject(GameObject gameObject, Zone newZone)
        {
            ZoneObject zoneObject = gameObject.AddComponent<ZoneObject>();
            gameObject.name = newZone.Id.ToString();
            zoneObject.Zone = newZone;
            zoneObject.zoneMaterial = zoneMaterial;
            zoneMapping[newZone.Id] = zoneObject;
            zoneObject.Init(gameObject.GetComponent<SplineContainer>());
        }

    }

    public void UpdateZoneObjectReferences()
    {
        foreach (ZoneObject zoneObject in zoneMapping.Values)
        {
            if (zoneObject.Zone is SourceZone)
                zoneObject.Zone = Game.SourceZones[zoneObject.Zone.Id];
            else
                zoneObject.Zone = Game.TargetZones[zoneObject.Zone.Id];
        }
    }
}