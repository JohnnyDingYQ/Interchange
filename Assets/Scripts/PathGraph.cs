using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using QuikGraph.Algorithms;
using System;
using UnityEngine.Splines;


public class PathGraph : MonoBehaviour
{
    private static AdjacencyGraph<int, TaggedEdge<int, Spline>> graph;
    public static AdjacencyGraph<int, TaggedEdge<int, Spline>> Graph {
        get {
            graph ??= new();
            return graph;
        }
        set {
            graph = value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static IEnumerable<TaggedEdge<int, Spline>> GetPath(int root, int target) {
        Func<TaggedEdge<int, Spline>, double> edgeCost = edge => edge.Tag.GetLength();
        
        TryFunc<int, IEnumerable<TaggedEdge<int, Spline>>> tryGetPaths = Graph.ShortestPathsDijkstra(edgeCost, root);
        if (tryGetPaths(target, out IEnumerable<TaggedEdge<int, Spline>> path)) {
            return path;
        }
        return null;
    }

    public static void Print() {
        string str = "Vectices: ";
        foreach (int v in Graph.Vertices)
        {
            str += $"{v}, ";
        }
        str += "\nEdges: ";
        foreach (TaggedEdge<int, Spline> e in Graph.Edges)
        {
            str += $"{e.Source}->{e.Target} ";
        }

        Debug.Log(str);
    }
}
