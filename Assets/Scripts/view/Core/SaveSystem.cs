using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuikGraph;
using QuikGraph.Collections;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public static class SaveSystem
{
    public static void LoadGame()
    {
        Game.WipeState();
        string saveFile = Application.persistentDataPath + "/save0.json";

        if (File.Exists(saveFile))
        {
            string fileContents = File.ReadAllText(saveFile);
            // There is no server, whatever
            Game.GameSave = JsonConvert.DeserializeObject<GameSave>(fileContents, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });
            RestoreGameState();
        }
        else
        {
            throw new InvalidOperationException("Accessed save file does not exist");
        }

        static void RestoreGameState()
        {
            Game.Graph.AddVerticesAndEdgeRange(Game.GameSave.GraphSave);
            IdToObjectReferences();
            foreach (Road r in Game.Roads.Values)
            {
                r.EvaluateInnerOutline();
                Game.InvokeRoadAdded(r);
            }
            EvaluteIntersections();
        }
        static void EvaluteIntersections()
        {
            HashSet<Intersection> evaluated = new();
            foreach (Road r in Game.Roads.Values)
                foreach (Intersection i in new Intersection[] { r.StartIntersection, r.EndIntersection })
                    if (!evaluated.Contains(i))
                    {
                        evaluated.Add(i);
                        IntersectionUtil.EvaluateOutline(i);
                    }
        }
    }

    public static void SaveGame()
    {
        Game.GameSave.GraphSave = Game.Graph.Edges.ToList();
        ObjectReferencesToId();

        string s = JsonConvert.SerializeObject(Game.GameSave, Formatting.None, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            TypeNameHandling = TypeNameHandling.Auto,
        });
        File.WriteAllText(Application.persistentDataPath + "/save0.json", s);


    }

    static void ObjectReferencesToId()
    {
        foreach (Road road in Game.Roads.Values)
        {
            road.StartIntersection_ = road.StartIntersection.Id;
            road.EndIntersection_ = road.EndIntersection.Id;
            road.Lanes_ = road.Lanes.Select(lane => lane.Id).ToList();
        }
        foreach (Lane lane in Game.Lanes.Values)
        {
            lane.StartNode_ = lane.StartNode.Id;
            lane.EndNode_ = lane.EndNode.Id;
            lane.Road_ = lane.Road.Id;
        }
        foreach (Node node in Game.Nodes.Values)
        {
            node.InLanes_ = node.GetLanes(Direction.In).Select(l => l.Id).ToList();
            node.OutLanes_ = node.GetLanes(Direction.Out).Select(l => l.Id).ToList();
            node.Intersection_ = node.Intersection.Id;
        }
        foreach (Intersection i in Game.Intersections.Values)
        {
            i.Nodes_ = i.Nodes.Select(n => n.Id).ToList();
            i.InRoads_ = i.InRoads.Select(r => r.Id).ToList();
            i.OutRoads_ = i.OutRoads.Select(r => r.Id).ToList();
        }
    }

    static void IdToObjectReferences()
    {
        foreach (Road road in Game.Roads.Values)
        {
            road.StartIntersection = Game.Intersections[road.StartIntersection_];
            road.EndIntersection = Game.Intersections[road.EndIntersection_];
            road.Lanes = road.Lanes_.Select(i => Game.Lanes[i]).ToList();
        }
        foreach (Lane lane in Game.Lanes.Values)
        {
            lane.StartNode = Game.Nodes[lane.StartNode_];
            lane.EndNode = Game.Nodes[lane.EndNode_];
            lane.Road = Game.Roads[lane.Road_];
        }
        foreach (Node node in Game.Nodes.Values)
        {
            node.SetInLanes(node.InLanes_.Select(l => Game.Lanes[l]).ToHashSet());
            node.SetOutLanes(node.OutLanes_.Select(l => Game.Lanes[l]).ToHashSet());
            node.Intersection = Game.Intersections[node.Intersection_];
        }
        foreach (Intersection i in Game.Intersections.Values)
        {
            i.SetNodes(i.Nodes_.Select(n => Game.Nodes[n]).ToList());
            i.SetInRoads(i.InRoads_.Select(id => Game.Roads[id]).ToHashSet());
            i.SetOutRoads(i.OutRoads_.Select(id => Game.Roads[id]).ToHashSet());
        }
    }

}