using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private int cameraSpeed;
    private const float CameraScrollMultiplier = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            float normalizedSpeed = cameraSpeed*Time.deltaTime;
            Camera.main.transform.position += new Vector3(0, 0 ,normalizedSpeed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            float normalizedSpeed = cameraSpeed*Time.deltaTime;
            Camera.main.transform.position += new Vector3(-normalizedSpeed, 0 ,0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            float normalizedSpeed = cameraSpeed*Time.deltaTime;
            Camera.main.transform.position += new Vector3(0, 0 ,-normalizedSpeed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            float normalizedSpeed = cameraSpeed*Time.deltaTime;
            Camera.main.transform.position += new Vector3(normalizedSpeed, 0 ,0);
        }
        Camera.main.transform.position += new Vector3(0, -Input.mouseScrollDelta.y * CameraScrollMultiplier ,0);
    }
}
