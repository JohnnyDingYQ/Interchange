using UnityEngine;
using UnityEngine.EventSystems;

public class RoadGameObject : MonoBehaviour
{
    public Road Road { get; set; }

    void OnMouseOver()
    {
        Debug.Log(Road);
        Game.SelectedRoad = Road;
    }

    // void OnMouseExit()
    // {
    //     Game.SelectedRoad = null;
    // }
}