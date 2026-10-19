using UnityEngine;
using RMC.SaveFile.SaveSerialice;
using System;
using RMC.Player_Interaction.ThePlayerGeneralManager;
[Serializable]

public class SaveZoneScript : MonoBehaviour
{
    [Header ("References")]
     [SerializeField] private GameObject Player; 
    public Vector2 spawn; 
    [SerializeField] private GameManager Gmanager; 
    
    private bool _IsPlayerInside;

    void Awake()
    {
        spawn = RMC_ThePlayerGeneralManager.RMC_SaveFile_Information.SpawnPoint;
    }
    void OnCollisionEnter2D(Collision2D NewSpawnPoint)
    {
        if (NewSpawnPoint.gameObject.CompareTag("Player"))
        {
            Gmanager._CanInteract = true; 
            _IsPlayerInside = true;
        } 

    }

    void OnCollisionExit2D(Collision2D NewSpawnPoint)
    {
        if (NewSpawnPoint.gameObject.CompareTag("Player"))
        {
            Gmanager._CanInteract = false;
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
            spawn = Player.transform.position; 
        }
    }
}
