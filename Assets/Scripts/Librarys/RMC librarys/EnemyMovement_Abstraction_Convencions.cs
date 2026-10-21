using UnityEngine; 
using System;
    //Credits To: Uriel Gomez madriz And Proyect Reveria Github Tools
public abstract class Enemy_Movement_BaseBahivior : MonoBehaviour, Enemy_interaction_Library
{
 #region Variables
    [Header ("Referenicas")]

    //Si alguna estadistica no esta aca probablemente esta en enemy Stats
    [SerializeField] protected EnemyStats Enemy_Stats; 
    [SerializeField] protected Collider2D bodyCollider;
    [SerializeField] protected Collider2D FeetCollider; 
    [SerializeField] protected GameObject Enemy; 
    [SerializeField] protected Rigidbody2D Enemy_Rickbody; 
    [Header ("Posicion")]
    [Range(0f, 1f)] public float Enemy_MoveThreshold = 0.25f; 
    [SerializeField] protected bool _RigthView;
    protected Vector2 Enemy_Position; 
    protected Vector2 EnemyGravity; 
    [SerializeField] protected LayerMask floor; 
    [SerializeField,Range (0.1f,0.6f)] protected float EnemyTimeMoving =0.1f; //cuantas unidades debe seguir antes de girarse
    [SerializeField] protected float WallCheckDistance = 0.1f; 

    protected float HorizontalSpeed = 0f; 
    protected float VerticalVelocity = 0f;

    protected float aceleration; 
    protected float _edgedetection;
    protected float _velocity;


    //Si el enemigo esta en el piso o no.
    protected bool _Istouchingthefloor;
    #endregion

    #region Unity Update

    protected virtual void Awake()
    {
        //DO NOT PUT THIS ON FIXED UPDATE
        UpdateData();

    }

    protected virtual void UpdateData()
    {
        //DO NOT PUT THIS ON FIXED UPDATE
        Enemy_Position = Enemy.transform.position; 
        HPStatus_Enemy = Enemy_Stats.T_U_P_C.EnemyHP; 
        aceleration = Enemy_Stats.T_U_P_C.Enemy_Aceleration;
        _edgedetection = Enemy_Stats.T_U_P_C.Enemy_LedgeCheckoofset;
        _velocity = Enemy_Stats.T_U_P_C.Enemy_Speed;
    }

    protected virtual void FixedUpdate()
    {
            overload_MovementFunction();
    }

    #endregion

    #region Movement

    public float HPStatus_Enemy;
   protected virtual void overload_MovementFunction()
    {
        
        float Direction = _RigthView ? 1f : -1f;

        VerticalVelocity = Enemy_Rickbody.linearVelocity.y;

        //feetcolider
        Bounds feet = FeetCollider.bounds;
        Vector2 FootOrigin = new Vector2(feet.center.x, feet.min.y + 0.05f);

        RaycastHit2D hit = Physics2D.Raycast(FootOrigin, Vector2.down, 0.2f, floor);
        _Istouchingthefloor = hit.collider != null; 

        if (_Istouchingthefloor)
        {
            //Estos son cajas de Raycast, esot significa que el Collider en si dependede ellas para detectar cosas
            Vector2 FrontFoot = new Vector2(feet.center.x + Direction * (feet.extents.x + _edgedetection), feet.min.y + 0.05f);
            RaycastHit2D LedgeHit = Physics2D.Raycast(FrontFoot, Vector2.down, 0.2f, floor);

            Vector2 FrontBody = new Vector2(bodyCollider.bounds.center.x + Direction * bodyCollider.bounds.extents.x, bodyCollider.bounds.center.y);
            RaycastHit2D WallHit = Physics2D.Raycast(FrontBody, Vector2.right * Direction, WallCheckDistance, floor);

            if (LedgeHit.collider == null || WallHit.collider != null)
            {
                HorizontalSpeed = 0f;
                Enemy_Rickbody.linearVelocity = new Vector2 (0f,VerticalVelocity);
                Overload_turn(!_RigthView);
                return;
            }
            //Esta formula es casi unviersal entre enemigos
            HorizontalSpeed = Mathf.Lerp(HorizontalSpeed, _velocity, aceleration * Time.fixedDeltaTime);
        }
        else
        {
            HorizontalSpeed = 0; 
        }

        Enemy_Rickbody.linearVelocity = new Vector2 (HorizontalSpeed * Direction, VerticalVelocity);
    }
       protected virtual void Overload_turn(bool T)
    {


        _RigthView = T; 

        if (T)
        {
            Enemy.transform.Rotate(0f, 180f,0f); 
        } else         
        {
            Enemy.transform.Rotate(0f, -180f,0f); 
        }
    }

    #endregion

    #region  Contracts & convencions
    public void _EnemyReciveDamage_Contract(float Damage)
    {
        HPStatus_Enemy -= Damage;
    }
    #endregion

}