using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
//Pedirle a Cloude deje un mensaje de en que partes hay que conectar las animaciones

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
    [SerializeField] private PlayerManager Pmanager; 
    

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
    private ContactPoint2D[] _floorContacts = new ContactPoint2D[8];

    //Jump Variables
    

    public float VerticalVelocity {get; private set;}
    private bool _isJumping; 
    private bool _IsFastFalling; 
    private bool _IsFalling; 
    private float _FastFallTime; 
    private float _FastFallRelSpeed; 
    private int _NumberofJumpUse;

    

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
    private bool _iswalljumpFalling; 

    private float _walljumpPostBufferT;
    private int _walljumpDir;

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

    //ledge grab: esta colgado del borde, esquina del borde agarrado y tiempo para volver a agarrar
    private bool _isGrabbingLedge;
    private Vector2 _ledgeCorner;
    private float _LedgeCooldownT;

    #endregion

    #region  Unity Update Interfas
        private void Awake()
    {
        _IsFacingR = true; 
        RickBody = GetComponent<Rigidbody2D>();
    }

  

    private void Update()
    {
        Debug.Log(Input_Manager.movement);

        if (Pmanager.PlayerCanMove )
        {
            Jump_ConterTime();
            JumpCheck();

            LandCheck();
            IsTouchingWall();

        }

        if (Pmanager.WalljumpIsActive)
        {
            WallJumpCheck();
        }

        if (Pmanager.WallSlideIsActive)
        {
            WallSlide_Check();
        }

        if (Pmanager.DashIsActive)
        {
            Dash_Check();
        }

        //revisa si se puede agarrar, subir o soltar un borde
        if (Pmanager.PlayerCanMove)
        {
            LedgeGrab_Check();
        }


                   
    }

    private void FixedUpdate()
    {
        

        Isgrounded_CollisionCheck();
        Bumbhead();
        if (!Pmanager.PlayerCanMove) return;
        Jump(); 
        Fall();
        WallSlide();
        WallJump();
        Dash(); 
        if (Pmanager.PlayerCanMove)
        {
            if (_isGit)
            {
                Move(Mstas.GAcceleration,Mstas.GDesaseleration,Input_Manager.movement); 
            } 
            else
            {
                //walljump
                if (_useWallJumpMstats)
                {
                    if (Input_Manager.movement.x * _walljumpDir > 0f)
                    {
                        Move(Mstas.WallJumpMAcceleration,Mstas.WallJumpMDAcceleration, Input_Manager.movement);
                    }
                
                }
                //Ariborne
                else
                {
                    Move(Mstas.Aacceleration, Mstas.ADeceleration,Input_Manager.movement); 
                }
            }

            ApplyVelocity(); 
        }
    }

    #endregion

    #region  Movement
 
    private void ApplyVelocity()
    {
        //mantiene al jugador quieto mientras esta colgado de un borde
        if (_isGrabbingLedge)
        {
            _Horizontalvelocity = 0f;
            VerticalVelocity = 0f;
            RickBody.linearVelocity = Vector2.zero;
            return;
        }

        if (!_isdashing)
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -Mstas.MaxFallSpeed, Mstas.FallSpeed);
        } else
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, Mstas.Velocity_Min_Dashing,Mstas.Velocity_Max_Dashing);
        }

        //en el suelo la velocidad sigue la superficie, asi pendientes y curvas se recorren a velocidad constante
        bool followGround = _isGit && !_iswallJumping && ((!_isdashing && !_isJumping && VerticalVelocity <= 0f) || (_isdashing && !_isAirDashing && _dashDirection.y == 0f));

        if (followGround)
        {
            Vector2 groundNormal = GetGroundNormal();
            Vector2 groundTangent = new Vector2(groundNormal.y, -groundNormal.x);
            RickBody.linearVelocity = groundTangent * _Horizontalvelocity + groundNormal * VerticalVelocity;
            return;
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

            ///<Aqui va una animacion de 'Correr' (si _isGit es true) o de 'Moverse en el aire' (si _isGit es false)
 
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
            ///<Aqui va una animacion de 'Idle' (quieto, sin input de movimiento)

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
        ///<Aqui va una animacion de 'Giro' (cambio de direccion, opcional)

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
        } else if ( _isGrabbingLedge || _isWallSliding || (_istouchingWall && !_isGit))
        {
            return; 
        }
        //Check what happen in the moment we use Space
        if (Input_Manager.JumpWpress)
        {
            _JumpBufferT = Mstas.jumpbuffTime;
            _JumpRduringBuffer = true; 
        }
 
        //Check in buttom was pressed this frame
        if (Input_Manager.JumpwRelased)
        {
            if (_JumpBufferT > 0f)
            {
                _JumpRduringBuffer = false; 
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
            ///<Aqui va una animacion de 'Salto' (primer salto desde el suelo o coyote time)

            InitialJump(1);
            
 
            //if (_JumpRduringBuffer) {
                //_IsFastFalling = true;
                //_FastFallRelSpeed = VerticalVelocity; 
           // }
        }
 
        //MultipleJump
 
        else if (_JumpBufferT > 0f && (_isJumping || _IsFalling || _iswallJumping || _iswalljumpFalling || _isAirDashing || _isdashFastFalling ) && !_istouchingWall && _NumberofJumpUse < Mstas.NumberOfJumpsAllowed)
        {
            ///<Aqui va una animacion de 'Doble Salto' (salto en el aire)

            InitialJump(1);
            _isdashFastFalling = false;
        }
 
    }
 
    private void InitialJump(int numberofJumpUsed)
    {
        _isJumping = true;
        _IsFalling = false;
        _IsFastFalling = false;
        _FastFallTime = 0f;
        _IsPastApexThreshold = false;
 
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
            if (_IbumHead && VerticalVelocity > 0f)
            {
                VerticalVelocity = -0.1f;
                _IsPastApexThreshold = false;
                _IsFastFalling = true;
                _FastFallTime = Mstas.TimeForUpswardC;
            }
 
            //Gravity on Ascending
            if (VerticalVelocity >= 0f)
            {
                ///<Aqui va una animacion de 'Subiendo' (el salto va hacia arriba)

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
                            ///<Aqui va una animacion de 'Punto Maximo del Salto' (apex, flotando)

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
                    ///<Aqui va una animacion de 'Caida' (cae despues de un salto)

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
        if (!_isGit && !_isJumping && !_isWallSliding && !_iswallJumping && !_isdashing && !_isdashFastFalling && !_isGrabbingLedge)
        {
            if (!_IsFalling)
            {
                ///<Aqui va una animacion de 'Caida' (cae sin haber saltado, por ejemplo al salir de una plataforma)

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
            ///<Aqui va una animacion de 'Aterrizaje' (toca el suelo despues de saltar, caer, wall jump o dash)

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
        if (!_isGit && !_isJumping && !_isWallSliding && !_iswallJumping && !_isdashing && !_isGrabbingLedge && _isdashFastFalling)
        {
            if (!_IsFalling)
            {
                ///<Aqui va una animacion de 'Caida despues del Dash' (cae al terminar un dash en el aire)

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

                ///<Aqui va una animacion de 'Agarre en Muro' (empieza a deslizarse por la pared)

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
            ///<Aqui va el fin de la animacion de 'Agarre en Muro' (deja de deslizarse, pasa a caida o idle)

            ++_NumberofJumpUse;
        }
        _isWallSliding = false;
    }
 
    void WallSlide()
    {
        if (_isWallSliding)
        {
            ///<Aqui va una animacion de 'Deslizarse en Muro' (loop mientras baja por la pared)

            VerticalVelocity = Mathf.Lerp(VerticalVelocity, -Mstas.wallSLideSpeed, Mstas.WallSlideDseleration * Time.deltaTime);
        }
    }
 
    private void WallJumpCheck()
    {
        if (!_isGit && (_istouchingWall || _isWallSliding))
        {
            _walljumpPostBufferT = Mstas.WallJumpPostBufferTime;
        }
 
        if (Input_Manager.JumpWpress && !_isGit && !_iswallJumping && !_isGrabbingLedge)
        {
            if (_iswallSlidingFalling && _walljumpPostBufferT >= 0f)
            {
                InitiateWallJump();
            }
            else if (_isWallSliding || _istouchingWall)
            {
                InitiateWallJump();
            }
        }
    }
 
    private void InitiateWallJump()
    {
        ///<Aqui va una animacion de 'Salto de Pared' (Wall Jump)

        if (!_iswallJumping)
        {
            _iswallJumping = true;
            _useWallJumpMstats = true;
        }
 
        StopWallSlidng();
        Restart_JumpValues();
 
        _walljumpT = 0f;
        VerticalVelocity = Mstas.InitialJumpVelocityWallJump;
 
        int dirMultiplier = 0;
        Vector2 hitPoint = _lastwallhit.collider.ClosestPoint(_BodyCoil.bounds.center);
 
        if (hitPoint.x > transform.position.x)
        {
            dirMultiplier = -1;
        }
        else
        {
            dirMultiplier = 1;
        }

        _walljumpDir = dirMultiplier;
 
        _Horizontalvelocity = Mathf.Abs(Mstas.walljumpD.x) * dirMultiplier;
    }
 
    private void WallJump()
    {
        if (_iswallJumping)
        {
            _walljumpT += Time.fixedDeltaTime;
            if (_walljumpT >= Mstas.TimeTillJumpApex)
            {
                _useWallJumpMstats = false;
            }
 
            if (_IbumHead && VerticalVelocity > 0f)
            {
                VerticalVelocity = -0.1f;
                _IsFastFalling = true;
                _FastFallTime = Mstas.TimeForUpswardC;
                _useWallJumpMstats = false;
            }
 
            if (VerticalVelocity >= 0f)
            {
                VerticalVelocity += Mstas.GravityonWallJump * Mstas.WallJumpGravityMult * Time.fixedDeltaTime;
                _ApexPoint = Mathf.InverseLerp(Mstas.InitialJumpVelocityWallJump, 0f, VerticalVelocity);
 
                if (_ApexPoint > Mstas.ApexTrheshold)
                {
                    if (!_IsPastApexThreshold)
                    {
                        _IsPastApexThreshold = true;
                        _TimePastApexThreshold = 0f;
                    }
 
                    _TimePastApexThreshold += Time.fixedDeltaTime;
 
                    if (_TimePastApexThreshold < Mstas.ApexHangTime)
                    {
                        VerticalVelocity = 0f;
                    }
                    else
                    {
                        VerticalVelocity = -0.1f;
                    }
                }
            }
            else if (!_IsFastFalling)
            {
                VerticalVelocity += Mstas.GravityonWallJump * Mstas.WallJumpGravityOnReleaseMult * Time.fixedDeltaTime;
            }
 
            if (VerticalVelocity < 0f)
            {
                ///<Aqui va una animacion de 'Caida despues del Salto de Pared'

                _iswalljumpFalling = true;
            }
        }
    }
 
    #endregion

    #region  Dash
    void Dash_Check()
    {
        if (Input_Manager.DashWpress)
        {
            //Dash
            if (_isGit && _dashOnGroundTimer < 0f && !_isdashing)
            {
                ///<Aqui va una animacion de 'Dash' (en el suelo)

                Overload_InitiateJump(); 
            }
            //AirDash

            else if (!_isGit && !_isdashing && _numberOfDashUsed < Mstas.numberofDash)
            {
                _isAirDashing = true;

                ///<Aqui va una animacion de 'Dash en el Aire' (Air Dash)

                Overload_InitiateJump();

                //a veces el jugador pierde un salto al hacer un dash desde un muro, aqui esta el arreglo:
                if (_walljumpPostBufferT > 0f)
                {
                    --_NumberofJumpUse; 

                    if (_NumberofJumpUse < 0)
                    {
                        _NumberofJumpUse = 0; 
                    }
                }
            }
        }
    }

    void Overload_InitiateJump()
    {
        _dashDirection = Input_Manager.movement; 

        Vector2 Closedirection = Vector2.zero; 
        float MinDistance = float.MaxValue; 

        if (_dashDirection.sqrMagnitude > 0.01f)
        {
            for (int i = 0; i < Mstas.DashDirection.Length; i++)
            {
                if (Mstas.DashDirection[i] == Vector2.zero)
                {
                    continue;
                }

                //skip if we hit bang

                if (_dashDirection == Mstas.DashDirection[i])
                {
                    Closedirection = _dashDirection; 
                    break; 
                }

                float Distance = Vector2.Distance(_dashDirection, Mstas.DashDirection[i]);

                //Check if this diagonal apply Absolute value 

                bool isdiagonal = (Mathf.Abs(Mstas.DashDirection[i].x) == 1 && Mathf.Abs(Mstas.DashDirection[i].y) == 1);

                if (isdiagonal)
                {
                    Distance -= Mstas.DeashDiagonalBias; 
                }

                if (Distance < MinDistance)
                {
                    MinDistance = Distance;
                    Closedirection = Mstas.DashDirection[i];
                }
            }
        }

        if (Closedirection == Vector2.zero)
        {
            if (_IsFacingR)
            {
                Closedirection = Vector2.right;
            } else
            {
                Closedirection = Vector2.left;
            }
        }

        _dashDirection = Closedirection.normalized;
        _numberOfDashUsed++; 
        _isdashing = true;
        _dashTimer = 0f;
        _dashOnGroundTimer = Mstas.timeBtwDash;

        Restart_JumpValues();
        Restart_WallJumpValue();
        StopWallSlidng();

        //el dash suelta el borde si estaba colgado
        if (_isGrabbingLedge)
        {
            ReleaseLedge();
        }
    }

    void Dash()
    {
        if (_isdashing)
        {
            ///<Aqui va una animacion de 'Dash' (loop mientras dura el dash, usa _dashDirection para la direccion)

            _dashTimer += Time.fixedDeltaTime;
            if (_dashTimer >= Mstas.DashTime)
            {
                if (_isGit)
                {
                    ResetDashV();
                }

                _Horizontalvelocity = Mathf.Clamp(_Horizontalvelocity, -Mstas.MaxRunSpeed, Mstas.MaxRunSpeed);

                ///<Aqui va el fin de la animacion de 'Dash' (termino el dash, pasa a caida, idle o correr)

                _isAirDashing = false;
                _isdashing = false;

                if (!_isJumping && !_iswallJumping)
                {
                    _DashFastFallTime = 0f; 
                    _dashFastFallRelaeseSpeed = VerticalVelocity;

                    if (!_isGit)
                    {
                        _isdashFastFalling = true;
                    }
                }

                return;
            }

            _Horizontalvelocity = Mstas.dashspeed * _dashDirection.x;

            if (_dashDirection.y != 0f || _isAirDashing)
            {
                VerticalVelocity = Mstas.dashspeed * _dashDirection.y;
            }
        }
        //Handle DAsh Cut Time
        else if (_isdashFastFalling)
        {
            if (VerticalVelocity > 0f)
            {
                if (_DashFastFallTime < Mstas.DashTimeForUpwardC)
                {
                    VerticalVelocity = Mathf.Lerp(_dashFastFallRelaeseSpeed, 0f, (_DashFastFallTime / Mstas.DashTimeForUpwardC));
                }
                else if (_DashFastFallTime >= Mstas.DashTimeForUpwardC)
                {
                    VerticalVelocity += Mstas.Gravity * Mstas.DashGravityOnRealaseMult * Time.fixedDeltaTime;
                }

                _DashFastFallTime += Time.fixedDeltaTime;
            }

            else
            {
                VerticalVelocity += Mstas.Gravity * Mstas.DashGravityOnRealaseMult * Time.fixedDeltaTime;
            }
        }
        
    }
    #endregion

    #region Ledge Grab
    //Detecta un borde al caer junto a una pared, lo agarra, y si ya esta colgado permite subir (salto) o soltar (abajo)
    void LedgeGrab_Check()
    {
        if (!Mstas.LedgeGrabEnabled) return;

        if (_isGrabbingLedge)
        {
            ///<Aqui va una animacion de 'Colgado del Borde' (loop mientras esta agarrado del borde)

            if (Input_Manager.JumpWpress)
            {
                LedgeClimb();
            }
            else if (Input_Manager.movement.y <= -Mstas.MoveThreshold)
            {
                ReleaseLedge();
            }
            return;
        }

        if (_isGit || _isdashing || !_istouchingWall || VerticalVelocity >= 0f || _LedgeCooldownT > 0f) return;

        float dir = 1f;
        float frontX = _BodyCoil.bounds.max.x;
        if (!_IsFacingR)
        {
            dir = -1f;
            frontX = _BodyCoil.bounds.min.x;
        }
        float topY = _BodyCoil.bounds.max.y;

        //rayo alto sin pared y rayo bajo con pared = hay un borde
        Vector2 upperOrigin = new Vector2(frontX, topY + Mstas.LedgeCheckHeight);
        Vector2 lowerOrigin = new Vector2(frontX, topY - Mstas.LedgeCheckHeight);

        RaycastHit2D upperHit = Physics2D.Raycast(upperOrigin, Vector2.right * dir, Mstas.LedgeDetectRayLength, Mstas.Glayer);
        RaycastHit2D lowerHit = Physics2D.Raycast(lowerOrigin, Vector2.right * dir, Mstas.LedgeDetectRayLength, Mstas.Glayer);

        if (upperHit.collider != null || lowerHit.collider == null) return;

        //busca la superficie de arriba del borde para saber la altura de la esquina
        Vector2 downOrigin = new Vector2(lowerHit.point.x + dir * 0.02f, topY + Mstas.LedgeCheckHeight);
        RaycastHit2D topHit = Physics2D.Raycast(downOrigin, Vector2.down, Mstas.LedgeCheckHeight * 2f, Mstas.Glayer);

        if (topHit.collider == null || !IsWalkableNormal(topHit.normal)) return;

        _ledgeCorner = new Vector2(lowerHit.point.x, topHit.point.y);

        ///<Aqui va una animacion de 'Agarrar Borde' (el momento en que se engancha del borde)

        _isGrabbingLedge = true;

        Restart_JumpValues();
        Restart_WallJumpValue();
        StopWallSlidng();
        ResetDashV();
        ResetAmountDash();

        _NumberofJumpUse = 0;
        _JumpBufferT = 0f;
        _Horizontalvelocity = 0f;
        VerticalVelocity = 0f;

        //acomoda al jugador colgando justo debajo de la esquina
        Vector2 snap = new Vector2((_ledgeCorner.x - dir * Mstas.LedgeHangDistance) - frontX, (_ledgeCorner.y - Mstas.LedgeHangOffset) - topY);
        RickBody.position += snap;
        Physics2D.SyncTransforms();
    }

    //Sube al jugador encima del borde, no sube si no hay espacio libre arriba
    private void LedgeClimb()
    {
        float dir = _IsFacingR ? 1f : -1f;

        Vector2 delta = new Vector2(
            (_ledgeCorner.x + dir * (_BodyCoil.bounds.extents.x + Mstas.LedgeClimbForward)) - _BodyCoil.bounds.center.x,
            (_ledgeCorner.y + Mstas.LedgeClimbUp) - _FeetCoil.bounds.min.y);

        Vector2 targetCenter = (Vector2)_BodyCoil.bounds.center + delta;

        if (Physics2D.OverlapBox(targetCenter, _BodyCoil.bounds.size * 0.95f, 0f, Mstas.Glayer) != null)
        {
            return;
        }

        ///<Aqui va una animacion de 'Subir el Borde' (trepar encima del borde)

        TeleportTo(RickBody.position + delta);
    }

    //Suelta el borde y activa el tiempo para que no lo vuelva a agarrar de inmediato
    private void ReleaseLedge()
    {
        ///<Aqui va el fin de la animacion de 'Colgado del Borde' (se suelta del borde, pasa a caida)

        _isGrabbingLedge = false;
        _LedgeCooldownT = Mstas.LedgeRegrabTime;
    }
    #endregion
 
    #region Timer
    private void Jump_ConterTime()
    {
        _JumpBufferT -= Time.deltaTime; 
        _walljumpPostBufferT -= Time.deltaTime;
        _LedgeCooldownT -= Time.deltaTime;
 
        if (!_isGit)
        {
            _CyoteTimer -= Time.deltaTime;
        } else { _CyoteTimer = Mstas.jumpcyote; }

        if (_isGit)
        {
            _dashOnGroundTimer -= Time.deltaTime;
        }
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
            _isGit = IsWalkableNormal(GetGroundNormal()); 
            if (_isGit && !_Ghit.collider.gameObject.CompareTag("Non_StableGround"))
            {
                _LastStableground = Vector2Int.RoundToInt(Player.transform.position); 
 
            }
        }else if ( _Ghit == false)
        {
            _isGit = TouchingFloorContact(); 
        }
 
 
    }

    //Devuelve la normal de la superficie bajo los pies, se usa para seguir pendientes y curvas
    private Vector2 GetGroundNormal()
    {
        Vector2 origin = new Vector2(_FeetCoil.bounds.center.x, _FeetCoil.bounds.min.y + Mstas.GDRayLength);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, Mstas.GDRayLength * 2f, Mstas.Glayer);

        if (hit.collider != null && hit.normal.sqrMagnitude > 0.01f)
        {
            return hit.normal;
        }

        return Vector2.up;
    }

    //Dice si una superficie es suficientemente plana para caminar sobre ella (MaxSlopeAngle)
    private bool IsWalkableNormal(Vector2 normal)
    {
        return Vector2.Angle(normal, Vector2.up) <= Mstas.MaxSlopeAngle;
    }

    private bool TouchingFloorContact()
    {
        if (VerticalVelocity > 0f) return false;

        int n = RickBody.GetContacts(_floorContacts);
        for (int i = 0; i < n; i++)
        {
            if (_floorContacts[i].normal.y > 0.7f)
            {
                return true;
            }
        }
        return false;
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
 
        float adjustHigth = _BodyCoil.bounds.size.y * Mstas.WallDetectRayHeigthMult; 
 
        Vector2 BcastOrigin = new Vector2(OriginPEnd, _BodyCoil.bounds.center.y);
        Vector2 BcastSize = new Vector2(0.05f, adjustHigth); 
 
        _wallhit = Physics2D.BoxCast(BcastOrigin, BcastSize, 0f, transform.right, Mstas.WallDetectionRAyLength, Mstas.Glayer);
 
        //una superficie caminable (pendiente suave) no cuenta como pared
        if (_wallhit.collider != null && !IsWalkableNormal(_wallhit.normal))
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
        _IsPastApexThreshold = false; 
    }
 
    private void Restart_WallJumpValue()
    {
        _iswallSlidingFalling = false; 
        _useWallJumpMstats = false;
        _iswallJumping = false; 
        _iswalljumpFalling = false; 
        _IsPastApexThreshold = false;
        
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

    public void KillMovement()
    {
        ///<Aqui va una animacion de 'Daño' (se llama al recibir daño, cancela cualquier otra animacion de movimiento)

        _Horizontalvelocity = 0f;
        VerticalVelocity = 0f;
        RickBody.linearVelocity = Vector2.zero;

        Restart_JumpValues();
        Restart_WallJumpValue();
        StopWallSlidng();
        ResetDashV();

        _JumpBufferT = 0f;
        _walljumpPostBufferT = 0f;
        _CyoteTimer = 0f;

        _isdashing = false;
        _isAirDashing = false;
        _dashTimer = 0f;
        _isGrabbingLedge = false;
        
    }

    public void TeleportTo(Vector2 position)
{
    KillMovement();
    RickBody.position = position;
    transform.position = position;
    Physics2D.SyncTransforms();
}
    #endregion
}