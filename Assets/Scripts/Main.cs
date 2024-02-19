using UnityEngine;

public class Main : MonoBehaviour
{
    public static Vector3 MouseWorldPos;
    [SerializeField] private int Height;
    [SerializeField] private int Width;
    void Awake()
    {
        Utility.Info.logEnabled = true;

        Grid.Height = Height;
        Grid.Width = Width;
        Grid.Dim = 1;
        Grid.Level = 1;
        Utility.DrawGridBounds();

        Application.targetFrameRate = 165;

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
    }
}
