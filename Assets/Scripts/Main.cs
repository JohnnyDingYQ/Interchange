using UnityEngine;

public class Main : MonoBehaviour
{
    public static Vector3 MouseWorldPos;
    [SerializeField] private int Height;
    [SerializeField] private int Width;
    public static int BuildMode { get; set; }
    void Awake()
    {
        Log.Info.logEnabled = true;
        Log.DrawGridBounds();

        Grid.Height = Height;
        Grid.Width = Width;
        Grid.Dim = 1;
        Grid.Level = 1;

        BuildMode = 1;

        Camera.main.transform.position =
            new Vector3(
                ((float)Grid.Width) / 2 * Grid.Dim,
                Camera.main.transform.position.y,
                ((float)Grid.Height) / 2 * Grid.Dim
            );
    }

    // Update is called once per frame
    void Update()
    {
        MouseWorldPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z)
        {
            z = Camera.main.transform.position.y - Grid.Level
        };
        MouseWorldPos = Camera.main.ScreenToWorldPoint(MouseWorldPos);
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BuildMode = 1;
            Log.Info.Log("Main: Build mode switched to 1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            BuildMode = 2;
            Log.Info.Log("Main: Build mode switched to 2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            BuildMode = 3;
            Log.Info.Log("Main: Build mode switched to 3");
        }
    }
}
