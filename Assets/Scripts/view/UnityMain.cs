using UnityEngine;

public class UnityMain : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    private bool showRoadAndLanes = true;
    private bool showPaths = true;
    private const float DrawDuration = 0.2f;
    void Awake()
    {

        Application.targetFrameRate = 60;

        Game.Unity = new SaveSystemImpl();

        if (inputManager != null)
        {
            inputManager.SaveGame += SaveGame;
            inputManager.LoadGame += LoadGame;
            inputManager.ShowRoadAndLanes += () =>
            {
                showRoadAndLanes = !showRoadAndLanes;
            };
            inputManager.ShowPaths += () =>
            {
                showPaths = !showPaths;
            };
        }
        InvokeRepeating("Draw", 0f, DrawDuration);
    }

    void Draw()
    {
        if (showPaths)
            Gizmos.DrawPaths(DrawDuration);
        if (showRoadAndLanes)
            Gizmos.DrawRoadsAndLanes(DrawDuration);
        Gizmos.DrawControlPoints(DrawDuration);
        Gizmos.DrawVertices(DrawDuration);
        Gizmos.DrawOutline(DrawDuration);
        
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.SaveGame -= SaveGame;
            inputManager.LoadGame -= LoadGame;
        }
    }

    void SaveGame()
    {
        Game.SaveGame();
    }

    void LoadGame()
    {
        Game.LoadGame();
    }
}
