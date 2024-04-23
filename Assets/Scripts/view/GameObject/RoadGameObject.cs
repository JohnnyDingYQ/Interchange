using UnityEngine;
using UnityEngine.EventSystems;

public class RoadGameObject : MonoBehaviour
{
    public Road Road { get; set; }

    void OnMouseOver()
    {
        Game.SelectedRoad = Road;
    }

    void OnMouseExit()
    {
        Game.SelectedRoad = null;
    }
}