using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Splines;

public class Zones: MonoBehaviour
{
    [SerializeField] GameObject zoneSplines;
    [SerializeField] Zone zonePrefab;

    void Awake()
    {
        Assert.AreEqual(0, zoneSplines.transform.position.x);
        Assert.AreEqual(0, zoneSplines.transform.position.y);
        Zone zone = Instantiate(zonePrefab, transform);
        zone.Init(zoneSplines.GetComponent<SplineContainer>());

    }
}