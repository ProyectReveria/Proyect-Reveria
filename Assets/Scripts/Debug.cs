using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Debug_GameEnviorment : MonoBehaviour
{
    public GameObject player; 

    void Start()
    {
        Debug.Log("For test Enviorment Purposes presing the Pkey whie Debug.CS is put into Player and the Calss Debug_GameEnviorment.Update() is active, presing P transport player to the test checkpont");
    }
    void Update()
    {
        Vector3 PlayerTestCheckpoint = new Vector3 (18,12,0);
        Keyboard ky = Keyboard.current; 

        if (ky == null)
        {
            return; 
        }

        if (ky.pKey.wasPressedThisFrame)
        {
            player.gameObject.transform.position = PlayerTestCheckpoint;
        }
    }


}
