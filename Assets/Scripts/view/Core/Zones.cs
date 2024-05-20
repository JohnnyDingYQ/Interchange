using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Zones : MonoBehaviour
{
    [SerializeField] GameObject zoneSplines;
    [SerializeField] Zone zonePrefab;
    void Awake()
    {
        Assert.AreEqual(0, zoneSplines.transform.position.x);
        Assert.AreEqual(0, zoneSplines.transform.position.y);
        foreach (Transform child in zoneSplines.transform)
        {
            Zone zone = Instantiate(zonePrefab, transform);
            zone.name = child.gameObject.name;
            zone.Init(child.gameObject.GetComponent<SplineContainer>(), int.Parse(zone.name));
        }
    }

    void Start()
    {
        foreach (IZone zone in Game.Zones.Values)
            foreach (IZone other in Game.Zones.Values)
                if (zone != other)
                    zone.Demands[other.Id] = 1;
    }
}