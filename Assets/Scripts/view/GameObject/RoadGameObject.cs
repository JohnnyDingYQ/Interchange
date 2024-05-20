using UnityEngine;
using UnityEngine.EventSystems;

public class RoadGameObject : MonoBehaviour
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