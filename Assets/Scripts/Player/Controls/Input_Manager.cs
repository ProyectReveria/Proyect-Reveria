using UnityEngine;
using UnityEngine.InputSystem;

public class Input_Manager : MonoBehaviour
{
    public static PlayerInput Player; 
    public static Vector2 movement; 
    public static bool JumpWpress;
    public static bool JumpisHeld;
    public static bool JumpwRelased; 
    public static bool Interact_WasPress;
    public static bool OnRun; 
    public static bool DashWpress; 
    public static bool AttackWassPressThisFrame; 
    public static bool _OnChangeCharacterWasPressThisFrame;
    public static bool _OnDropDownPlatformEvent;
    
    private InputAction _MovementHorizontal; 
    private InputAction _Jump; 
    private InputAction _RunAction;
    private InputAction _Dash;
    private InputAction _Interact; 

    private InputAction _Attack; 
    private InputAction _OnChangeCharacter;
    private InputAction _OnDropDownPlatform;

    private void Awake()
    {
        Player = GetComponent<PlayerInput>();
        _MovementHorizontal = Player.actions["Move"];
        _Jump = Player.actions["Jump"];
        _RunAction = Player.actions["Sprint"];
        _Interact = Player.actions["Interact"];
        _Dash = Player.actions["Dash"]; 
        _Attack = Player.actions["Attack"];
        _OnChangeCharacter = Player.actions["ChangeCharacter"];
        _OnDropDownPlatform = Player.actions["DropDownPlatform"];
        
    }

    private void Update()
    {
        movement = _MovementHorizontal.ReadValue<Vector2>(); 

        JumpWpress = _Jump.WasPressedThisFrame();
        JumpisHeld = _Jump.IsPressed();
        JumpwRelased = _Jump.WasReleasedThisFrame();
        
        OnRun = _RunAction.IsPressed();
        Interact_WasPress = _Interact.WasPressedThisFrame();
        DashWpress = _Dash.WasPressedThisFrame(); 
        AttackWassPressThisFrame = _Attack.WasPressedThisFrame(); 
        _OnChangeCharacterWasPressThisFrame = _OnChangeCharacter.WasPressedThisFrame();
        _OnDropDownPlatformEvent = _OnDropDownPlatform.WasPressedThisFrame();

        
    }
}
