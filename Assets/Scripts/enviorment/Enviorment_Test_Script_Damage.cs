using UnityEngine;

public class Enviorment_Test_Script_Damage : MonoBehaviour
{
    [Header ("Object References")]
    [SerializeField] private PlayerManager Pmanager; 
    public GameObject player; 
     [SerializeField] private Vector3 spawn = new Vector3 (0,0,0); 

    void OnCollisionEnter2D(Collision2D hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            player.transform.position = Movement_Manager._LastStableground;

            Pmanager._Arkhalis_Current_PlayerHP -= 1; 

            if (Pmanager._Arkhalis_Current_PlayerHP >= 0)
            {
                Pmanager.Restart_HP(); 
                player.transform.position = spawn; 
            }
            
        }
    }
}