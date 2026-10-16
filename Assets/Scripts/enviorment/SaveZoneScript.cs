using UnityEngine;

public class SaveZoneScript : MonoBehaviour
{
    [Header ("References")]
     [SerializeField] private GameObject Player; 
    [SerializeField] private EnviormentDamage SpawnScript;
    [SerializeField] private GameManager Gmanager; 
    
    private bool _IsPlayerInside; 


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
            SpawnScript.spawn = Player.transform.position; 
            Debug.Log("Se guardo correctamente el checkpoint");
        }
    }
}
