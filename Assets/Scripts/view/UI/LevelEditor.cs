using System.Collections;
using NUnit.Framework;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LevelEditor : MonoBehaviour
{
    VisualElement root;
    Button setSource, hightlightSources, setTarget, hightlightTargets, setInnate, highLightInnates, setBoundaryCenter, displayBoundary;
    Button save, load, allRoadsInnate;
    FloatField boundaryRadiusField;
    Toggle boundCamera;
    bool shouldSetSource, shouldSetTarget, shouldSetInnate, shouldSetBoundaryCenter;
    readonly WaitForSeconds waitAnimationDuration = new(3);
    public static bool selected;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<MouseEnterEvent>(evt => InputSystem.MouseIsInGameWorld = false);
        root.RegisterCallback<MouseLeaveEvent>(evt => InputSystem.MouseIsInGameWorld = true);

        setSource = root.Q<Button>("setSource");
        hightlightSources = root.Q<Button>("hightlightSources");
        setTarget = root.Q<Button>("setTarget");
        hightlightTargets = root.Q<Button>("hightlightTargets");
        setInnate = root.Q<Button>("setInnate");
        highLightInnates = root.Q<Button>("highLightInnates");
        allRoadsInnate = root.Q<Button>("allRoadsInnate");
        setBoundaryCenter = root.Q<Button>("setBoundaryCenter");
        displayBoundary = root.Q<Button>("displayBoundary");
        boundaryRadiusField = root.Q<FloatField>("boundaryRadiusField");
        boundCamera = root.Q<Toggle>("boundCamera");
        save = root.Q<Button>("save");
        load = root.Q<Button>("load");

        setSource.RegisterCallback((ClickEvent evt) => { ResetAll(); shouldSetSource = true; });
        setTarget.RegisterCallback((ClickEvent evt) => { ResetAll(); shouldSetTarget = true; });
        setBoundaryCenter.RegisterCallback((ClickEvent evt) => { ResetAll(); shouldSetBoundaryCenter = true; });
        setInnate.RegisterCallback((ClickEvent evt) => { ResetAll(); shouldSetInnate = true; });
        hightlightSources.RegisterCallback((ClickEvent evt) => StartCoroutine(TempHighLight(RoadProp.InnateSource)));
        hightlightTargets.RegisterCallback((ClickEvent evt) => StartCoroutine(TempHighLight(RoadProp.InnateTarget)));
        highLightInnates.RegisterCallback((ClickEvent evt) => StartCoroutine(TempHighLight(RoadProp.Innate)));
        displayBoundary.RegisterCallback((ClickEvent evt) => Gizmos.DrawCircle(
            new(Game.BoundaryCenter.x, 0, Game.BoundaryCenter.y), Game.BoundaryRadius, 20, Color.cyan, 3
        ));
        boundCamera.RegisterCallback<ChangeEvent<bool>>((evt) => Game.CameraBoundOn = evt.newValue);

        allRoadsInnate.RegisterCallback((ClickEvent evt) => Debug.Log("All roads innate: " + AllRoadsAreInnate()));

        boundaryRadiusField.RegisterValueChangedCallback(evt =>
        {
            Game.BoundaryRadius = evt.newValue;
            Debug.Log($"Level Editor: boundary radius set to {evt.newValue}");
        });

        void ResetAll()
        {
            shouldSetSource = false;
            shouldSetTarget = false;
            shouldSetBoundaryCenter = false;
            shouldSetInnate = false;
        }

        bool AllRoadsAreInnate()
        {
            foreach (Road road in Game.Roads.Values)
            {
                RoadProp roadProp = road.RoadProp;
                if (roadProp != RoadProp.Innate && roadProp != RoadProp.InnateSource && roadProp != RoadProp.InnateTarget)
                    return false;
            }
            return true;
        }

        IEnumerator TempHighLight(RoadProp roadProp)
        {
            Assert.True(roadProp == RoadProp.Innate || roadProp == RoadProp.InnateSource || roadProp == RoadProp.InnateTarget);
            foreach (Road road in Game.Roads.Values)
                if (road.RoadProp == roadProp)
                    Roads.Highlight(road);
            yield return waitAnimationDuration;
            foreach (Road road in Game.Roads.Values)
                if (road.RoadProp == roadProp)
                    Roads.Unhighlight(road);
        }
    }

    void Update()
    {
        if (selected)
        {
            selected = false;
            if (Game.HoveredRoad != null)
            {
                if (shouldSetSource)
                {
                    Game.HoveredRoad.RoadProp = RoadProp.InnateSource;
                    Debug.Log("Level Editor: Road set to source");
                    return;
                }
                if (shouldSetTarget)
                {
                    Game.HoveredRoad.RoadProp = RoadProp.InnateTarget;
                    Debug.Log("Level Editor: Road set to target");
                    return;
                }
                if (shouldSetInnate)
                {
                    Game.HoveredRoad.RoadProp = RoadProp.Innate;
                    Debug.Log("Level Editor: Road set to innate");
                    return;
                }
            }
            if (shouldSetBoundaryCenter)
            {
                Game.BoundaryCenter = new(InputSystem.MouseWorldPos.x, InputSystem.MouseWorldPos.z);
                Debug.Log($"Level Editor: boundary center set to {Game.BoundaryCenter}");
                return;
            }

        }
    }


    public static void SetSelected()
    {
        selected = true;
    }
}