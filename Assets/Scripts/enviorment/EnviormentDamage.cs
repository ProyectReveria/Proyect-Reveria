using UnityEngine;
using System.Collections;
using RMC.Player_Interaction.ThePlayerGeneralManager;
using NUnit.Framework;
using C_Reveria.Game_Stats_And_Variables;


public class EnviormentDamage : MonoBehaviour
{
    #region  Variables
    [Header ("Object References")]
    [SerializeField] private Arkhalis_Movement_Manager BloodManager;
    [SerializeField] private Ariamis_Movement_Manager MagicManager;
    [SerializeField] private GameObject BloodCharacter;  //Arkhalis
    [SerializeField] private GameObject MagicCharacter; // Ariamis
    [SerializeField] private PlayerStats PlayerStats;
    [SerializeField] private PlayerManager Pmanager; 

    #endregion
    void OnCollisionEnter2D(Collision2D hit)
    {
        

        if (hit.gameObject == BloodCharacter && Pmanager._IsArkhalisActive || hit.gameObject == MagicCharacter && Pmanager._IsAriamisActive )
        {   
            StartCoroutine(Respawn_onStableGround());
        }
    }

    IEnumerator Respawn_onStableGround()
    {   
        Pmanager.PlayerCanMove = false;
        if (Pmanager._IsArkhalisActive && Pmanager.Arkhalis_HPStatus > 0)
        {
            Pmanager.Arkhalis_HPStatus -= PlayerStats.LowDamageEnviorment;
            yield return new WaitForSeconds(PlayerStats.RespawnTime);
            BloodCharacter.transform.position = BloodManager._LastStableground; 
            MagicCharacter.transform.position = BloodManager._LastStableground;
            Pmanager.PlayerCanMove = true;
        }

        if (Pmanager._IsAriamisActive && Pmanager.Ariamis_HPstatus > 0)
        {
            Pmanager.Ariamis_HPstatus -= PlayerStats.LowDamageEnviorment;
            yield return new WaitForSeconds(PlayerStats.RespawnTime);
            MagicCharacter.transform.position = MagicManager._LastStableground;
            BloodManager.transform.position = MagicManager._LastStableground;
            Pmanager.PlayerCanMove = true;
        }

        if (Pmanager.Ariamis_HPstatus <= 0 || Pmanager.Arkhalis_HPStatus <= 0)
        {
            yield return new WaitForSeconds(PlayerStats.RespawnTime);
            MagicCharacter.transform.position = RMC_ThePlayerGeneralManager.Game_Data_FromSave.SpawnPoint;
            BloodManager.transform.position = RMC_ThePlayerGeneralManager.Game_Data_FromSave.SpawnPoint;
            Pmanager.PlayerCanMove = true;
        }
    }
}