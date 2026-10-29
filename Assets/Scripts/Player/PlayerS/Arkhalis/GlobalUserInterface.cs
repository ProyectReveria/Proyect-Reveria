using RMC.Player_Interaction.ThePlayerGeneralManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalInterface : MonoBehaviour
{
    #region References
    [Header ("Object References")]
    [SerializeField] private RMC_ThePlayerGeneralManager GManager;
    [SerializeField] private PlayerStats PlayerStats;
    [SerializeField] private PlayerManager Pmanajer; 
    
    [Header ("UI")]
    [SerializeField] private TMP_Text GlobalHPUI_onTest;
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
        if (Pmanajer._IsArkhalisActive)
        {
            GlobalHPUI_onTest.text = $"Arkhalis HP: {Pmanajer.Arkhalis_HPStatus}|{GManager.ArkhalisHPStatus_AfterUpgrades}"; 
        } else if (Pmanajer._IsAriamisActive)
        {
            GlobalHPUI_onTest.text = $"Ariamis HP : {Pmanajer.Ariamis_HPstatus} | {GManager.AriamisHPStatus_AfterUpgrades}";
        }

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
