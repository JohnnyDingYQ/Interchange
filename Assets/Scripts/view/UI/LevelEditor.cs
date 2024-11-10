using System.Collections;
using NUnit.Framework;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LevelEditor : MonoBehaviour
{
    [SerializeField]
    Main main;
    VisualElement root;
    Button setSource, hightlightSources, setTarget, hightlightTargets, setInnate, highLightInnates, setBoundaryCenter, displayBoundary;
    Button save, load, allRoadsInnate, exitEditor;
    FloatField boundaryRadiusField;
    IntegerField levelField;
    Toggle boundCamera;
    bool shouldSetSource, shouldSetTarget, shouldSetInnate, shouldSetBoundaryCenter;
    readonly WaitForSeconds waitAnimationDuration = new(3);
    int level;
    public static bool selected;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        


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
        levelField = root.Q<IntegerField>("levelField");
        save = root.Q<Button>("save");
        load = root.Q<Button>("load");
        exitEditor = root.Q<Button>("exitEditor");

        setSource.RegisterCallback((ClickEvent evt) => { ResetTriggers(); shouldSetSource = true; });
        setTarget.RegisterCallback((ClickEvent evt) => { ResetTriggers(); shouldSetTarget = true; });
        setBoundaryCenter.RegisterCallback((ClickEvent evt) => { ResetTriggers(); shouldSetBoundaryCenter = true; });
        setInnate.RegisterCallback((ClickEvent evt) => { ResetTriggers(); shouldSetInnate = true; });
        hightlightSources.RegisterCallback((ClickEvent evt) => StartCoroutine(TempHighLight(RoadProp.InnateSource)));
        hightlightTargets.RegisterCallback((ClickEvent evt) => StartCoroutine(TempHighLight(RoadProp.InnateTarget)));
        highLightInnates.RegisterCallback((ClickEvent evt) => StartCoroutine(TempHighLight(RoadProp.Innate)));
        displayBoundary.RegisterCallback((ClickEvent evt) => Gizmos.DrawSquare(
            new(Game.BoundaryCenter.x, 5, Game.BoundaryCenter.y), Game.BoundaryRadius, new(1, 0.65f, 0, 1), 3
        ));
        allRoadsInnate.RegisterCallback((ClickEvent evt) => Debug.Log("All roads innate: " + AllRoadsAreInnate()));
        save.RegisterCallback((ClickEvent evt) =>
        {
            SaveSystem saveSystem = new(Application.dataPath + "/Levels/Saves/" + level);
            saveSystem.SaveGame();
            Debug.Log($"Level saved to {level} ");
        });
        load.RegisterCallback((ClickEvent evt) =>
        {
            SaveSystem saveSystem = new(Application.dataPath + "/Levels/Saves/" + level);
            saveSystem.LoadGame();
            main.ComplyToGameSave();
            Debug.Log($"Loaded level {level} ");
        });
        exitEditor.RegisterCallback((ClickEvent evt) => Game.LevelEditorOn = false);

        boundCamera.RegisterCallback<ChangeEvent<bool>>((evt) =>
        {
            Game.CameraBoundOn = evt.newValue;
            InputSystem.MouseIsInGameWorld = true;
        });

        boundaryRadiusField.RegisterValueChangedCallback(evt =>
        {
            Game.BoundaryRadius = evt.newValue;
            Debug.Log($"Level Editor: boundary radius set to {evt.newValue}");
        });

        levelField.RegisterValueChangedCallback(evt => level = evt.newValue);

        void ResetTriggers()
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

    void OnEnable()
    {
        root.RegisterCallback<MouseEnterEvent>(MouseEnter);
        root.RegisterCallback<MouseLeaveEvent>(MouseLeave);
    }

    void OnDisable()
    {
        root.UnregisterCallback<MouseEnterEvent>(MouseEnter);
        root.UnregisterCallback<MouseLeaveEvent>(MouseLeave);
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

    void MouseEnter(MouseEnterEvent evt)
    {
        InputSystem.MouseIsInGameWorld = false;
    }

    void MouseLeave(MouseLeaveEvent evt)
    {
        InputSystem.MouseIsInGameWorld = true;
    }
}