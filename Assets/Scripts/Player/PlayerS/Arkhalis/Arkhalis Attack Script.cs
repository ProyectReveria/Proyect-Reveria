using UnityEngine;
using C_Reveria.TagsLibrary; 

public class AttackScript : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Arkhalis_Movement_Manager Mmanager; 
    [SerializeField] private PlayerStats Pstats; 
    [SerializeField] private Collider2D PlayerHitbox; 
    [SerializeField] private Collider2D AttackHB; 
    public Input_Manager Imanager; 
    private RaycastHit2D AttackCast; 

    //internal references
    private bool _HitEnemy;
    private Component Enemy_stats;
    [Range(0.25f,1f), SerializeField]private float Attack_Raysice = 0.25f;
    private         Vector2 Direction;

    void Update()
    {
        if (Input_Manager.AttackWassPressThisFrame)
        {
            _Attackenemy();
        }
    }



    void _Attackenemy()
    {

        Vector2 Attack_Ray = new Vector2(AttackHB.bounds.center.x,PlayerHitbox.bounds.center.y);
        Vector2 RangeAttack_Ray_Higth = new Vector2 (AttackHB.bounds.size.x,Attack_Raysice);
        AttackCast = Physics2D.BoxCast(Attack_Ray,RangeAttack_Ray_Higth,0f, Direction,Attack_Raysice);

        if (!Mmanager._IsFacingR)
        {
            Direction = Vector2.right;
        } else
        {
            Direction = Vector2.left;
        }
        foreach (string enemy in System_Tags.Enemy_Array ){
            Debug.Log(enemy);
            if (!AttackCast.transform.CompareTag(enemy))
            {
            continue;
            }

            foreach (string Script in System_Tags.scripts)
            {    
                if (AttackCast.transform.gameObject.CompareTag(enemy))
                {
                    Enemy_stats = AttackCast.transform.GetComponent(Script); 
                    if (Enemy_stats != null)
                    {
                        break;
                    }
                }  
            }

            break;
        }

        if (Enemy_stats == null)
        {
            return;
        } else
        {
           Reveria_Contract_Broker Enemy_Convencion = Enemy_stats as Reveria_Contract_Broker; 
           if (Enemy_Convencion == null)
            {
                return;
            }else
            {
                Debug.Log ("i hit enemy");
                Enemy_Convencion._EnemyReciveDamage_Contract(Pstats._Arkhalis_Melee_Damage);
            }
        }
    }
}
