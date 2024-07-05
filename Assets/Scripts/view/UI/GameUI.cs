using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUI: MonoBehaviour
{
    private VisualElement root;
    private static VisualElement start;
    private static VisualElement pause;
    private const float animationDuration = 1;
    WaitForSeconds waitAnimationDuration = new(animationDuration);

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        start = root.Q<VisualElement>("start");
        pause = root.Q<VisualElement>("pause");
    }

    public void ShowStartIcon()
    {
        StartCoroutine(StartIconCoroutine());
    }

    public void ShowStopIcon()
    {
        StartCoroutine(PauseIconCoroutine());
    }

    IEnumerator StartIconCoroutine()
    {
        start.style.display = DisplayStyle.Flex;
        yield return waitAnimationDuration;
        start.style.display = DisplayStyle.None;
    }

    IEnumerator PauseIconCoroutine()
    {
        pause.style.display = DisplayStyle.Flex;
        yield return waitAnimationDuration;
        pause.style.display = DisplayStyle.None;
    }
}