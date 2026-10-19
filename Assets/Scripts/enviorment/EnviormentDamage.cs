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
    public Vector3 spawn = RMC_ThePlayerGeneralManager.RMC_SaveFile_Information.SpawnPoint_Static;
    #endregion
    void OnCollisionEnter2D(Collision2D hit)
    {
        

        if (hit.gameObject.CompareTag("Player"))
        {   

            StartCoroutine(Respawn_onStableGround());

            if (Pmanager._Arkhalis_Current_PlayerHP <= 0)
            {
                Movement_Manager._LastStableground = spawn;
                StartCoroutine(Respawn_onStableGround());
                Pmanager.Restart_HP(); 
                player.transform.position = spawn; 
            }
            
        }
    }

    IEnumerator Respawn_onStableGround()
    {
        Mmanager.KillMovement();
        if (Gmanager._CanResiveDamage)
        {
            Pmanager._Arkhalis_Current_PlayerHP -= PlayerStats.LowDamageEnviorment; 
            Gmanager._CanResiveDamage = false;
        }
        Pmanager.PlayerCanMove = false;
        yield return new WaitForSeconds(PlayerStats.RespawnTime); 
        Mmanager.TeleportTo(Movement_Manager._LastStableground); 
        Pmanager.PlayerCanMove = true;
        Gmanager._CanResiveDamage = true;
        
    }


}