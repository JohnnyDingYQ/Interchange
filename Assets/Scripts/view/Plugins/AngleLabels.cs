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
        label.SetText(MyNumerics.Round(angle, 1) + "°");

    }
}