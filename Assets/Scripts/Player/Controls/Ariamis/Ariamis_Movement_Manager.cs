using UnityEngine;

public class Ariamis_Movement_Manager : Movement_Manager_Abstract
{
    
    protected override void Update()
    {
        IActive_HasPlayer();
        base.Update();
    }

    protected override void IActive_HasPlayer()
    {
        if (Pmanager._IsArkhalisActive == true)
        {
            BloodMovementManager.enabled = true;
            BloodCamara.enabled = true;
            MagicCamara.enabled = false;
            MagicMovmentManger.enabled = false;
            
        }
    }
}
