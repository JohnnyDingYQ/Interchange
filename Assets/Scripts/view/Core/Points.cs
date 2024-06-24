using System.Linq;
using UnityEngine;

public class Points : MonoBehaviour
{
    [SerializeField]
    GameObject targets;
    [SerializeField]
    GameObject sources;

    void Start()
    {
        foreach (Transform child in targets.transform)
        {
            Point target = new(uint.Parse(child.gameObject.name));
            Game.Targets.Add(target.Id, target);

            Node n = new(child.transform.position, 0, 0)
            {
                IsPersistent = true
            };
            Intersection i = new();
            i.SetNodes(new() { n });
            n.Intersection = i;
            target.Node = n;
            Game.RegisterIntersection(i);
            Game.RegisterNode(n);
        }

        foreach (Transform child in sources.transform)
        {
            SourcePoint source = new(uint.Parse(child.gameObject.name));
            Game.Sources.Add(source.Id, source);

            Node n = new(child.transform.position, 0, 0)
            {
                IsPersistent = true
            };
            Intersection i = new();
            i.SetNodes(new() { n });
            n.Intersection = i;
            source.Node = n;
            Game.RegisterIntersection(i);
            Game.RegisterNode(n);
        }
    }

    void Update()
    {
        DemandsGenerator.GenerateDemands(Time.deltaTime);
        DemandsSatisfer.SatisfyDemands(Time.deltaTime);
    }
}