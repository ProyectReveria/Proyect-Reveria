using UnityEngine;
using RMC.SaveFile.SaveSerialice;
using System;
using RMC.Player_Interaction.ThePlayerGeneralManager;
[Serializable]

public class SaveZoneScript : MonoBehaviour
{
    [Header ("References")]
    [SerializeField] RMC_ThePlayerGeneralManager GeneralManager; 
     [SerializeField] private GameObject Player; 

     [SerializeField] private PlayerManager playerManager;

    
    private bool _IsPlayerInside;

    void OnCollisionEnter2D(Collision2D NewSpawnPoint)
    {
        if (NewSpawnPoint.gameObject.CompareTag("Player"))
        {
            playerManager._CanInteract = true; 
            _IsPlayerInside = true;
        } 

    }

    void OnCollisionExit2D(Collision2D NewSpawnPoint)
    {
        if (NewSpawnPoint.gameObject.CompareTag("Player"))
        {
            playerManager._CanInteract = false;
            _IsPlayerInside = false;
        }

    }

    void Update()
    {
        OverloadSaveInteract_Function(); 
    }

    void OverloadSaveInteract_Function()
    {
        if (Input_Manager.Interact_WasPress && _IsPlayerInside)
        {
            RMC_ThePlayerGeneralManager.Game_Data_FromSave.SpawnPoint = Player.transform.position; 
        }
    }
}
