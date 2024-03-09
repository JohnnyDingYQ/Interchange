using UnityEngine;

public class UnityMain : MonoBehaviour
{
    [SerializeField] private int Height;
    [SerializeField] private int Width;
    [SerializeField] private InputManager inputManager;
    void Awake()
    {
        Utility.Info.logEnabled = true;

        Application.targetFrameRate = 165;

        Game.SaveSystem = new SaveSystemImpl();

        if (inputManager != null)
        {
            inputManager.SaveGame += SaveGame;
            inputManager.LoadGame += LoadGame;
        }
        InvokeRepeating("Draw", 0f, 0.5f);
    }

    void Draw()
    {
        Utility.DrawAllRoads(0.5f);
        Utility.DrawControlPoints(0.5f);
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
