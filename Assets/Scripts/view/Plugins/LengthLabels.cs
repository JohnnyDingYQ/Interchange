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
    private TextLabel label1;
    private TextLabel label2;

    void Awake()
    {
        label1 = Instantiate(textLabel, gameObject.transform);
        label2 = Instantiate(textLabel, gameObject.transform);
    }

    void OnEnable()
    {
        Build.SupportedLineUpdated += DrawLengthLabel;
    }

    void OnDisable()
    {

        Build.SupportedLineUpdated -= DrawLengthLabel;
    }

    void DrawLengthLabel(SupportLine supportLine)
    {
        supportLine.ReplaceYCoord(Main.GetHUDObjectHeight(HUDLayer.SupportLines));
        if (supportLine.Segment1Set)
        {
            label1.gameObject.SetActive(true);
            label1.ApplyWorldPos((supportLine.Segment1.start + supportLine.Segment1.end) / 2);
            label1.SetText(MyNumerics.Round(math.length(supportLine.Segment1.start - supportLine.Segment1.end), 1) + "u");
        }
        else
            label1.gameObject.SetActive(false);

        if (supportLine.Segment2Set)
        {
            label2.gameObject.SetActive(true);
            label2.ApplyWorldPos((supportLine.Segment2.start + supportLine.Segment2.end) / 2);
            label2.SetText(MyNumerics.Round(math.length(supportLine.Segment2.start - supportLine.Segment2.end), 1) + "u");
        }
        else
            label2.gameObject.SetActive(false);

    }

}