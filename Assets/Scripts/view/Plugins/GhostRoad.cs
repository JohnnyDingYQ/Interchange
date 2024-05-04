using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GhostRoad : MonoBehaviour
{
    [SerializeField]
    RoadGameObject roadPrefab;
    static RoadGameObject ghostroad;
    public static bool GhostIsOn { get; set; }
    void Awake()
    {
        ghostroad = Instantiate(roadPrefab);
    }
    void FixedUpdate()
    {
        if (GhostIsOn)
            UpdateGhostRoad();
    }

    void UpdateGhostRoad()
    {
        RemoveGhostRoad();
        if (Build.ShouldShowGhostRoad())
        {
            ghostroad.gameObject.SetActive(true);
            Road road = Build.BuildGhostRoad(InputSystem.MouseWorldPos);
            ghostroad.Road = road;
        }
        else
            ghostroad.gameObject.SetActive(false);
    }

    public static void RemoveGhostRoad()
    {
        if (ghostroad.Road != null)
            Game.RemoveRoad(ghostroad.Road);
    }
}