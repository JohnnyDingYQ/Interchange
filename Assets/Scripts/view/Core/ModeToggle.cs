using UnityEngine;

public class ModeToggle : MonoBehaviour
{
    [SerializeField]
    GameUI gameUI;

    bool IsInBuildMode { get; set; }

    void Start()
    {
        IsInBuildMode = true;
        ToggleMode();
    }

    public void ToggleMode()
    {
        if (IsInBuildMode)
            SwitchToViewMode();
        else
            SwitchToBuildMode();
        Game.BuildModeOn = IsInBuildMode;
    }

    public void SwitchToBuildMode()
    {
        Camera.main.cullingMask = ~(1 << LayerMask.NameToLayer("Cars"));
        CarDriver.TimeScale = 0;
        gameUI.StartPauseAnimation();
        IsInBuildMode = true;
    }

    public void SwitchToViewMode()
    {
        Camera.main.cullingMask = LayerMask.NameToLayer("Everything");
        CarDriver.TimeScale = 1;
        Build.ResetSelection();
        gameUI.StartUnpauseAnimation();
        IsInBuildMode = false;
    }
}