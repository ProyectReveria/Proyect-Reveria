using UnityEngine;

[CreateAssetMenu(menuName = "C.Reveria_Player_Input_Manager")]

public class Movement_Stats : ScriptableObject 
{
    [Header ("PlayerTags")]

    public string Arkhalis_LayerMask; 
    public string Ariamis_LayerMask; 
    
    [Header("Walk")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 5f;
    [Range(0.25f , 50f)] public float GAcceleration = 2.5f; 
    [Range(0.25f , 50f)] public float GDesaseleration = 10f; 
    [Range(0.25f , 50f)] public float Aacceleration = 7.5f; 
    [Range(0.25f , 50f)] public float ADeceleration = 7.5f;

    [Header ("Run")]
    [Range(1f ,100f)] public float MaxRunSpeed = 15f; 

    [Header("CheckData & Colissions")]
    public LayerMask Glayer; 
    public float GDRayLength = 0.02f; 
    public float HeadDRayLength = 0.02f; 
    [Range(0f , 1f)] public float HeadWidth = 0.75f; 
    
}
