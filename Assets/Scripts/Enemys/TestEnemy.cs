using UnityEngine;
using reveria_Stadistics_Structs;
public class TestEnemy : MonoBehaviour
{
    #region Variables
    [Header ("Referenicas")]

    //Si alguna estadistica no esta aca probablemente esta en enemy Stats
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
    [SerializeField,Range (0.1f,0.6f)] float EnemyTimeMoving =0.1f; //cuantas unidades debe seguir antes de girarse
    [SerializeField] private float WallCheckDistance = 0.1f; 

    float HorizontalSpeed = 0f; 
    float VerticalVelocity = 0f;


    //Si el enemigo esta en el piso o no.
    private bool _Istouchingthefloor;
    #endregion

    #region Unity Update

    void Awake()
    {
        //NO METAS ESTO EN EL FIXED UPDATE NO NO ES BUENA IDEA Y NO NO FUNCIONA
        Enemy_Position = Enemy.transform.position; 
    }

    void FixedUpdate()
    {
            overload_MovementFunction();
    }

    #endregion

    #region Movement
   void overload_MovementFunction()
    {
        float Velocity = Enemy_Stats.T_U_P_C.Enemy_Speed ;
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
            Vector2 FrontFoot = new Vector2(feet.center.x + Direction * (feet.extents.x + Enemy_Stats.T_U_P_C.Enemy_LedgeCheckoofset), feet.min.y + 0.05f);
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
            HorizontalSpeed = Mathf.Lerp(HorizontalSpeed, Velocity, Enemy_Stats.T_U_P_C.Enemy_Aceleration * Time.fixedDeltaTime);
        }
        else
        {
            HorizontalSpeed = 0; 
        }

        Enemy_Rickbody.linearVelocity = new Vector2 (HorizontalSpeed * Direction, VerticalVelocity);
    }
        void Overload_turn(bool T)
    {


        _RigthView = T; 

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