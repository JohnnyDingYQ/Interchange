using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Intersections : MonoBehaviour
{
    [SerializeField]
    IntersectionObject intersectionPrefab;
    [SerializeField]
    Color safeColor, unsafeColor, createdColor;
    [SerializeField]
    GoreAreaObject goreAreaPrefab;
    readonly Dictionary<uint, IntersectionObject> intersectionMapping = new();

    void OnEnable()
    {
        Game.IntersectionAdded += InstantiateIntersection;
        Game.IntersectionUpdated += UpdateIntersection;
        Game.IntersectionRemoved += DestroyIntersection;
    }

    void OnDisable()
    {
        Game.IntersectionAdded -= InstantiateIntersection;
        Game.IntersectionUpdated -= UpdateIntersection;
        Game.IntersectionRemoved -= DestroyIntersection;
    }

    public void InstantiateIntersection(Intersection ix)
    {
        IntersectionObject ixObject = Instantiate(intersectionPrefab, transform);
        ixObject.Intersection = ix;
        float3 pos = GetCenter(ix);
        pos.y = Main.GetHUDObjectHeight(HUDLayer.Intersections);
        ixObject.transform.position = pos;
        intersectionMapping[ix.Id] = ixObject;
        CreateGoreAreas(ix);
    }

    public void DestroyIntersection(Intersection ix)
    {
        Destroy(intersectionMapping[ix.Id].gameObject);
    }

    void UpdateIntersection(Intersection ix)
    {
        IntersectionObject ixObject = intersectionMapping[ix.Id];
        ixObject.GetComponent<Renderer>().material.SetColor("_Color", !ix.IsSafe ? unsafeColor : ix.CreatedByDivision ? createdColor : safeColor);
        ixObject.transform.position = GetCenter(ix);
        RemoveGoreAreas(ix);
        CreateGoreAreas(ix);
    }

    void RemoveGoreAreas(Intersection ix)
    {
        IntersectionObject ixObject = intersectionMapping[ix.Id];
        while (ixObject.transform.childCount > 0)
            DestroyImmediate(ixObject.transform.GetChild(0).gameObject);
    }

    void CreateGoreAreas(Intersection ix)
    {
        IntersectionObject ixObject = intersectionMapping[ix.Id];
        IEnumerable<GoreArea> goreAreas = ix.GetGoreAreas();
        foreach (GoreArea goreArea in goreAreas)
        {
            GoreAreaObject goreAreaObject = Instantiate(goreAreaPrefab, ixObject.transform);
            goreAreaObject.GoreArea = goreArea;
            goreAreaObject.Init();
        }
    }

    public void DestoryAll()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
        intersectionMapping.Clear();
    }

    float3 GetCenter(Intersection ix)
    {
        return (ix.Nodes.First().Pos + ix.Nodes.Last().Pos) / 2;
    }

    float3 GetWidth(Intersection ix)
    {
        return ix.Nodes.Count * Constants.LaneWidth;
    }
}