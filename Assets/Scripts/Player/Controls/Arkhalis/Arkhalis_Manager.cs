

//En orden para este script funcionar ocupas: Un Monobehaivor maneje los inputs junto a un Player Input. En el Player Input el Input managerdebe estar hay y este ocupa un script object con las stats. deberian venir ya en el paquete

// Base On: https://www.youtube.com/watch?v=zHSWG05byEc 

//La gravedad en este codigo es manual al igual que las physicas. para que se tome en cuenta
using UnityEngine; 
public class Arkhalis_Movement_Manager : Movement_Manager_Abstract
{
    
    protected override void Update()
    {
        Debug.Log($"{name} IActive_HasPlayer | Aria={Pmanager._IsAriamisActive} Ark={Pmanager._IsArkhalisActive} AlrearyRender={AlrearyRender}");
        IActive_HasPlayer();
        base.Update();
    }

    protected override void IActive_HasPlayer()
    {
        if (Pmanager._IsAriamisActive == true && !AlrearyRender)
        {

            BloodMovementManager.KillMovement();
            //Unrenderall
            MagicRender_Library.OnRender_Awake(Pmanager.Render_Objects);
            //Desactivate onecharacter render and activate the other
            MagicRender_Library.enabled = true;
            BloodRender_Library.enabled = false;
            //movement and camara
            MagicMovmentManger.enabled = true;
            BloodMovementManager.enabled = false;
            //colliders: el nuevo activo (Ariamis) solido, el seguidor (Arkhalis) en trigger

        }

        if (Pmanager._IsArkhalisActive && Pmanager.Is_ArkhalisDead)
        {
            Pmanager._IsArkhalisActive = false;
        }
    }
}