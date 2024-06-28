using UnityEngine;

public class ModeToggle : MonoBehaviour
{

    static bool IsInBuildMode { get; set; }

    void Start()
    {
        IsInBuildMode = true;
        ToggleMode();
    }

    public static void ToggleMode()
    {
        if (IsInBuildMode)
            SwitchToViewMode();
        else
            SwitchToBuildMode();
        IsInBuildMode = !IsInBuildMode;
        Game.BuildModeOn = IsInBuildMode;
    }

    static void SwitchToBuildMode()
    {
        Camera.main.cullingMask = ~(1 << LayerMask.NameToLayer("Cars"));
        CarDriver.TimeScale = 0;
    }

    static void SwitchToViewMode()
    {
        Camera.main.cullingMask = LayerMask.NameToLayer("Everything");
        CarDriver.TimeScale = 1;
    }
}