using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour , Reveria_Contract_Broker
{
    [Header ("RenderObjectList")]
    public List<GameObject> Render_Objects = new List<GameObject>();
    [Header ("Interactions")]
    public  bool _CanInteract = false; 
    public bool _CanResiveDamage = true; 
    public bool _Iframes = false; 
    [Header ("Player Stats")]
    [SerializeField] private PlayerStats Stats; 

    public float Arkhalis_HPStatus;
    public float Ariamis_HPstatus;

    [Header ("Is Arkhalisor Ariamis Active")]
    public bool _IsAriamisActive;
    public bool _IsArkhalisActive; 

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
