using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AngleLabels : MonoBehaviour
{
    [SerializeField]
    private TextLabel textLabel;
    private List<TextLabel> labels;

    void Awake()
    {
        labels = new();
        for (int i = 0; i < 2; i++)
            labels.Add(Instantiate(textLabel, gameObject.transform));
    }

    void FixedUpdate()
    {
        List<Tuple<float3, float3>> t = Build.SupportLines;
        for (int i = 1; i < Build.SupportLines.Count; i++)
        {
            TextLabel l = labels[i];
            Tuple<float3, float3> s1 = Build.SupportLines[i - 1];
            Tuple<float3, float3> s2 = Build.SupportLines[i];

            l.gameObject.SetActive(true);
            l.ApplyWorldPos(s1.Item2);
            l.SetText(Round(GetAngleInDegree(s2.Item1 - s2.Item2, s1.Item2 - s1.Item1), 1) + "°");
        }
        for (int i = Build.SupportLines.Count; i < labels.Count; i++)
            labels[i].gameObject.SetActive(false);
    }

    float Round(float n, int places)
    {
        return (float)(Math.Round(n * Math.Pow(10, places)) / Math.Pow(10, places));
    }

    float GetAngleInDegree(float3 a, float3 b)
    {
        if (math.length(a) == 0 || math.length(b) == 0)
            return 0;
        return MathF.Acos(math.dot(a, b) / math.length(a) / math.length(b)) / MathF.PI * 180;
    }
}