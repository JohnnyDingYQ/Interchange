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
                    ""interactions"": """",
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
    }
}