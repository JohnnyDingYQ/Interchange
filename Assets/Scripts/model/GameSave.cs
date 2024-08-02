using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using System;
using System.Collections;
using System.Reflection;

public class GameSave : IPersistable
{
    public readonly float versionNumber = 0.1f;
    public uint CarServiced { get; set; }
    [IPersistableDict]
    public Dictionary<uint, Node> Nodes { get; private set; }
    [IPersistableDict]
    public Dictionary<uint, Road> Roads { get; private set; }
    [IPersistableDict]
    public Dictionary<uint, Intersection> Intersections { get; private set; }
    [IPersistableDict]
    public Dictionary<uint, Lane> Lanes { get; private set; }
    [IPersistableDict]
    public Dictionary<uint, Vertex> Vertices { get; private set; }
    [IPersistableDict]
    public Dictionary<uint, Interchange.Edge> Edges { get; private set; }
    [IPersistableDict]
    public Dictionary<uint, Curve> Curves { get; private set; }
    [NotSaved]
    public Dictionary<uint, Car> Cars { get; private set; }
    public uint Id { get; set; }

    public GameSave()
    {
        Vertices = new();
        Edges = new();
        Lanes = new();
        Roads = new();
        Nodes = new();
        Intersections = new();
        Cars = new();
        Curves = new();
        CarServiced = 0;
    }

    public bool IPersistableAreInDict()
    {
        List<FieldProperty> fieldProperties = FieldProperty.GetFieldProperties(GetType(), this);
        Dictionary<Type, Dictionary<uint, IPersistable>> lut = new();
        foreach (FieldProperty fieldProperty in fieldProperties)
            if (fieldProperty.GetCustomAttribute<IPersistableDictAttribute>() != null)
            {
                Dictionary<uint, IPersistable> dict = new();
                object value = fieldProperty.GetValue();
                PropertyInfo valuesProp = fieldProperty.Type().GetProperty("Values");
                dynamic values = valuesProp.GetValue(value);
                foreach (IPersistable item in values)
                    dict[item.Id] = item;

                lut[fieldProperty.GetGenericCollectionItemType(1)] = dict;
            }
        foreach (FieldProperty fieldProperty in fieldProperties)
            if (fieldProperty.GetCustomAttribute<IPersistableDictAttribute>() != null)
            {
                dynamic dict = fieldProperty.GetValue(this);
                Type itemType = fieldProperty.GetGenericCollectionItemType(1);
                List<FieldProperty> itemProperties = FieldProperty.GetFieldProperties(itemType, null);
                foreach (IPersistable itemInDict in dict.Values)
                {
                    foreach (FieldProperty itemProperty in itemProperties)
                    {
                        if (itemProperty.GetCustomAttribute<SaveIDAttribute>() != null)
                        {
                            IPersistable item = (IPersistable)itemProperty.GetValue(itemInDict);
                            if (item == null)
                                continue;
                            if (!Equals(item, lut[itemProperty.Type()][item.Id]))
                                return false;
                            continue;
                        }
                        if (itemProperty.GetCustomAttribute<SaveIDCollectionAttribute>() != null)
                        {
                            if (itemProperty.GetValue(itemInDict) is IEnumerable<IPersistable> collection)
                            {
                                foreach (IPersistable item in collection)
                                    if (!Equals(item, lut[itemProperty.GetGenericCollectionItemType(0)][item.Id]))
                                        return false;
                            }
                            else
                                throw new InvalidOperationException();
                            continue;
                        }
                    }
                }
            }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj is GameSave other)
        {
            foreach (FieldProperty fieldProperty in FieldProperty.GetFieldProperties(GetType(), this))
            {
                if (fieldProperty.GetCustomAttribute<IPersistableDictAttribute>() != null)
                {
                    dynamic dict = fieldProperty.GetValue(this);
                    dynamic otherDict = fieldProperty.GetValue(other);
                    if (dict.Count != otherDict.Count)
                        return false;
                    foreach (uint key in dict.Keys)
                        if (!Equals(dict[key], otherDict[key]))
                            return false;
                    continue;
                }
            }
            return true;
        }
        else
            return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}