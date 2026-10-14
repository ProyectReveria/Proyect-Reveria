using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

//En orden para este script funcionar ocupas: Un Monobehaivor maneje los inputs junto a un Player Input. En el Player Input el Input managerdebe estar hay y este ocupa un script object con las stats. deberian venir ya en el paquete

// Base On: https://www.youtube.com/watch?v=zHSWG05byEc 

//La gravedad en este codigo es manual al igual que las physicas. para que se tome en cuenta
public class Movement_Manager : MonoBehaviour
{

    public static bool _isDashActive;
    public static bool _isWalljumpActive; 
    // _G = Ground 
    #region  References
    [Header ("References")]

    public Movement_Stats Mstas;
    [SerializeField] private Collider2D _FeetCoil;
    [SerializeField] private Collider2D _BodyCoil; 

    [SerializeField] private GameObject Player; 

    private Rigidbody2D RickBody; 

    public float _Horizontalvelocity {get; private set;}
    private bool _IsFacingR; 
    //GroundHit
    private RaycastHit2D _Ghit; 
    //heat hit
    private RaycastHit2D _HeadHit; 
    //wallhit
    private RaycastHit2D _wallhit; 
    private RaycastHit2D _lastwallhit; 

    private bool _isGit; 
    private bool _IbumHead;

    private bool _IrTouchingWall; 

    //Jump Variables
    

    public float VerticalVelocity {get; private set;}
    private bool _isJumping; 
    private bool _IsFastFalling; 
    private bool _IsFalling; 
    private float _FastFallTime; 
    private float _FastFallRelSpeed; 
    private int _NumberofJumpUse;

    //AddJumps



    //Apex
    private float _ApexPoint; 
    private float _TimePastApexThreshold; 
    private bool _IsPastApexThreshold;

    //JumpBuffer V
    private float _JumpBufferT; 
    private bool _JumpRduringBuffer; 

    //cyotetime
    private float _CyoteTimer; 

    //wall slide
    private bool _isWallSliding; 
    private bool _iswallSlidingFalling; 
    private bool _istouchingWall; 

    //walljump 
    private bool _useWallJumpMstats;
    private bool _iswallJumping; 
    private float _walljumpT; 
    private bool _iswalljumpfastfalling;
    private bool _iswalljumpFalling; 
    private float _walljumpFastFallTime;
    private float _walljumpfastfallrealesspeed;

    private float _walljumpPostBufferT;

    private float _WallJumpApexPoint;
    private float _timePastWallJumpApexThreshold; 
    private bool _ispaswallApexThreshold; 

    //dash vars

    private bool _isdashing;
    private bool _isAirDashing; 
    private float _dashTimer; 
    private float _dashOnGroundTimer; 
    private int _numberOfDashUsed; 
    private Vector2 _dashDirection; 
    private bool _isdashFastFalling; 
    private float _DashFastFallTime; 
    private float _dashFastFallRelaeseSpeed; 

    //NonStableGround

    private bool _IsonStableGround; 
    public static Vector2 _LastStableground {get; set;}

    #endregion

    #region  Unity Update Interfas
        private void Awake()
    {
        _IsFacingR = true; 
        RickBody = GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        Jump_ConterTime();
        JumpCheck();

        IsTouchingWall();
        WallSlide_Check();
                    Debug.Log($"Last Stable Ground {_LastStableground}");
    }

    private void FixedUpdate()
    {
        //Recordar poner el Si esta activo el dash o no, lo mismo apra el walljump
        LandCheck();
        Isgrounded_CollisionCheck();
        Bumbhead();
        Jump(); 
        Fall();
        WallSlide();

        if (_isGit)
        {
            Move(Mstas.GAcceleration,Mstas.GDesaseleration,Input_Manager.movement); 
        } 
        else
        {
            //walljump
            if (_useWallJumpMstats)
            {
                Move(Mstas.WallJumpMAcceleration,Mstas.WallJumpMDAcceleration, Input_Manager.movement);
            
            }
            //Ariborne
            else
            {
                Move(Mstas.Aacceleration, Mstas.ADeceleration,Input_Manager.movement); 
            }
        }

        ApplyVelocity(); 
    }


    #endregion

    #region  Movement

