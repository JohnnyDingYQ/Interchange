using UnityEngine;

[CreateAssetMenu(fileName = "New Zone Color Data", menuName = "Zone Color Data", order = 51)]
public class ZoneColor : ScriptableObject
{
    public Color FullyConnected;
    public Color Unconnected;
    public Color Disbaled;
}