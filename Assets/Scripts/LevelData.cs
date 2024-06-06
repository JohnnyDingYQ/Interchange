using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LevelData", menuName = "Level Data", order = 51)]
public class NYC : ScriptableObject
{
    public List<int> InitialZones;
    public List<string> ZoneNames;
}