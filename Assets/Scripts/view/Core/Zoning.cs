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

    void Update()
    {
        DemandsGenerator.GenerateDemands(Time.deltaTime);
        DemandsSatisfer.SatisfyDemands(Time.deltaTime);

    }

    void OnDestroy()
    {
        Game.UpdateHoveredZone -= UpdateHoveredZone;
    }

    public void UpdateHoveredZone()
    {
        float3 mousePos = Game.MouseWorldPos;
        mousePos.y = Constants.HeightOffset + 5;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(mousePos, new float3(0, -1, 0), 100);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.GetComponent<ZoneHumbleObject>() != null)
                Game.HoveredZone = hit.collider.gameObject.GetComponent<ZoneHumbleObject>().Zone;
        }
    }
}