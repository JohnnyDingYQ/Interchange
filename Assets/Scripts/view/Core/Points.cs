using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class Points : MonoBehaviour
{
    [SerializeField]
    GameObject targets;
    [SerializeField]
    GameObject sources;
    static GameObject sessionTargets;
    static GameObject sessionSources;

    void Start()
    {
        sessionSources = sources;
        sessionTargets = targets;
        foreach (Transform child in targets.transform)
        {
            Point target = new(uint.Parse(child.gameObject.name));
            Game.Targets.Add(target.Id, target);

            SetupNode(target, child.transform.position);
        }

        foreach (Transform child in sources.transform)
        {
            SourcePoint source = new(uint.Parse(child.gameObject.name));
            Game.Sources.Add(source.Id, source);

            SetupNode(source, child.transform.position);
        }

        static void SetupNode(Point p, float3 pos)
        {
            Node n = new(pos, 0, 0)
            {
                IsPersistent = true
            };
            Intersection i = new();
            i.SetNodes(new() { n });
            n.Intersection = i;
            p.Node = n;
            Game.RegisterIntersection(i);
            Game.RegisterNode(n);
        }
    }

    public static bool NodePositionsAreCorrect()
    {
        // nothing to check against, function is vacuously true
        if (sessionTargets == null || sessionSources == null)
            return true;
        foreach (Transform child in sessionSources.transform)
        {
            uint id = uint.Parse(child.gameObject.name);
            Point p = Game.Sources[id];
            if (!MyNumerics.AreNumericallyEqual(p.Node.Pos, child.transform.position))
                return false;
        }
        foreach (Transform child in sessionTargets.transform)
        {
            uint id = uint.Parse(child.gameObject.name);
            Point p = Game.Targets[id];
            if (!MyNumerics.AreNumericallyEqual(p.Node.Pos, child.transform.position))
                return false;
        }
        return true;
    }

    void Update()
    {
        DemandsGenerator.GenerateDemands(Time.deltaTime);
        DemandsSatisfer.SatisfyDemands(Time.deltaTime);
    }

    void OnDestroy()
    {
        sessionSources = null;
        sessionTargets = null;
    }
}