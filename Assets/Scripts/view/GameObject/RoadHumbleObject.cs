using UnityEngine;
using UnityEngine.EventSystems;

public class RoadHumbleObject : MonoBehaviour
{
    public Road Road { get; set; }

    // void OnMouseOver()
    // {
    //     if (Road.IsGhost)
    //         return;
    //     gameObject.layer = LayerMask.NameToLayer("Outline");
    //     Game.HoveredRoad = Road;
    // }

    // void OnMouseExit()
    // {
    //     gameObject.layer = LayerMask.NameToLayer("Default");
    //     Game.HoveredRoad = null;
    // }
}