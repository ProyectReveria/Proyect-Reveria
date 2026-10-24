using UnityEngine;
using System.Collections;
using RMC.Player_Interaction.ThePlayerGeneralManager;
using NUnit.Framework;

public class EnviormentDamage : MonoBehaviour
{
    #region  Variables
    [Header ("Object References")]
    [SerializeField] private Arkhalis_Movement_Manager BloodManager; 
    [SerializeField] private Ariamis_Movement_Manager Magicmanager;
    [SerializeField] private PlayerStats PlayerStats;
    [SerializeField] private PlayerManager Pmanager; 

    public GameObject player; 
    #endregion
    void OnCollisionEnter2D(Collision2D hit)
    {
        

        if (hit.gameObject.CompareTag("Player"))
        {   

            StartCoroutine(Respawn_onStableGround());

            if (Pmanager.Arkhalis_HPStatus <= 0)
            {
                BloodManager._LastStableground = RMC_ThePlayerGeneralManager.Game_Data_FromSave.SpawnPoint;
                StartCoroutine(Respawn_onStableGround());
                Pmanager.Restart_HP(); 
                player.transform.position = RMC_ThePlayerGeneralManager.Game_Data_FromSave.SpawnPoint; 
            }
            
        }
    }

    IEnumerator Respawn_onStableGround()
    {
        if (Pmanager._IsArkhalisActive)
        {
            BloodManager.KillMovement();
        } else if (Pmanager._IsAriamisActive)
        {
            Magicmanager.KillMovement();
        }
        
        if (Pmanager._CanResiveDamage)
        {
            Pmanager.Arkhalis_HPStatus -= PlayerStats.LowDamageEnviorment; 
            Pmanager._CanResiveDamage = false;
        }
        Pmanager.PlayerCanMove = false;
        
        yield return new WaitForSeconds(PlayerStats.RespawnTime); 

        if (Pmanager._IsArkhalisActive)
        {
            BloodManager.TeleportTo(Magicmanager._LastStableground); 
        } else if (Pmanager._IsAriamisActive)
        {
            Magicmanager.TeleportTo(Magicmanager._LastStableground); 
        }

        Pmanager.PlayerCanMove = true;
        Pmanager._CanResiveDamage = true;
        
    }


}