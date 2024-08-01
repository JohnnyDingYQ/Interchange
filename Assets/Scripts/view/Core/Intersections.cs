using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Intersections : MonoBehaviour
{
    [SerializeField]
    IntersectionHumbleObject intersectionPrefab;
    Dictionary<uint, IntersectionHumbleObject> intersectionMapping = new();

    void OnEnable()
    {
        Game.IntersectionAdded += InstantiateIntersection;
        Game.IntersectionRemoved += DestroyIntersection;
    }

    void OnDisable()
    {
        Game.IntersectionAdded -= InstantiateIntersection;
        Game.IntersectionRemoved -= DestroyIntersection;
    }

    public void InstantiateIntersection(Intersection ix)
    {
        IntersectionHumbleObject ixObject = Instantiate(intersectionPrefab, transform);
        ixObject.Intersection = ix;
        float3 pos = GetCenter(ix);
        pos.y = Main.GetHUDObjectHeight(HUDLayer.Intersections);
        ixObject.transform.position = pos;
        intersectionMapping[ix.Id] = ixObject;
    }

    public void DestroyIntersection(Intersection ix)
    {
        Destroy(intersectionMapping[ix.Id].gameObject);
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