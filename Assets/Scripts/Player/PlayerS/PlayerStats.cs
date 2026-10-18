using UnityEngine; 

[CreateAssetMenu(menuName = "Arkhalis & Ariamis Stadistics")]
public class PlayerStats : ScriptableObject
{
    [Header ("Damage by enviorment & Respawn time")]

    [Range (1f,5f)] public float RespawnTime = 1; 
    [Range (1f,4.5f)] public float LowDamageEnviorment = 1f; 
    [Range (1f,4.5f)] public float MediumDamageEnviorment = 3f; 
    [Range (1f,4.5f)] public float ExtremeDamageEnviorment = 4.5f; 

    [Header("Both Players Stadistics")]

    //?Money esta temporalmente aqui
    [Range (0, 999)] public int Money = 0; 
    [Header("Arkhalis")]
    [Range (5f, 10f)] public float _Arkhalis_HP = 5; 
    [Range (0f,2f)] public float _Arkhalis_Resistance = 0f;

    [Range (1f,5f)] public float _Arkhalis_Melee_Damage = 1f; 
    [Range (0.5f,1f)] public float _Arkhalis_ongRange_Damage = 0.5f; 
    [Range (10f,20f)] public float _Arkhalis_HealthDamageRatio = 10f; 
    [Range (1f,2f)] public float NumberofHealthHealth = 1f; 

    [Header("Ariamis")]
    [Range (5f, 10f)] public float _Ariamis_HP = 5; 
    [Range (0f,2f)] public float _Ariamis_Resistance = 0f;
    [Range (0.5f,3f)] public float _Ariamis_Melee_Damage = 0.5f; 
    [Range (2f,10f)] public float _Ariamis_ongRange_Damage_Charge = 2f; 
    [Range (0.5f, 1f)] public float _Ariamis_ongRange_Damage_Notcharge = 0.5f; 
    [Range (4f,8f)] public float _Ariamis_RedCrystal_Damage_Range = 4f; 
    [Range (20f,80f)] public float _Ariamis_MagicContainerStack = 20f; 
    [Range (1f,2f)] public float _AriamisGaingofMagicStack = 1f; 
    [Range (1f,2f)] public float _Ariamis_HealethRatio = 1f; 
}