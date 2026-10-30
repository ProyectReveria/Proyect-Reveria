using System;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "C.Reveria_Player_Input_Manager")]

public class C_ReveriaMovementStats : ScriptableObject 
{
    [Header ("PlayerTags")]

    public LayerMask Arkhalis_LayerMask; 
    public LayerMask Ariamis_LayerMask; 
    
    
    [Header("Walk")]
    //G = ground 
    // A = air
    [Range(0f, 1f)] public float MoveThreshold = 0.25f; 
    [Range(1f, 100f)] public float MaxWalkSpeed = 5f;
    [Range(0.25f , 50f)] public float GAcceleration = 2.5f; 
    [Range(0.25f , 50f)] public float GDesaseleration = 10f; 
    [Range(0.25f , 50f)] public float Aacceleration = 7.5f; 
    [Range(0.25f , 50f)] public float ADeceleration = 7.5f;
    [Range(0.5f, 50f)] public float WallJumpMAcceleration = 5f; 
    [Range(0.5f, 50f)] public float WallJumpMDAcceleration = 5f;  

    [Header ("Run")]
    [Range(1f ,100f)] public float MaxRunSpeed = 15f; 

    [Header("CheckData & Colissions")]
    public LayerMask Glayer; 
    public float GDRayLength = 0.02f; 
    public float HeadDRayLength = 0.02f; 
    [Range(0f , 1f)] public float HeadWidth = 0.75f; 
    public float WallDetectionRAyLength = 0.125f; 
    [Range (0.1f, 2f)] public float WallDetectRayHeigthMult = 0.9f; 

    [Header ("Jump")]
    public float jumpHitgh = 6.5f; 
    [Range (1f, 1.1f)] public float jumpHitghCompenFactor = 1.054f; 
    public float TimeTillJumpApex = 0.35f; 
    [Range (0.1f, 5f)] public float GraviyOnReleaseMult = 2f; 
    public float MaxFallSpeed = 26f; 
    [Range (1,5)] public int NumberOfJumpsAllowed = 1; 

    [Header("Resset Jump Options")]
    public bool RestJumpOnWallS = true; 

    [Header ("Jump Cut off")]
    [Range(0.02f, 0.3f)] public float TimeForUpswardC = 0.27f; 

    [Header ("Jump Apex")]

    [Range (0.5f, 1f)] public float ApexTrheshold = 0.97f; 
    [Range (0.1f, 1f)] public float ApexHangTime = 0.075f; 

    [Header("JumpBuff")]

    [Range (0f,1f)] public float jumpbuffTime = 0.125f; 

    [Header ("Jump Coyote Time")]
    [Range (0f, 1f)] public float jumpcyote = 0.1f;

    [Header ("wall Slide")]

    [Min(0.01f)] public float wallSLideSpeed = 5f; 
    [Range (0.1f, 1f)] public float WallSlideDseleration = 0.5f; 
    [Range(0.25f, 5f)] public float WallJumpGravityOnReleaseMult = 1f; 

    [Header("Wall jump")]
    public Vector2 walljumpD = new Vector2(-20f, 6.5f);
    [Range (0f,1f)] public float WallJumpPostBufferTime = 0.125f; 
    [Range(0.1f, 5f)] public float WallJumpGravityMult = 1f; 

    [Header("Dash")]
    [Range(0f, 1f)] public float DashTime = 0.11f; 
    [Range(1f, 200f)] public float dashspeed = 40f; 
    [Range(0f,1f)] public float timeBtwDash = 0.225f; 
    public bool RessetDasthOnWallSlinde = true; 
    [Range(0, 5)] public int numberofDash = 1; 
    [Range (0f,0.5f)] public float DeashDiagonalBias = 0.4f; 

    [Header("Dash Cancel Time")]
    [Range(0.1f,5f)] public float DashGravityOnRealaseMult = 1; 
    [Range(0.02f,0.3f)] public float DashTimeForUpwardC = 0.27f;

    [Header ("Dash Speed")]

    [Range(-100f, 100f)] public float Velocity_Min_Dashing = -50f;
    [Range(-100f, 100f)] public float Velocity_Max_Dashing = 50f;

    [Header("JumpFaling Speed")]

    [Range(1f, 100f)] public float FallSpeed = 50f; 

    //ArrayofDirectionsFordash

    public readonly Vector2[] DashDirection = new Vector2[]
    {
        //quitar el normalzie ne las diagnonales permite que vaya mas rapido, valorar si es util
        //! el que toque esto es de oaxaca y|o judio
        new Vector2(0,0), //Nothing
        new Vector2(1,0), //rigth
        new Vector2(1,1).normalized, //top rigth
        new Vector2(0,1), //up
        new Vector2(-1,1).normalized, //top left
        new Vector2(-1,0), //left
        new Vector2(-1,-1).normalized, //botom left
        new Vector2(0,-1), //down
        new Vector2(1,-1).normalized,  //bootom- rigth
    };
    
    [Header("Slopes")]
    [Range (10f,80f)]public float MaxSlopeAngle = 50f;

    [Header("Ledge Grab")]
    public bool LedgeGrabEnabled = true;
    [Range (0.1f,1f)] public float LedgeDetectRayLength = 0.3f;
    [Range (0.1f,1f)] public float LedgeCheckHeight = 0.2f;
    [Range (0.1f,1f)] public float LedgeHangOffset = 0.1f;
    [Range (0.01f,1f)] public float LedgeHangDistance = 0.02f;
    [Range (0.1f,1f)] public float LedgeClimbForward = 0.1f;
    [Range (0.01f,1f)] public float LedgeClimbUp = 0.05f;
    [Range (0.1f,1f)] public float LedgeRegrabTime = 0.3f;
    
    //Gravity
    public float Gravity {get; private set;}
    public float InitialJumpVelocity {get; private set;}

    public float AdjustmentJFactor {get; private set; }

    //gravityonwalls
    public float GravityonWallJump {get; private set;}
    public float InitialJumpVelocityWallJump {get; private set;}

    public float AdjustmentJFactorWallJump {get; private set; }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        //! El que toque esto lo mando a siria a recoger agua 
        //jump
        AdjustmentJFactor = jumpHitgh * jumpHitghCompenFactor; 
        Gravity = -(2f * AdjustmentJFactor) / Mathf.Pow(TimeTillJumpApex, 2F);//esta formula debe ser esta y punto
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex; //esta forumla debe estar y punto

        //walljump
        AdjustmentJFactorWallJump = walljumpD.y * jumpHitghCompenFactor; 
        GravityonWallJump = -(2f * AdjustmentJFactorWallJump) / Mathf.Pow(TimeTillJumpApex, 2F);//esta formula debe ser esta y punto
        InitialJumpVelocityWallJump = Mathf.Abs(GravityonWallJump) * TimeTillJumpApex; //esta forumla debe estar y punto


    }


}
