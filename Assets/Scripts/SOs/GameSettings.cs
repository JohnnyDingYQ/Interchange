using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Settings", menuName = "Scriptable Objects/Game Settings", order = 51)]
public class GameSettings : ScriptableObject
{
    public bool levelEditorOn;
    public bool debugPanelOn;
    public bool continuousBuild;
    public bool displaysGhost;
}