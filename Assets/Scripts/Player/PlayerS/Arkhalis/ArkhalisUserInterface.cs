using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArkhalisUserInterface : MonoBehaviour
{
    #region References
    [Header ("Object References")]

    [SerializeField] private PlayerStats PlayerStats;
    [SerializeField] private PlayerManager Pmanajer; 
    
    [Header ("UI")]
    [SerializeField] private TMP_Text Hp_UIArkhalis;
    [SerializeField] private RawImage Interaction_E; 

    
    #endregion
    #region Unity Updates

    void Update()
    {
        Overload_HP_UI_Function();
        Overload_E_UI_Function();
    }

    #endregion

    #region  UI_Arkhalis
    void Overload_HP_UI_Function()
    {
        Hp_UIArkhalis.text = $"Arkhalis HP: {Pmanajer.Arkhalis_HPStatus}|{PlayerStats._Arkhalis_HP}"; 
    }

    void Overload_E_UI_Function()
    {
        if (Pmanajer._CanInteract)
        {
            Interaction_E.enabled = true;
        } else
        {
            Interaction_E.enabled = false; 
        }

    }
    #endregion
}
