using UnityEngine;
using UnityEngine.InputSystem;

public class Input_Manager : MonoBehaviour
{
    [SerializeField] public static PlayerInput Player; 
    [SerializeField] public static Vector2 movement; 
    [SerializeField] public static bool JumpWpress;
    [SerializeField] public static bool JumpisHeld;
    [SerializeField] public static bool JumpwRelased; 
    [SerializeField] public static bool OnRun; 
    
    private InputAction _MovementHorizontal; 
    private InputAction _Jump; 
    private InputAction _RunAction;

    private void Awake()
    {
        Player = GetComponent<PlayerInput>();
        _MovementHorizontal = Player.actions["move"];
        _Jump = Player.actions["Jump"];
        _RunAction = Player.actions["Run"];
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
