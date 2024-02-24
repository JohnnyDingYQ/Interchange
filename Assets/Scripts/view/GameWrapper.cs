using UnityEngine;

public class Main : MonoBehaviour
{
    public static Vector3 MouseWorldPos;
    [SerializeField] private int Height;
    [SerializeField] private int Width;
    [SerializeField] private InputManager inputManager;
    void Awake()
    {
        Utility.Info.logEnabled = true;

        Application.targetFrameRate = 165;

        Game.SaveSystem = new SaveSystem();

        if (inputManager != null)
        {
            inputManager.SaveGame += SaveGame;
            inputManager.LoadGame += LoadGame;
        }

    }

    // Update is called once per frame
    void Update()
    {
        MouseWorldPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)
        {
            z = Camera.main.transform.position.y -0
        };
        MouseWorldPos = Camera.main.ScreenToWorldPoint(MouseWorldPos);
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
