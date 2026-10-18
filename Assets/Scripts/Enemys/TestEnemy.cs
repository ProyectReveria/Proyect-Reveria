using UnityEngine;
using reveria_Stadistics_Structs;
public class TestEnemy : MonoBehaviour
{
    #region Variables
    [Header ("Referenicas")]
    [SerializeField] private EnemyStats Enemy_Stats; 
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private Collider2D FeetCollider; 
    [SerializeField] private GameObject Enemy; 
    [SerializeField] private Rigidbody2D Enemy_Rickbody; 
    [Header ("Posicion")]
    [Range(0f, 1f)] public float Enemy_MoveThreshold = 0.25f; 
    [SerializeField] private bool _RigthView;
    private Vector2 Enemy_Position; 
    private Vector2 EnemyGravity; 
    [SerializeField] private LayerMask floor; 
    [SerializeField,Range (0.1f,0.6f)] float EnemyTimeMoving =0.6f; //cuantas unidades debe seguir antes de girarse



    //Si el enemigo esta en el piso o no.
    private bool _Istouchingthefloor;
    #endregion

    #region Unity Update


    void fixedDeltaTime()
    {
        Enemy_Position = Enemy.transform.position; 

        if (Enemy_Stats.T_U_P_C.Enemy_Type == StructureLibrary.Enemy_Type.Test_Enemy)
        {
            overload_MovementFunction();
        }
        
    }

    #endregion

    #region Movement
    void overload_MovementFunction()
    {
        float time = Time.fixedDeltaTime; 
        float Velocity = Enemy_Stats.T_U_P_C.Enemy_Speed ;
        float HorizontalSpeed = 0; 

        //Esta tocando el suelo?
        RaycastHit2D hit = Physics2D.Raycast(FeetCollider.transform.position, Vector2.down, 0.72f, floor);
        if (hit.collider.IsTouchingLayers(floor))
        {

            _Istouchingthefloor = true;
        } 
        else if (!hit.collider.IsTouchingLayers(floor))
        {

            _Istouchingthefloor = false;
        } else if (hit.collider.IsTouchingLayers())
        {
            Overload_turn(!_RigthView);
            return;
        }

        if (_Istouchingthefloor)
        {
            HorizontalSpeed = Mathf.Lerp(HorizontalSpeed, Velocity, EnemyTimeMoving * time);


        } else if (!_Istouchingthefloor)
        {
            HorizontalSpeed = 0; 
            return;
        }

    }

        void Overload_turn(bool T)
    {
        ///<Aqui va una animacion de 'Giro' (cambio de direccion, opcional)

        if (T)
        {
            transform.Rotate(0f, 180f,0f); 
        } else         
        {
            transform.Rotate(0f, -180f,0f); 
        }
    }

    #endregion

}
