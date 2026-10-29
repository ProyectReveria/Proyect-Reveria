using UnityEngine;

public class Ariamis_Movement_Manager : Movement_Manager_Abstract
{
    
    protected override void Update()
    {
        Debug.Log($"{name} IActive_HasPlayer | Aria={Pmanager._IsAriamisActive} Ark={Pmanager._IsArkhalisActive} AlrearyRender={AlrearyRender}");
        IActive_HasPlayer();
        base.Update();
    }

    protected override void IActive_HasPlayer()
    {
        if (Pmanager._IsArkhalisActive == true && !AlrearyRender)
        {
            
            MagicMovmentManger.KillMovement();
            //Unrenderall
            BloodRender_Library.OnRender_Awake(Pmanager.Render_Objects);
            //Desactivate onecharacter render and activate the other
            BloodRender_Library.enabled = true;
            MagicRender_Library.enabled = false;
            //movement and camara
            BloodMovementManager.enabled = true;
            MagicMovmentManger.enabled = false;
            //colliders: el nuevo activo (Arkhalis) solido, el seguidor (Ariamis) en trigger

        }

        if (Pmanager._IsAriamisActive && Pmanager.Is_AriamisDead)
        {
            Pmanager._IsAriamisActive = false;

        }
    }
}
