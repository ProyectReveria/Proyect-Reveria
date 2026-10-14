using TMPro;
using UnityEngine;

public class ArkhalisUserInterface : MonoBehaviour
{
    [Header ("Object References")]

    [SerializeField] private PlayerStats PlayerStats;
    [SerializeField] private PlayerManager Pmanajer; 
    
    [Header ("UI")]
    [SerializeField] private TMP_Text Hp_UIArkhalis;

    

    #region Unity Updates

    void Update()
    {
        Overload_HP_UI_Function();
    }

    #endregion

    #region  UI_Arkhalis
    void Overload_HP_UI_Function()
    {
        Hp_UIArkhalis.text = $"Arkhalis HP: {Pmanajer._Arkhalis_Current_PlayerHP}|{PlayerStats._Arkhalis_HP}"; 
    }
    #endregion
}
