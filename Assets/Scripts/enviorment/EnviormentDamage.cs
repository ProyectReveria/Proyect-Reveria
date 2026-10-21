using UnityEngine;
using System.Collections;
using RMC.Player_Interaction.ThePlayerGeneralManager;

public class EnviormentDamage : MonoBehaviour
{
    #region  Variables
    [Header ("Object References")]
    [SerializeField] private Movement_Manager Mmanager; 
    [SerializeField] private PlayerStats PlayerStats;
    [SerializeField] private PlayerManager Pmanager; 
    [SerializeField] private GameManager Gmanager; 
    public GameObject player; 
    #endregion
    void OnCollisionEnter2D(Collision2D hit)
    {
        

        if (hit.gameObject.CompareTag("Player"))
        {   

            StartCoroutine(Respawn_onStableGround());

            if (Pmanager.Arkhalis_HPStatus <= 0)
            {
                Movement_Manager._LastStableground = RMC_ThePlayerGeneralManager.Game_Data_FromSave.SpawnPoint;
                StartCoroutine(Respawn_onStableGround());
                Pmanager.Restart_HP(); 
                player.transform.position = RMC_ThePlayerGeneralManager.Game_Data_FromSave.SpawnPoint; 
            }
            
        }
    }

    IEnumerator Respawn_onStableGround()
    {
        Mmanager.KillMovement();
        if (Gmanager._CanResiveDamage)
        {
            Pmanager.Arkhalis_HPStatus -= PlayerStats.LowDamageEnviorment; 
            Gmanager._CanResiveDamage = false;
        }
        Pmanager.PlayerCanMove = false;
        yield return new WaitForSeconds(PlayerStats.RespawnTime); 
        Mmanager.TeleportTo(Movement_Manager._LastStableground); 
        Pmanager.PlayerCanMove = true;
        Gmanager._CanResiveDamage = true;
        
    }


}