using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class LengthLabels : MonoBehaviour
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
        for (int i = 0; i < Build.SupportLines.Count; i++)
        {
            TextLabel l = labels[i];
            Tuple<float3, float3, float> s = Build.SupportLines[i];

            l.gameObject.SetActive(true);
            l.ApplyWorldPos((s.Item1 + s.Item2) / 2);
            l.SetText(Round(s.Item3, 1) + "u");
        }
        for (int i = Build.SupportLines.Count; i < labels.Count; i++)
            labels[i].gameObject.SetActive(false);
    }

    float Round(float n, int places)
    {
        return (float) (Math.Round(n * Math.Pow(10, places)) / Math.Pow(10, places));
    }
}