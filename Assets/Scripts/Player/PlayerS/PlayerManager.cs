using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header ("Player Stats")]
    [SerializeField] private PlayerStats Stats; 

    public float Arkhalis_HPStatus;
    public float Ariamis_HPstatus;


    [Header ("Player Can Move")]
    
    public bool PlayerCanMove = true; 
    ///<Regla de HP>
    ///  el daño solo puede ser 1 o 0.5, no mas que esto
    /// <end> 

    void Awake()
    {
        Arkhalis_HPStatus = Stats._Arkhalis_HP;
    }

    public void Restart_HP()
    {
        Arkhalis_HPStatus = Stats._Arkhalis_HP;
    }
}
