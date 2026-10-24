

//En orden para este script funcionar ocupas: Un Monobehaivor maneje los inputs junto a un Player Input. En el Player Input el Input managerdebe estar hay y este ocupa un script object con las stats. deberian venir ya en el paquete

// Base On: https://www.youtube.com/watch?v=zHSWG05byEc 

//La gravedad en este codigo es manual al igual que las physicas. para que se tome en cuenta
public class Arkhalis_Movement_Manager : Movement_Manager_Abstract
{
    
    protected override void Update()
    {
        IActive_HasPlayer();
        base.Update();
    }

    protected override void IActive_HasPlayer()
    {
        if (Pmanager._IsAriamisActive  == true)
        {
            MagicMovmentManger.enabled = true;
            MagicCamara.enabled = true;
            BloodCamara.enabled = false;
            BloodMovementManager.enabled = false;
            
            
        }
    }
}