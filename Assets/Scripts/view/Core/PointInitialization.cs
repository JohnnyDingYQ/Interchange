using System.Linq;
using UnityEngine;

public class PointInitialization : MonoBehaviour
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

            Node n = new(child.transform.position, 0, 0);
            Intersection i = new();
            i.SetNodes(new() { n });
            n.Intersection = i;
            Game.RegisterIntersection(i);
            Game.RegisterNode(n);
        }

        foreach (Transform child in sources.transform)
        {
            Point target = new(uint.Parse(child.gameObject.name));
            Game.Sources.Add(target.Id, target);

            Node n = new(child.transform.position, 0, 0);
            Intersection i = new();
            i.SetNodes(new() { n });
            n.Intersection = i;
            Game.RegisterIntersection(i);
            Game.RegisterNode(n);
        }

        Debug.Log(Game.Targets.Keys.First());
        Debug.Log(Game.Sources.Keys.First());
    }
}