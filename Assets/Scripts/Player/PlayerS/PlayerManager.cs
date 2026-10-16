using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header ("Player Stats")]
    [SerializeField] private PlayerStats Stats; 

    public float _Arkhalis_Current_PlayerHP;

    [Header ("Enable Mecanics")]
    public bool WallSlideIsActive = false; 
    public bool WalljumpIsActive = false; 

    public bool MultijumpIsActive = false; 

    public bool DashIsActive = false; 

    [Header ("Player Can Move")]
    
    public bool PlayerCanMove = true; 
    ///<Regla de HP>
    ///  el daño solo puede ser 1 o 0.5, no mas que esto
    /// <end> 

    void Awake()
    {
        _Arkhalis_Current_PlayerHP = Stats._Arkhalis_HP;
    }

    public void Restart_HP()
    {
        _Arkhalis_Current_PlayerHP = Stats._Arkhalis_HP;
    }
}
