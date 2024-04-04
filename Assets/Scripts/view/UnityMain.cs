using UnityEngine;

public class UnityMain : MonoBehaviour
{
    [SerializeField] private int Height;
    [SerializeField] private int Width;
    [SerializeField] private InputManager inputManager;
    private bool showRoadAndLanes = true;
    private bool showPaths = true;
    private const float DrawDuration = 0.5f;
    void Awake()
    {
        Utility.Info.logEnabled = true;

        Application.targetFrameRate = 60;

        Game.SaveSystem = new SaveSystemImpl();

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
            Utility.DrawPaths(DrawDuration);
        if (showRoadAndLanes)
            Utility.DrawRoadsAndLanes(DrawDuration);
        Utility.DrawControlPoints(DrawDuration);
        Utility.DrawVertices(DrawDuration);
        Utility.DrawOutline(DrawDuration);
        
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
        Utility.Info.Log("Saving");
        Game.SaveGame();
    }

    void LoadGame()
    {
        Utility.Info.Log("Loading");
        Game.LoadGame();
    }
}
