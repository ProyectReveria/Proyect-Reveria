using UnityEngine;

public class MovementManagerfromGround : MonoBehaviour
{
    #region  References
    [Header ("References")]

    public Movement_Stats Mstas;
    [SerializeField] private Collider2D _GroundCollider;
    [SerializeField] private Collider2D _BodyCollider; 

    private Rigidbody2D RickBody; 

    private Vector2 _moveV; 
    private bool _IsFacingR; 

    private RaycastHit2D _Ghit; 
    private RaycastHit2D _HeadHit; 
    
    private bool _isGit; 
    private bool _IbumHead;

    #endregion

    #region  Unity Update Interfas
        private void Awake()
    {
        _IsFacingR = true; 
        RickBody = GetComponent<Rigidbody2D>();
    }
    #endregion

    #region  Movement

    private void Move(float Accel, float Daccel, Vector2 Minput)
    {
        if (Minput != Vector2.zero)
        {
            Vector2 Tvelocity = Vector2.zero; 

            if (Input_Manager.OnRun)
            {
                Tvelocity = new Vector2();
            } else {
                Tvelocity = new Vector2(Minput.x, 0f ) * Mstas.MaxRunSpeed;}
        }
    }


    #endregion



}
