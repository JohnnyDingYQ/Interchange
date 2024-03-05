using System;
using System.Collections.Generic;

/// <summary>
/// Package an object with a float so that it can be sorted with respect to the float
/// </summary>
public class FloatContainer : IComparable<FloatContainer>
{
    public float Float { get; set; }
    public object Object { get; set; }
    public FloatContainer(float f, Node o)
    {
        Float = f;
        Object = o;
    }

    public int CompareTo(FloatContainer other)
    {
        return Float.CompareTo(other.Float);
    }

    public static List<T> Unwrap<T>(List<FloatContainer> fcs)
    {
        List<T> objects = new();
        foreach(FloatContainer fc in fcs)
            objects.Add((T) fc.Object);
        return objects;
    }
}