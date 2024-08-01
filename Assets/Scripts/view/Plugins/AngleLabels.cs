using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AngleLabels : MonoBehaviour
{
    [SerializeField]
    private TextLabel textLabel;
    private TextLabel label;

    void Awake()
    {
        label = Instantiate(textLabel, gameObject.transform);
    }

    void OnEnable()
    {
        Build.SupportedLineUpdated += DrawAngelLabel;
    }

    void OnDisable()
    {

        Build.SupportedLineUpdated -= DrawAngelLabel;
    }


    void DrawAngelLabel(SupportLine supportLine)
    {
        supportLine.ReplaceYCoord(Main.GetHUDObjectHeight(HUDLayer.SupportLines));
        if (!supportLine.Segment1Set)
        {
            label.gameObject.SetActive(false);
            return;
        }
        label.gameObject.SetActive(true);
        label.ApplyWorldPos(supportLine.Segment1.end);
        float angle = MyNumerics.AngleInDegrees(
            supportLine.Segment2.end - supportLine.Segment2.start,
            supportLine.Segment1.end - supportLine.Segment1.start
        );
        label.SetText(Round(angle, 1) + "°");

    }

    float Round(float n, int places)
    {
        return (float)(Math.Round(n * Math.Pow(10, places)) / Math.Pow(10, places));
    }
}