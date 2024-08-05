using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Zone Material Data", menuName = "Zone Material Data", order = 51)]
public class ZoneMaterial : ScriptableObject
{
    public Material SourceMaterial;
    public Material TargetMaterial;
    public Material DisbaledMaterial;
}