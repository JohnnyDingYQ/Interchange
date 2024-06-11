using UnityEngine;
using UnityEngine.EventSystems;

public class RoadHumbleObject : MonoBehaviour
{
    public Road Road { get; set; }

    void OnMouseOver()
    {
        Game.HoveredRoad = Road;
    }

    void OnMouseExit()
    {
        Game.HoveredRoad = null;
    }
}