    private void ApplyVelocity()
    {
        if (!_isdashing)
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -Mstas.MaxFallSpeed, Mstas.FallSpeed);
        } else
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, Mstas.Velocity_Min_Dashing,Mstas.Velocity_Max_Dashing);
        }

        RickBody.linearVelocity = new Vector2 (_Horizontalvelocity, VerticalVelocity);
    }

    private void Move(float Accel, float Daccel, Vector2 Minput)
    {
        if (_isdashing) return;

        if (Mathf.Abs(Minput.x) >= Mstas.MoveThreshold)
        {
            turnC(Minput); 

            float Tvelocity = 0; 

            if (Input_Manager.OnRun)
            {
                Tvelocity = Minput.x * Mstas.MaxRunSpeed;
            } else {
                Tvelocity = Minput.x * Mstas.MaxRunSpeed;}

            _Horizontalvelocity = Mathf.Lerp(_Horizontalvelocity, Tvelocity, Accel * Time.fixedDeltaTime); 
            //RickBody.linearVelocity  = new Vector2(_moveV.x, RickBody.linearVelocity.y);  //No es necasrio pero esta aqui si ahce falta
        }
        else
        {
            _Horizontalvelocity = Mathf.Lerp(_Horizontalvelocity, 0f, Daccel * Time.fixedDeltaTime);
            //RickBody.linearVelocity = new Vector2(_moveV.x, RickBody.linearVelocity.y); //No es necasrio pero esta aqui si ahce falta
        }
    }

    private void turnC(Vector2 moveInput)
    {
        if (_IsFacingR && moveInput.x < 0)
        {
            Overload_turn(false); 
        } else if (!_IsFacingR && moveInput.x > 0)
        {
            Overload_turn(true);
        }
    }

    void Overload_turn(bool T)
    {
        if (T)
        {
            _IsFacingR = true; 
            transform.Rotate(0f, 180f,0f); 
        } else         
        {
            _IsFacingR = false; 
            transform.Rotate(0f, -180f,0f); 
        }
    }




    #endregion

    #region Jump 

    private void JumpCheck()
    {
        if (_iswallSlidingFalling && _walljumpPostBufferT >= 0)
        {
            return; 
        } else if ( _isWallSliding || (_istouchingWall && !_isGit))
        {
            return; 
        }
        //Check what happen in the moment we use Space
        if (Input_Manager.JumpWpress)
        {
            _JumpBufferT = Mstas.jumpbuffTime;
            _JumpRduringBuffer = false; 
        }

        //Check in buttom was pressed this frame
        if (Input_Manager.JumpWpress)
        {
            if (_JumpBufferT > 0f)
            {
                _JumpRduringBuffer = true; 
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_IsPastApexThreshold)
                {
                    _IsPastApexThreshold = false;
                    _IsFastFalling = true;
                    _FastFallTime = Mstas.TimeForUpswardC;
                    VerticalVelocity = 0f;
                }else
                {
                    _IsFastFalling = true;
                    _FastFallRelSpeed = VerticalVelocity; 
                }
            }
        }
        // Initial Jump Whit Buff and Cyote T 

        if (_JumpBufferT > 0f && !_isJumping && (_isGit || _CyoteTimer > 0f))
        {
            InitialJump(1);
            

            //if (_JumpRduringBuffer) {
                //_IsFastFalling = true;
                //_FastFallRelSpeed = VerticalVelocity; 
           // }
        }

        //MultipleJump

        else if (_JumpBufferT > 0f && (_IsFalling || _iswallJumping || _iswalljumpFalling || _isAirDashing || _isdashFastFalling ) && !_istouchingWall && _NumberofJumpUse < Mstas.NumberOfJumpsAllowed)
        {
            _IsFalling = false;
            InitialJump(1 );
            
            if (_isdashFastFalling)
            {
                _isdashFastFalling = false;
            }
        }

        //AirJump
        else if (_JumpBufferT > 0f && _IsFalling && !_iswalljumpFalling && _NumberofJumpUse < Mstas.NumberOfJumpsAllowed - 1)
        {
            InitialJump(1);
            _IsFalling = false;
        }

    }

    private void InitialJump(int numberofJumpUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        Restart_WallJumpValue();

        _JumpBufferT = 0f;
        _NumberofJumpUse += numberofJumpUsed;
        VerticalVelocity = Mstas.InitialJumpVelocity; 
    }

    private void  Jump()
    {
        //Apply Gravity While Jumping 

        if (_isJumping)
        {
            //check head 
            if (_IbumHead)
            {
                _IsFastFalling = true;
            }

            //Gravity on Ascending
            if (VerticalVelocity >= 0f)
            {
                //Apex Controls
                VerticalVelocity += Mstas.Gravity * Time.fixedDeltaTime;
                _ApexPoint = Mathf.InverseLerp(Mstas.InitialJumpVelocity, 0f, VerticalVelocity);
                if (_ApexPoint > Mstas.ApexTrheshold)
                {
                    if (!_IsPastApexThreshold)
                    {
                        _IsPastApexThreshold = true;
                        _TimePastApexThreshold = 0f;
                    }

                    if (_IsPastApexThreshold)
                    {
                        _TimePastApexThreshold += Time.fixedDeltaTime;

                        if (_TimePastApexThreshold < Mstas.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        } else
                        {
                            VerticalVelocity = -0.1f;
                        }
                    }
                }
            }
            //Grevity on ascending not pacinthe treshole
            //graviy on descending
            else if (!_IsFastFalling)
            {
                VerticalVelocity += Mstas.Gravity * Mstas.GraviyOnReleaseMult * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0f)
            {
                if (!_IsFalling)
                {
                    _IsFalling = true;
                }
            }
        }

        //Jump cut
        if (_IsFastFalling)
        {
            if(_FastFallTime >= Mstas.TimeForUpswardC)
            {
                VerticalVelocity += Mstas.Gravity * Mstas.GraviyOnReleaseMult * Time.fixedDeltaTime; 
            }
            else if (_FastFallTime < Mstas.TimeForUpswardC) {
                VerticalVelocity = Mathf.Lerp(_FastFallRelSpeed, 0f , (_FastFallTime / Mstas.TimeForUpswardC));
            }

            _FastFallTime += Time.fixedDeltaTime;
        }
        //normal gravity while falling
        if (!_isGit && !_isJumping && !_isWallSliding)
        {
            if (!_IsFalling)
            {
                _IsFalling = true;
            }

            VerticalVelocity += Mstas.Gravity * Time.fixedDeltaTime;
        }

        //clamp fall speed

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -Mstas.MaxFallSpeed, Mstas.FallSpeed);

        //RickBody.linearVelocity = new Vector2 (RickBody.linearVelocity.x, VerticalVelocity);
        
    }

    private void LandCheck()
    {
        if ((_isJumping || _IsFalling || _iswalljumpFalling || _iswallSlidingFalling || _isWallSliding || _isdashFastFalling) && _isGit && VerticalVelocity <= 0f)
        {
            Restart_JumpValues();
            Restart_WallJumpValue();
            StopWallSlidng();
            ResetAmountDash();

            _NumberofJumpUse = 0; 

            VerticalVelocity = Physics2D.gravity.y; 

            if (_isdashFastFalling && _isGit)
            {
                ResetDashV();
                return;
            }

            ResetDashV();
        }
    }

    private void Fall()
    {
        if (!_isGit && !_isJumping && !_isWallSliding && !_iswallJumping && !_isdashing && _isdashFastFalling)
        {
            if (!_IsFalling)
            {
                _IsFalling = true;
            }

            VerticalVelocity += Mstas.Gravity * Time.fixedDeltaTime; 
        }
    }


    #endregion

    #region Wall Slide / Wall Jump

    void WallSlide_Check()
    {
        if (_istouchingWall && !_isGit && !_isdashing)
        {
            if(VerticalVelocity < 0f && !_isWallSliding)
            {
                Restart_JumpValues();
                Restart_WallJumpValue();
                ResetDashV();

                if (Mstas.RessetDasthOnWallSlinde)
                {
                    ResetAmountDash(); 
                }
                
                _iswallSlidingFalling = false;
                _isWallSliding = true;

                if (Mstas.RessetDasthOnWallSlinde)
                {
                    _NumberofJumpUse = 0; 
                }
            }
        }
        else if (_isWallSliding && !_istouchingWall && !_isGit && !_iswallSlidingFalling)
        {
            _iswallSlidingFalling = true; 
            StopWallSlidng();
        }

        else
        {
             StopWallSlidng();
        }
    }

    private void StopWallSlidng()
    {
        if (_isWallSliding) //tambien peude cambiarse para que pase
        {
            ++_NumberofJumpUse;
        }
        _isWallSliding = false;
    }

    void WallSlide()
    {
        if (_isWallSliding)
        {
            VerticalVelocity = Mathf.Lerp(VerticalVelocity, -Mstas.wallSLideSpeed, Mstas.WallSlideDseleration * Time.deltaTime);
        }
    }

    #endregion

    #region Timer
    private void Jump_ConterTime()
    {
        _JumpBufferT -= Time.deltaTime; 

        if (!_isGit)
        {
            _CyoteTimer -= Time.deltaTime;
        } else { _CyoteTimer = Mstas.jumpcyote; }
    }
    #endregion

    #region  Checkcolisions

    

    private void Isgrounded_CollisionCheck()
    {
        //Estos dos coliders lo que hacen es detectar si esta parado en el suelo a diferencia del restodel codigo son solo controles
        Vector2 BoxRay = new Vector2(_FeetCoil.bounds.center.x, _FeetCoil.bounds.min.y);
        Vector2 BoxCast = new Vector2(_FeetCoil.bounds.size.x, Mstas.GDRayLength);

        _Ghit = Physics2D.BoxCast(BoxRay, BoxCast, 0f, Vector2.down, Mstas.GDRayLength, Mstas.Glayer);
        
        if (_Ghit.collider != null)
        {
            _isGit = true; 
            if (!_Ghit.collider.gameObject.CompareTag("Non_StableGround"))
            {
                _LastStableground = Vector2Int.RoundToInt(Player.transform.position); 

            }
        }else if ( _Ghit == false)
        {
            _isGit = false; 
        }


    }
    private void Bumbhead() // akk Check head
    {
        Vector2 headcast = new Vector2(_FeetCoil.bounds.center.x, _BodyCoil.bounds.max.y);
        Vector2 headsice = new Vector2(_FeetCoil.bounds.size.x, Mstas.HeadDRayLength);

        _HeadHit = Physics2D.BoxCast(headcast, headsice, 0f, Vector2.up, Mstas.HeadDRayLength, Mstas.Glayer); 
        if (_HeadHit.collider != null)
        {
            _IbumHead = true; 
        } else
        {
            _IbumHead= false;
        }
    }

    private void IsTouchingWall()
    {
        float OriginPEnd = 0f; 
        if (_IsFacingR)
        {
            OriginPEnd = _BodyCoil.bounds.max.x;
        }
        else {OriginPEnd = _BodyCoil.bounds.min.x;}

        float adjustHigth = _BodyCoil.bounds.size.y * 0.8f; 

        Vector2 BcastOrigin = new Vector2(OriginPEnd, _BodyCoil.bounds.center.y);
        Vector2 BcastSize = new Vector2(0.05f, adjustHigth); 

        _wallhit = Physics2D.BoxCast(BcastOrigin, BcastSize, 0f, transform.right, Mstas.WallDetectionRAyLength, Mstas.Glayer);

        if (_wallhit.collider != null)
        {
            _lastwallhit = _wallhit; 
            _istouchingWall = true; 
        } else {_istouchingWall = false;}
    }


    #endregion

    #region  ResetValues
    private void Restart_JumpValues()
    {
        _isJumping = false;
        _IsFalling = false;
        _IsFastFalling = false;
        _FastFallTime = 0f; 
        _ispaswallApexThreshold = false; 
    }

    private void Restart_WallJumpValue()
    {
        _iswallSlidingFalling = false; 
        _useWallJumpMstats = false;
        _iswallJumping = false; 
        _iswalljumpFalling = false; 
        _ispaswallApexThreshold = false;
        
        _walljumpFastFallTime = 0f; 
        _walljumpT = 0f; 
        
    }

    private void ResetDashV()
    {
        _isdashFastFalling = false; 
        _dashOnGroundTimer = -0.1f; 
    }

    private void ResetAmountDash()
    {
        _numberOfDashUsed = 0; 
    }
    #endregion
}