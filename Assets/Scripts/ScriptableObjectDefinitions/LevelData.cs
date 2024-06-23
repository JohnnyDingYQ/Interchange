using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Data", menuName = "Level Data", order = 51)]
public class NYC : ScriptableObject
{
    public List<int> InitialZones;
    public List<string> ZoneNames;
    public AnimationCurve Difficulty;
    public List<float> ZoneDemandScaling;
    public List<int> ZoneUnlockRequirements;
}