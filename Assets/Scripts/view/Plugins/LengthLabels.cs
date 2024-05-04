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
    private TextLabel seg1;
    private TextLabel seg2;

    void Awake()
    {
        seg1 = Instantiate(textLabel, gameObject.transform);
        seg2 = Instantiate(textLabel, gameObject.transform);

    }

    void FixedUpdate()
    {
        List<Tuple<float3, float3>> t = Build.GetSupportLines();
        if (t.Count == 0)
        {
            seg1.gameObject.SetActive(false);
            seg2.gameObject.SetActive(false);
        }
        if (t.Count == 2)
        {
            seg1.gameObject.SetActive(true);
            seg2.gameObject.SetActive(true);
            seg1.ApplyWorldPos((t[0].Item1 + t[0].Item2) / 2);
            seg2.ApplyWorldPos((t[1].Item1 + t[1].Item2) / 2);
            seg1.SetText(Round(math.length(t[0].Item1 - t[0].Item2), 1) + "u");
            seg2.SetText(Round(math.length(t[1].Item1 - t[1].Item2), 1) + "u");
        }
    }

    float Round(float n, int places)
    {
        return (float) (Math.Round(n * Math.Pow(10, places)) / Math.Pow(10, places));
    }
}