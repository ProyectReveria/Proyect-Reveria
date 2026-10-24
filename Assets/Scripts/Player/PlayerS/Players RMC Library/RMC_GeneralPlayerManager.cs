using System;
using UnityEngine;
using RMC.Player_Interaction.Damage_Interaction;

using RMC.SaveFile.SaveSerialice;
using Unity.VectorGraphics;
using System.IO;
using System.Collections.Generic;
///<Recordatorios>
/// Agregar los flac de las mejoras conforme se agreugen.
namespace RMC.Player_Interaction.ThePlayerGeneralManager
{
    [DefaultExecutionOrder(-7777777)]
    public class RMC_ThePlayerGeneralManager : MonoBehaviour
    {   

        [Serializable]
        public class RMC_SaveFile_Information 
        {
            [Header ("What character is active")]
            

            [Header ("Ignore")]

            public  bool _WallSlideisAllow = true;
            public  bool  _DoubleJumpisAllow = true; 
            public bool _WalljumpIsAllow = true;
            public  bool _DashIsActive = true;

            public  bool _Player_Money = true;

            public  Vector2 SpawnPoint; 
            //Do Not Give It the Wrong Path (Can be really bad)
            public  string _SafeFilePath = Path_List.DefaultSaveFile_Path; 
            public string PlayerScene; 

            //Redundantemente necesario

        }


        #region  Variables for Reference
        [Header ("(This is A Messege: This object DO NOT work has 'part of the object' is a SincronLibrary, this imply in order for this library work need Serialize References)")]
        
        public static RMC_SaveFile_Information Game_Data_FromSave; 
        public static string DefaulPath = Path_List.DefaultSaveFile_Path; 


        [Header ("Referencial Data")]
        public GameObject[] Players = new GameObject[1]; 
        public PlayerManager Pmanager; 

        public PlayerStats _ScriptableObject_PlayerStats;

        //Non visible
        private float ArkhalisHPStatus_AfterUpgrades; 
        private float AriamisHPStatus_AfterUpgrades;

        #endregion

        #region Awake 
        void Awake()
        {
            Path_List.InitiatePath();
            Game_Data_FromSave = new RMC_SaveFile_Information();

            
            if (Game_Data_FromSave._SafeFilePath == null ||Game_Data_FromSave._SafeFilePath is string  == false)
            {
                DefaulPath = Path_List.DefaultSaveFile_Path;
            }else
            {
                DefaulPath = Game_Data_FromSave._SafeFilePath; 
            }

        List<string> guardados = SerialiceGameData.Find_AllSaveFiles();
        if (guardados.Count > 0)
        {
            SerialiceGameData.Deserialice_GameDAta<RMC_SaveFile_Information>(guardados[0], Action_Deserialice);
        }

            ConfirmUpgrades();
            Pmanager.Arkhalis_HPStatus = ArkhalisHPStatus_AfterUpgrades ; 
            Pmanager.Ariamis_HPstatus = AriamisHPStatus_AfterUpgrades; 
        }

        #endregion

        #region Create_Object For Safe

        void ConfirmUpgrades()
        {
            //Agregar las confirmaciones boleanas de las upgrades primero y agregar cada if statement conforme se agreguen
            ArkhalisHPStatus_AfterUpgrades = _ScriptableObject_PlayerStats._Arkhalis_HP;
            AriamisHPStatus_AfterUpgrades = _ScriptableObject_PlayerStats._Ariamis_HP; 
        }

        void SaveAndRegen()
        {
            Pmanager.Arkhalis_HPStatus = _ScriptableObject_PlayerStats._Arkhalis_HP; 
            Pmanager.Ariamis_HPstatus = _ScriptableObject_PlayerStats._Ariamis_HP; 
            SerialiceGameData.Serialize_GameData(Game_Data_FromSave);
            
        }

        static void  Deserialice(){
            SerialiceGameData.Deserialice_GameDAta<RMC_SaveFile_Information>(DefaulPath,Action_Deserialice);
        }

        static void  Action_Deserialice(RMC_SaveFile_Information data)
        {
            Game_Data_FromSave = data;
            DefaulPath = data._SafeFilePath;

        }

        #endregion


    }

}
