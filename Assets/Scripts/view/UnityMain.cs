using UnityEngine;

public class UnityMain : MonoBehaviour
{
    [SerializeField] private int Height;
    [SerializeField] private int Width;
    [SerializeField] private InputManager inputManager;
    private bool showRoadAndLanes = true;
    private bool showPaths = true;
    private float drawDuration = 0.5f;
    void Awake()
    {
        Utility.Info.logEnabled = true;

        Application.targetFrameRate = 165;

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
        InvokeRepeating("Draw", 0f, drawDuration);
    }

    void Draw()
    {
        if (showPaths)
            Utility.DrawPaths(drawDuration);
        if (showRoadAndLanes)
            Utility.DrawRoadsAndLanes(drawDuration);
        Utility.DrawControlPoints(drawDuration);
        Utility.DrawVertices(drawDuration);
        
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
