using UnityEngine;

public class Player_Universal_Convencions : MonoBehaviour , Reveria_Contract_Broker
{
    [SerializeField] private PlayerManager Pmanager; 

    public void _PlayerReciveDamage_Contract(float Damage)
    {
        Pmanager.Ariamis_HPstatus -= Damage;
    }
}
