//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Scripts/GameActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @GameActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @GameActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""GameActions"",
    ""maps"": [
        {
            ""name"": ""InGame"",
            ""id"": ""1ddd219f-30cd-4baa-8e8f-90edbcec54b4"",
            ""actions"": [
                {
                    ""name"": ""MoveCamera"",
                    ""type"": ""Value"",
                    ""id"": ""095c05d6-08ac-473d-911d-c69afd2d4d91"",
                    ""expectedControlType"": ""Vector3"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Build"",
                    ""type"": ""Button"",
                    ""id"": ""3d2e49d8-a3e2-4c8e-9d2b-f0d113fa15ee"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Tap"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SetLaneWidthTo1"",
                    ""type"": ""Button"",
                    ""id"": ""e3d14fe2-fae1-435b-9de9-6165c6f1406a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SetLaneWidthTo2"",
                    ""type"": ""Button"",
                    ""id"": ""8b939ddd-fa55-4c30-830a-873b653917da"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SetLaneWidthTo3"",
                    ""type"": ""Button"",
                    ""id"": ""d5df3627-5ede-44a8-ae0c-f301de14767a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SaveGame"",
                    ""type"": ""Button"",
                    ""id"": ""ee5f7868-3d5b-4fce-aaae-08b0d5cbd339"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""LoadGame"",
                    ""type"": ""Button"",
                    ""id"": ""bdd5d218-0a05-40b7-87bc-f1b845fddf08"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""DivideRoad"",
                    ""type"": ""Button"",
                    ""id"": ""887c1d3b-fdc0-480b-b3c5-4e85fd0cdcd5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""RemoveRoad"",
                    ""type"": ""Button"",
                    ""id"": ""80942f95-8132-432e-aa8b-38c487f29c31"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AbandonBuild"",
                    ""type"": ""Button"",
                    ""id"": ""f313cc28-09f8-48e3-bbd6-feb27caf5592"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ParallelSpacingDrag"",
                    ""type"": ""Button"",
                    ""id"": ""676f582d-b29b-4a4a-8d86-c26f57cdb8e5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""DragCamera"",
                    ""type"": ""Button"",
                    ""id"": ""b37da4a0-cfbf-49f6-ac50-c78e369e4621"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ToggleParallelBuildMode"",
                    ""type"": ""Button"",
                    ""id"": ""354b1c2c-d354-4896-bb11-dbcdc1c9205e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""LevelEditorSelect"",
                    ""type"": ""Button"",
                    ""id"": ""cd1fb85d-9bfc-42eb-9ae7-24fb551a6bb8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""BulkSelect"",
                    ""type"": ""Button"",
                    ""id"": ""99aae0c1-9675-4abe-bc25-ae19dcb7f1c5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Hold(duration=0.25)"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""DecreaseElevation"",
                    ""type"": ""Button"",
                    ""id"": ""6d5ae660-34f7-4554-8f69-a764e81bdf22"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""IncreaseElevation"",
                    ""type"": ""Button"",
                    ""id"": ""36eff879-62ea-412a-82cd-42db15916346"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""StraightMode"",
                    ""type"": ""Button"",
                    ""id"": ""cab08211-240a-4067-8f8b-5e9aedaf161f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SpinCamera"",
                    ""type"": ""Value"",
                    ""id"": ""aa9b8520-e13e-4fd5-81a7-65db48eb351a"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""7298c412-af08-40c2-9d8d-b43e6400c512"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Build"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""036c33a6-d60f-447e-bb9d-e53f20f46030"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetLaneWidthTo1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""3D Vector"",
                    ""id"": ""594fff7b-1463-408e-862c-a25d39d2828a"",
                    ""path"": ""3DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""d6931a97-9768-44d6-957b-2a0d52e8832d"",
                    ""path"": ""<Mouse>/scroll/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""4527604f-5e27-4046-bc8b-fc853247ef4f"",
                    ""path"": ""<Mouse>/scroll/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""10f23556-59fb-4d57-a3b8-cebe6ce53cee"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""46d719e4-519f-49b3-800d-62740fd55dd6"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""forward"",
                    ""id"": ""bf389f64-2a73-4744-93a6-6ca10e5363d2"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""backward"",
                    ""id"": ""2933f7f8-4d06-4d65-b311-7f1b562201d2"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""994d0935-103d-4fc5-bc6e-c0b24d6fc956"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetLaneWidthTo2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""20c7840a-7b81-4404-b74c-f921369194b4"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SetLaneWidthTo3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a7904c07-9415-4eb0-9d11-9cd2ac292f15"",
                    ""path"": ""<Keyboard>/o"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SaveGame"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3c5d306d-99fa-4663-8bc1-052e630aae2b"",
                    ""path"": ""<Keyboard>/p"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LoadGame"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9cf8bcdb-8c9c-4afd-80d2-b9afbb7fafa2"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DivideRoad"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""89218214-cf9e-49aa-9d99-a9b8961089e6"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RemoveRoad"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cf87bb18-b175-4013-afdc-7869fa33628f"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AbandonBuild"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e8929b3b-afa8-4bbd-8afc-bf97bc86de06"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ParallelSpacingDrag"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""00a3e5ab-669c-4301-a5fd-f9e13a3f6127"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DragCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""41e7c391-159f-4531-80d9-bcc1ccdc366f"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleParallelBuildMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b2d9ddd9-d625-4afb-8b4b-f843b4506154"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LevelEditorSelect"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""543f1518-d923-4e6a-a778-1eca6be43005"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""BulkSelect"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2ca46d08-7aa0-42c6-b4d4-a148b1c31a4f"",
                    ""path"": ""<Mouse>/forwardButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""IncreaseElevation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d76672b9-cae0-4dbb-8407-77ff724ffe13"",
                    ""path"": ""<Keyboard>/period"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""IncreaseElevation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""924d7ac7-d01e-4a80-a91b-29b23a523fe3"",
                    ""path"": ""<Keyboard>/comma"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DecreaseElevation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cbf8a344-c484-4995-ad4f-7f7cef647cb7"",
                    ""path"": ""<Mouse>/backButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DecreaseElevation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""52466241-c35d-4513-ba50-be304d5f0974"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""StraightMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""9a12f042-de2e-4127-b678-2f0a8cd88414"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpinCamera"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""ffc5417c-6e9d-4101-802f-c51196d0b1ec"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpinCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""5a867df0-8fde-46fb-89ff-c0388abb8ec5"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpinCamera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // InGame
        m_InGame = asset.FindActionMap("InGame", throwIfNotFound: true);
        m_InGame_MoveCamera = m_InGame.FindAction("MoveCamera", throwIfNotFound: true);
        m_InGame_Build = m_InGame.FindAction("Build", throwIfNotFound: true);
        m_InGame_SetLaneWidthTo1 = m_InGame.FindAction("SetLaneWidthTo1", throwIfNotFound: true);
        m_InGame_SetLaneWidthTo2 = m_InGame.FindAction("SetLaneWidthTo2", throwIfNotFound: true);
        m_InGame_SetLaneWidthTo3 = m_InGame.FindAction("SetLaneWidthTo3", throwIfNotFound: true);
        m_InGame_SaveGame = m_InGame.FindAction("SaveGame", throwIfNotFound: true);
        m_InGame_LoadGame = m_InGame.FindAction("LoadGame", throwIfNotFound: true);
        m_InGame_DivideRoad = m_InGame.FindAction("DivideRoad", throwIfNotFound: true);
        m_InGame_RemoveRoad = m_InGame.FindAction("RemoveRoad", throwIfNotFound: true);
        m_InGame_AbandonBuild = m_InGame.FindAction("AbandonBuild", throwIfNotFound: true);
        m_InGame_ParallelSpacingDrag = m_InGame.FindAction("ParallelSpacingDrag", throwIfNotFound: true);
        m_InGame_DragCamera = m_InGame.FindAction("DragCamera", throwIfNotFound: true);
        m_InGame_ToggleParallelBuildMode = m_InGame.FindAction("ToggleParallelBuildMode", throwIfNotFound: true);
        m_InGame_LevelEditorSelect = m_InGame.FindAction("LevelEditorSelect", throwIfNotFound: true);
        m_InGame_BulkSelect = m_InGame.FindAction("BulkSelect", throwIfNotFound: true);
        m_InGame_DecreaseElevation = m_InGame.FindAction("DecreaseElevation", throwIfNotFound: true);
        m_InGame_IncreaseElevation = m_InGame.FindAction("IncreaseElevation", throwIfNotFound: true);
        m_InGame_StraightMode = m_InGame.FindAction("StraightMode", throwIfNotFound: true);
        m_InGame_SpinCamera = m_InGame.FindAction("SpinCamera", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // InGame
    private readonly InputActionMap m_InGame;
    private List<IInGameActions> m_InGameActionsCallbackInterfaces = new List<IInGameActions>();
    private readonly InputAction m_InGame_MoveCamera;
    private readonly InputAction m_InGame_Build;
    private readonly InputAction m_InGame_SetLaneWidthTo1;
    private readonly InputAction m_InGame_SetLaneWidthTo2;
    private readonly InputAction m_InGame_SetLaneWidthTo3;
    private readonly InputAction m_InGame_SaveGame;
    private readonly InputAction m_InGame_LoadGame;
    private readonly InputAction m_InGame_DivideRoad;
    private readonly InputAction m_InGame_RemoveRoad;
    private readonly InputAction m_InGame_AbandonBuild;
    private readonly InputAction m_InGame_ParallelSpacingDrag;
    private readonly InputAction m_InGame_DragCamera;
    private readonly InputAction m_InGame_ToggleParallelBuildMode;
    private readonly InputAction m_InGame_LevelEditorSelect;
    private readonly InputAction m_InGame_BulkSelect;
    private readonly InputAction m_InGame_DecreaseElevation;
    private readonly InputAction m_InGame_IncreaseElevation;
    private readonly InputAction m_InGame_StraightMode;
    private readonly InputAction m_InGame_SpinCamera;
    public struct InGameActions
    {
        private @GameActions m_Wrapper;
        public InGameActions(@GameActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @MoveCamera => m_Wrapper.m_InGame_MoveCamera;
        public InputAction @Build => m_Wrapper.m_InGame_Build;
        public InputAction @SetLaneWidthTo1 => m_Wrapper.m_InGame_SetLaneWidthTo1;
        public InputAction @SetLaneWidthTo2 => m_Wrapper.m_InGame_SetLaneWidthTo2;
        public InputAction @SetLaneWidthTo3 => m_Wrapper.m_InGame_SetLaneWidthTo3;
        public InputAction @SaveGame => m_Wrapper.m_InGame_SaveGame;
        public InputAction @LoadGame => m_Wrapper.m_InGame_LoadGame;
        public InputAction @DivideRoad => m_Wrapper.m_InGame_DivideRoad;
        public InputAction @RemoveRoad => m_Wrapper.m_InGame_RemoveRoad;
        public InputAction @AbandonBuild => m_Wrapper.m_InGame_AbandonBuild;
        public InputAction @ParallelSpacingDrag => m_Wrapper.m_InGame_ParallelSpacingDrag;
        public InputAction @DragCamera => m_Wrapper.m_InGame_DragCamera;
        public InputAction @ToggleParallelBuildMode => m_Wrapper.m_InGame_ToggleParallelBuildMode;
        public InputAction @LevelEditorSelect => m_Wrapper.m_InGame_LevelEditorSelect;
        public InputAction @BulkSelect => m_Wrapper.m_InGame_BulkSelect;
        public InputAction @DecreaseElevation => m_Wrapper.m_InGame_DecreaseElevation;
        public InputAction @IncreaseElevation => m_Wrapper.m_InGame_IncreaseElevation;
        public InputAction @StraightMode => m_Wrapper.m_InGame_StraightMode;
        public InputAction @SpinCamera => m_Wrapper.m_InGame_SpinCamera;
        public InputActionMap Get() { return m_Wrapper.m_InGame; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(InGameActions set) { return set.Get(); }
        public void AddCallbacks(IInGameActions instance)
        {
            if (instance == null || m_Wrapper.m_InGameActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_InGameActionsCallbackInterfaces.Add(instance);
            @MoveCamera.started += instance.OnMoveCamera;
            @MoveCamera.performed += instance.OnMoveCamera;
            @MoveCamera.canceled += instance.OnMoveCamera;
            @Build.started += instance.OnBuild;
            @Build.performed += instance.OnBuild;
            @Build.canceled += instance.OnBuild;
            @SetLaneWidthTo1.started += instance.OnSetLaneWidthTo1;
            @SetLaneWidthTo1.performed += instance.OnSetLaneWidthTo1;
            @SetLaneWidthTo1.canceled += instance.OnSetLaneWidthTo1;
            @SetLaneWidthTo2.started += instance.OnSetLaneWidthTo2;
            @SetLaneWidthTo2.performed += instance.OnSetLaneWidthTo2;
            @SetLaneWidthTo2.canceled += instance.OnSetLaneWidthTo2;
            @SetLaneWidthTo3.started += instance.OnSetLaneWidthTo3;
            @SetLaneWidthTo3.performed += instance.OnSetLaneWidthTo3;
            @SetLaneWidthTo3.canceled += instance.OnSetLaneWidthTo3;
            @SaveGame.started += instance.OnSaveGame;
            @SaveGame.performed += instance.OnSaveGame;
            @SaveGame.canceled += instance.OnSaveGame;
            @LoadGame.started += instance.OnLoadGame;
            @LoadGame.performed += instance.OnLoadGame;
            @LoadGame.canceled += instance.OnLoadGame;
            @DivideRoad.started += instance.OnDivideRoad;
            @DivideRoad.performed += instance.OnDivideRoad;
            @DivideRoad.canceled += instance.OnDivideRoad;
            @RemoveRoad.started += instance.OnRemoveRoad;
            @RemoveRoad.performed += instance.OnRemoveRoad;
            @RemoveRoad.canceled += instance.OnRemoveRoad;
            @AbandonBuild.started += instance.OnAbandonBuild;
            @AbandonBuild.performed += instance.OnAbandonBuild;
            @AbandonBuild.canceled += instance.OnAbandonBuild;
            @ParallelSpacingDrag.started += instance.OnParallelSpacingDrag;
            @ParallelSpacingDrag.performed += instance.OnParallelSpacingDrag;
            @ParallelSpacingDrag.canceled += instance.OnParallelSpacingDrag;
            @DragCamera.started += instance.OnDragCamera;
            @DragCamera.performed += instance.OnDragCamera;
            @DragCamera.canceled += instance.OnDragCamera;
            @ToggleParallelBuildMode.started += instance.OnToggleParallelBuildMode;
            @ToggleParallelBuildMode.performed += instance.OnToggleParallelBuildMode;
            @ToggleParallelBuildMode.canceled += instance.OnToggleParallelBuildMode;
            @LevelEditorSelect.started += instance.OnLevelEditorSelect;
            @LevelEditorSelect.performed += instance.OnLevelEditorSelect;
            @LevelEditorSelect.canceled += instance.OnLevelEditorSelect;
            @BulkSelect.started += instance.OnBulkSelect;
            @BulkSelect.performed += instance.OnBulkSelect;
            @BulkSelect.canceled += instance.OnBulkSelect;
            @DecreaseElevation.started += instance.OnDecreaseElevation;
            @DecreaseElevation.performed += instance.OnDecreaseElevation;
            @DecreaseElevation.canceled += instance.OnDecreaseElevation;
            @IncreaseElevation.started += instance.OnIncreaseElevation;
            @IncreaseElevation.performed += instance.OnIncreaseElevation;
            @IncreaseElevation.canceled += instance.OnIncreaseElevation;
            @StraightMode.started += instance.OnStraightMode;
            @StraightMode.performed += instance.OnStraightMode;
            @StraightMode.canceled += instance.OnStraightMode;
            @SpinCamera.started += instance.OnSpinCamera;
            @SpinCamera.performed += instance.OnSpinCamera;
            @SpinCamera.canceled += instance.OnSpinCamera;
        }

        private void UnregisterCallbacks(IInGameActions instance)
        {
            @MoveCamera.started -= instance.OnMoveCamera;
            @MoveCamera.performed -= instance.OnMoveCamera;
            @MoveCamera.canceled -= instance.OnMoveCamera;
            @Build.started -= instance.OnBuild;
            @Build.performed -= instance.OnBuild;
            @Build.canceled -= instance.OnBuild;
            @SetLaneWidthTo1.started -= instance.OnSetLaneWidthTo1;
            @SetLaneWidthTo1.performed -= instance.OnSetLaneWidthTo1;
            @SetLaneWidthTo1.canceled -= instance.OnSetLaneWidthTo1;
            @SetLaneWidthTo2.started -= instance.OnSetLaneWidthTo2;
            @SetLaneWidthTo2.performed -= instance.OnSetLaneWidthTo2;
            @SetLaneWidthTo2.canceled -= instance.OnSetLaneWidthTo2;
            @SetLaneWidthTo3.started -= instance.OnSetLaneWidthTo3;
            @SetLaneWidthTo3.performed -= instance.OnSetLaneWidthTo3;
            @SetLaneWidthTo3.canceled -= instance.OnSetLaneWidthTo3;
            @SaveGame.started -= instance.OnSaveGame;
            @SaveGame.performed -= instance.OnSaveGame;
            @SaveGame.canceled -= instance.OnSaveGame;
            @LoadGame.started -= instance.OnLoadGame;
            @LoadGame.performed -= instance.OnLoadGame;
            @LoadGame.canceled -= instance.OnLoadGame;
            @DivideRoad.started -= instance.OnDivideRoad;
            @DivideRoad.performed -= instance.OnDivideRoad;
            @DivideRoad.canceled -= instance.OnDivideRoad;
            @RemoveRoad.started -= instance.OnRemoveRoad;
            @RemoveRoad.performed -= instance.OnRemoveRoad;
            @RemoveRoad.canceled -= instance.OnRemoveRoad;
            @AbandonBuild.started -= instance.OnAbandonBuild;
            @AbandonBuild.performed -= instance.OnAbandonBuild;
            @AbandonBuild.canceled -= instance.OnAbandonBuild;
            @ParallelSpacingDrag.started -= instance.OnParallelSpacingDrag;
            @ParallelSpacingDrag.performed -= instance.OnParallelSpacingDrag;
            @ParallelSpacingDrag.canceled -= instance.OnParallelSpacingDrag;
            @DragCamera.started -= instance.OnDragCamera;
            @DragCamera.performed -= instance.OnDragCamera;
            @DragCamera.canceled -= instance.OnDragCamera;
            @ToggleParallelBuildMode.started -= instance.OnToggleParallelBuildMode;
            @ToggleParallelBuildMode.performed -= instance.OnToggleParallelBuildMode;
            @ToggleParallelBuildMode.canceled -= instance.OnToggleParallelBuildMode;
            @LevelEditorSelect.started -= instance.OnLevelEditorSelect;
            @LevelEditorSelect.performed -= instance.OnLevelEditorSelect;
            @LevelEditorSelect.canceled -= instance.OnLevelEditorSelect;
            @BulkSelect.started -= instance.OnBulkSelect;
            @BulkSelect.performed -= instance.OnBulkSelect;
            @BulkSelect.canceled -= instance.OnBulkSelect;
            @DecreaseElevation.started -= instance.OnDecreaseElevation;
            @DecreaseElevation.performed -= instance.OnDecreaseElevation;
            @DecreaseElevation.canceled -= instance.OnDecreaseElevation;
            @IncreaseElevation.started -= instance.OnIncreaseElevation;
            @IncreaseElevation.performed -= instance.OnIncreaseElevation;
            @IncreaseElevation.canceled -= instance.OnIncreaseElevation;
            @StraightMode.started -= instance.OnStraightMode;
            @StraightMode.performed -= instance.OnStraightMode;
            @StraightMode.canceled -= instance.OnStraightMode;
            @SpinCamera.started -= instance.OnSpinCamera;
            @SpinCamera.performed -= instance.OnSpinCamera;
            @SpinCamera.canceled -= instance.OnSpinCamera;
        }

        public void RemoveCallbacks(IInGameActions instance)
        {
            if (m_Wrapper.m_InGameActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IInGameActions instance)
        {
            foreach (var item in m_Wrapper.m_InGameActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_InGameActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public InGameActions @InGame => new InGameActions(this);
    public interface IInGameActions
    {
        void OnMoveCamera(InputAction.CallbackContext context);
        void OnBuild(InputAction.CallbackContext context);
        void OnSetLaneWidthTo1(InputAction.CallbackContext context);
        void OnSetLaneWidthTo2(InputAction.CallbackContext context);
        void OnSetLaneWidthTo3(InputAction.CallbackContext context);
        void OnSaveGame(InputAction.CallbackContext context);
        void OnLoadGame(InputAction.CallbackContext context);
        void OnDivideRoad(InputAction.CallbackContext context);
        void OnRemoveRoad(InputAction.CallbackContext context);
        void OnAbandonBuild(InputAction.CallbackContext context);
        void OnParallelSpacingDrag(InputAction.CallbackContext context);
        void OnDragCamera(InputAction.CallbackContext context);
        void OnToggleParallelBuildMode(InputAction.CallbackContext context);
        void OnLevelEditorSelect(InputAction.CallbackContext context);
        void OnBulkSelect(InputAction.CallbackContext context);
        void OnDecreaseElevation(InputAction.CallbackContext context);
        void OnIncreaseElevation(InputAction.CallbackContext context);
        void OnStraightMode(InputAction.CallbackContext context);
        void OnSpinCamera(InputAction.CallbackContext context);
    }
}
