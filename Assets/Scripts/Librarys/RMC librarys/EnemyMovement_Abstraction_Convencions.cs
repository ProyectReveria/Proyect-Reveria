using UnityEngine; 
using System;
using C_Reveria.TagsLibrary;
using System.Collections;
//Credits To: Uriel Gomez madriz And Proyect Reveria Github Tools
public abstract class Enemy_Movement_BaseBahivior : MonoBehaviour, Reveria_Contract_Broker
{
 #region Variables

    [Header ("Referenicas")]
    [SerializeField] protected RMC_RenderLibrary Render_Libray;
    [SerializeField] protected GameObject arkhalis_player;
    [SerializeField] protected GameObject ariamis_player;

    //Si alguna estadistica no esta aca probablemente esta en enemy Stats
    [SerializeField] protected EnemyStats Enemy_Stats; 
    [SerializeField] protected Collider2D bodyCollider;
    [SerializeField] protected Collider2D FeetCollider; 
    [SerializeField] protected GameObject Enemy; 
    [SerializeField] protected Rigidbody2D Enemy_Rickbody; 
    [Header ("Posicion")]
    [Range(0f, 1f)] public float Enemy_MoveThreshold = 0.25f; 
    public bool _RigthView;
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
        Render_Libray.AddToList(Enemy);

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

        if (HPStatus_Enemy >= 0)
        {
            Render_Libray.RemmoveToList(Enemy);
            Destroy(this.gameObject);
        }
    }

    
    #endregion

}

#region Attack script
public abstract class Enemy_AtackDetection : MonoBehaviour 
{
    [Header ("Referencias")]

    [SerializeField] protected PlayerManager PManager;
    [SerializeField] protected Collider2D EnemyAttackRange; 
    [SerializeField] protected Collider2D bodyCollider;
    [SerializeField] protected EnemyStats Enemy_Stats; 
    [SerializeField] protected GameObject enemy;
    [SerializeField] protected GameObject Proyectile; 
    [SerializeField] protected LayerMask player;
    [SerializeField] protected LayerMask Ground;
    [SerializeField] protected LayerMask ArkhalisLayer;
    [SerializeField] protected LayerMask AriamisLayer; 
    [Range (1f,5f)] protected float EnemyMaxDistanceDetection;
    [Range(1f,10f), SerializeField] protected float Proyectilspeed; 
    protected RaycastHit2D[] AttackCast; 
    protected RaycastHit2D HitCast_onAttackCast;
    protected float radius;
    protected Vector2 playerposition; 
    //referencias
    protected bool _IsAlrearyAttack; 
    protected float _TimeBetweenProyectiles; 

    protected Rigidbody2D proyectile;
    #region unity update
    protected virtual void Awake()
    {
        UpdateValue();
    }

    protected virtual void FixedUpdate()
    {
        virtual_DetectPlayerOnRay();
    }

    #endregion
    #region  attack and update
    void UpdateValue()
    {
        radius = Enemy_Stats.T_U_P_C.EnemyDetectionRange;
        _TimeBetweenProyectiles = Enemy_Stats.T_U_P_C.TimeBetweenAttacks;
    }

    protected virtual void virtual_DetectPlayerOnRay()
    {   

        //ciclecast :: only one detection
        //Circlecastall :: all targets
        AttackCast = Physics2D.CircleCastAll(enemy.transform.position, radius, enemy.transform.right,5, player);
        System.Array.Sort(AttackCast, (start,finish) => start.distance.CompareTo(finish.distance));
        foreach (RaycastHit2D hit in AttackCast)
        {
            
            if (hit.collider.gameObject.layer == Ground.value)
            {
                break;
            }

            if (hit.collider.CompareTag(System_Tags.PlayerTag) && (hit.transform.gameObject.layer == ArkhalisLayer.value && PManager._IsArkhalisActive ^ hit.transform.gameObject.layer == AriamisLayer.value && PManager._IsAriamisActive)  )
            {
                playerposition = hit.transform.position;
                HitCast_onAttackCast = hit;
                break;
            }

        }

        if ((playerposition != null || playerposition == Vector2.zero) && Enemy_Stats.T_U_P_C.Enemy_Type == reveria_Stadistics_Structs.StructureLibrary.Enemy_Type.Range_Enemy && !_IsAlrearyAttack)
        {
            ProyectileAtack();
        } else 
        {
            return;
        }

    }
    #endregion

    protected virtual void ProyectileAtack()
    {
            Vector2 Direction = ((Vector2)playerposition - proyectile.position).normalized;
            if (Proyectile.GetComponent<Rigidbody2D>() == null)
            {
                proyectile = Proyectile.AddComponent<Rigidbody2D>();
                proyectile.linearVelocity = Direction;
                StartCoroutine(TimeBetweenproyectiles(_TimeBetweenProyectiles));

            } else
            {
                proyectile = proyectile.GetComponent<Rigidbody2D>();
            
                proyectile.linearVelocity = Direction;
            }
    }

    protected virtual IEnumerator TimeBetweenproyectiles(float TimeBeetweenShots)
    {
        _IsAlrearyAttack = true;
        yield return new WaitForSeconds(TimeBeetweenShots);
        _IsAlrearyAttack = false;
        
    }
}
#endregion