using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private const int CameraSpeedMultiplier = 25;
    private const float CameraScrollMultiplier = 1.5f;
    [SerializeField] private InputManager inputManager;
    // Start is called before the first frame update
    void Start()
    {
        if (inputManager != null)
        {
            inputManager.MoveCameraUp += MoveCameraUp;
            inputManager.MoveCameraLeft += MoveCameraLeft;
            inputManager.MoveCameraDown += MoveCameraDown;
            inputManager.MoveCameraRight += MoveCameraRight;
        }
    }

    void Update()
    {
         Camera.main.transform.position += new Vector3(0, -Input.mouseScrollDelta.y * CameraScrollMultiplier ,0);
    }

    void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.MoveCameraUp -= MoveCameraUp;
            inputManager.MoveCameraLeft -= MoveCameraLeft;
            inputManager.MoveCameraDown -= MoveCameraDown;
            inputManager.MoveCameraRight -= MoveCameraRight;
        }
    }

    private void MoveCameraUp()
    {
        float normalizedSpeed = CameraSpeedMultiplier * Time.deltaTime;
        Camera.main.transform.position += new Vector3(0, 0, normalizedSpeed);
    }

    private void MoveCameraDown()
    {
        float normalizedSpeed = CameraSpeedMultiplier * Time.deltaTime;
        Camera.main.transform.position += new Vector3(0, 0 ,-normalizedSpeed);
    }

    private void MoveCameraRight()
    {
        float normalizedSpeed = CameraSpeedMultiplier * Time.deltaTime;
        Camera.main.transform.position += new Vector3(normalizedSpeed, 0 ,0);
    }

    private void MoveCameraLeft()
    {
        float normalizedSpeed = CameraSpeedMultiplier * Time.deltaTime;
        Camera.main.transform.position += new Vector3(-normalizedSpeed, 0 ,0);
    }
}
