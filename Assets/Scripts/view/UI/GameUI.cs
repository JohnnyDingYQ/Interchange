using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUI: MonoBehaviour
{
    private VisualElement root;
    private static VisualElement unpause;
    private static VisualElement pause;
    private const float animationDuration = 1;
    readonly WaitForSeconds waitAnimationDuration = new(animationDuration);
    Coroutine unpauseAnimation;
    Coroutine pauseAnimation;

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        unpause = root.Q<VisualElement>("start");
        pause = root.Q<VisualElement>("pause");
    }

    public void StartUnpauseAnimation()
    {
        pause.style.display = DisplayStyle.None;
        if (pauseAnimation != null)
            StopCoroutine(pauseAnimation);
        unpauseAnimation = StartCoroutine(UnpauseAnimation());
    }

    public void StartPauseAnimation()
    {
        unpause.style.display = DisplayStyle.None;
        if (unpauseAnimation != null)
            StopCoroutine(unpauseAnimation);
        pauseAnimation = StartCoroutine(PauseAnimation());
    }

    IEnumerator UnpauseAnimation()
    {
        unpause.style.display = DisplayStyle.Flex;
        yield return waitAnimationDuration;
        unpause.style.display = DisplayStyle.None;
    }

    IEnumerator PauseAnimation()
    {
        pause.style.display = DisplayStyle.Flex;
        yield return waitAnimationDuration;
        pause.style.display = DisplayStyle.None;
    }
}