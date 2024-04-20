using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        Physics.queriesHitTriggers = false;
    }

}
