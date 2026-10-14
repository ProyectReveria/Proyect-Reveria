using UnityEngine;
using UnityEngine.InputSystem;

public class Input_Manager : MonoBehaviour
{
    public static PlayerInput Player; 
    public static Vector2 movement; 
    public static bool JumpWpress;
    public static bool JumpisHeld;
    public static bool JumpwRelased; 
    public static bool OnRun; 
    
    private InputAction _MovementHorizontal; 
    private InputAction _Jump; 
    private InputAction _RunAction;

    private void Awake()
    {
        Player = GetComponent<PlayerInput>();
        _MovementHorizontal = Player.actions["Move"];
        _Jump = Player.actions["Jump"];
        _RunAction = Player.actions["Sprint"];
    }

    private void Update()
    {
        movement = _MovementHorizontal.ReadValue<Vector2>(); 

        JumpWpress = _Jump.WasPressedThisFrame();
        JumpisHeld = _Jump.IsPressed();
        JumpwRelased = _Jump.WasReleasedThisFrame();
        
        OnRun = _RunAction.IsPressed();
    }
}
