using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;
using UnityEngine.Splines;

public class Zoning : MonoBehaviour
{
    [SerializeField] GameObject zoneSplines;
    [SerializeField] ZoneHumbleObject zonePrefab;
    void Awake()
    {
        Assert.AreEqual(0, zoneSplines.transform.position.x);
        Assert.AreEqual(0, zoneSplines.transform.position.y);
        foreach (Transform child in zoneSplines.transform)
        {
            ZoneHumbleObject zone = Instantiate(zonePrefab, transform);
            zone.name = child.gameObject.name;
            zone.Init(int.Parse(zone.name), child.gameObject.GetComponent<SplineContainer>());
        }
        Game.UpdateHoveredZone += UpdateHoveredZone;
    }

    void Start()
    {
        DemandsGenerator.GenerateDemands();
    }

    void FixedUpdate()
    {
        DemandsSatisfer.SatisfyDemands();
    }

    void OnDestroy()
    {
        Game.UpdateHoveredZone -= UpdateHoveredZone;
    }

    public void UpdateHoveredZone()
    {
        float3 mousePos = Game.MouseWorldPos;
        mousePos.y = Constants.ElevationOffset + 5;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(mousePos, new float3(0, -1, 0), 100);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.GetComponent<ZoneHumbleObject>() != null)
                Game.HoveredZone = hit.collider.gameObject.GetComponent<ZoneHumbleObject>().zone;
        }
    }
}