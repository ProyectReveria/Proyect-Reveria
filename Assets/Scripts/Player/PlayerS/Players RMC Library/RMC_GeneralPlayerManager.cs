using System;
using UnityEngine;
using RMC.Player_Interaction.Damage_Interaction;

using RMC.SaveFile.SaveSerialice;
using Unity.VectorGraphics;
///<Recordatorios>
/// Agregar los flac de las mejoras conforme se agreugen.
namespace RMC.Player_Interaction.ThePlayerGeneralManager
{
    public class RMC_ThePlayerGeneralManager : MonoBehaviour
    {   
        #region  Serializeble Data and Update Serializable DAta
        [Serializable]
        public class RMC_SaveFile_Information : MonoBehaviour
        {
            [Header ("Ignore")]
            private bool _IsalrearyUpdate = false; 
            public  bool _DashisAllow ;
            public  bool _WallSlideisAllow ;
            public  bool  _DoubleJumpisAllow; 
            public  bool _TripleJumpisAllow;
            public  bool _DashIsActive;

            public  bool _Player_Money;

            public  Vector2 SpawnPoint; 
            //Do Not Give It the Wrong Path (Can be really bad)
            public  string _SafeFilePath; 
            public string PlayerScene; 

            //Static Versions for use
            public static bool _DashisAllow_Static ;
            public static bool _WallSlideisAllow_Static ;
            public static bool  _DoubleJumpisAllow_Static; 
            public static bool _TripleJumpisAllow_Static;
            public static bool _DashIsActive_Static;

            public static bool _Player_Money_Static;

            public  static Vector2 SpawnPoint_Static; 

            public static string _SafeFilePath_Static;
            //Se ocupa extraer el nombre de la escena y vovlerlo u nstring
            public static string PlayerScene_Static;

            //Redundantemente necesario

            void Awake()
            {
                if (_IsalrearyUpdate == false)
                {
                    Update_DataOnClase();
                    
                    if (SpawnPoint == null)
                    {
                        SpawnPoint = Vector2.zero; 
                    }
                    _IsalrearyUpdate = true;
                }

            }

            void Update()
            {
                if (_IsalrearyUpdate == false )
                {
                    Update_DataOnClase();
                    _IsalrearyUpdate = true;
                }
                
            }

            public void OnDeserializeUpdate(RMC_SaveFile_Information data )
            {
                _WallSlideisAllow_Static = data._WallSlideisAllow ;
                _DoubleJumpisAllow_Static = data._DoubleJumpisAllow; 
                _TripleJumpisAllow_Static = data._TripleJumpisAllow; 
                _DashIsActive_Static = data._DashIsActive;
                _Player_Money_Static = data._Player_Money; 
                SpawnPoint_Static = data.SpawnPoint; 
                _SafeFilePath_Static = data._SafeFilePath;
                PlayerScene_Static = data.PlayerScene; 
                _IsalrearyUpdate = false;
            }

            public void Update_DataOnClase()
            {
                _WallSlideisAllow = _WallSlideisAllow_Static;
                _DoubleJumpisAllow = _DoubleJumpisAllow_Static;
                _TripleJumpisAllow = _TripleJumpisAllow_Static;
                _DashIsActive = _DashIsActive_Static;
                _Player_Money = _Player_Money_Static;
                SpawnPoint = SpawnPoint_Static;
                _SafeFilePath = _SafeFilePath_Static;
                PlayerScene = PlayerScene_Static;
 
            }

        }
        #endregion


        #region  Variables for Reference
        [Header ("(This is A Messege: This object DO NOT work has 'part of the object' is a SincronLibrary, this imply in order for this library work need Serialize References)")]

        ///<Librerias Sincronizadas> 
        /// Una libreria sincronizada consiste de un componente o Script el cual contiene dentro definiciones y ejecuta funciones, pero no directamente
        /// Similar a una libreria con la diferencia de que tiene datos que de hecho usan otros scripts
        /// Estos son Pasivos y no executan el codigo, solo almacenan las funciones par poder ejecuarlo y suelen requerir variables serialziadas, Gameobject o incluso cosas superiores
        /// 
        /// Este tambien puede ser llamado para obtener las referencias tiene dentro
        /// 
        /// Esto permite solo llamar a esta libreria para usar sus metodos y tambien sus variables como Estados, Banderas e incluso señales
        /// 
        /// Todo funciona en el update a no ser se diga MUY explicitamente lo contrario
        /// ! Ejemplo Explicito: FixedUpdate_PlayerJump() 
        /// 
        /// <EndM>
        
        public RMC_SaveFile_Information _PreSerializeData; 
        public RMC_SaveFile_Information _DeserialzieData; 
        

        [Header ("Player HPStatus")]
        public float Arkhalis_HPStatus; 
        public float Ariamis_HPstatus; 

        [Header ("Referencial Data")]
        public GameObject[] Players = new GameObject[1]; 

        public PlayerStats _ScriptableObject_PlayerStats;

        //Non visible
        private float ArkhalisHPStatus_AfterUpgrades; 
        private float AriamisHPStatus_AfterUpgrades;

        #endregion

        #region Awake 
        void Awake()
        {
            //Agregar las confirmaciones boleanas de las upgrades primero
            ConfirmUpgrades();
            Arkhalis_HPStatus = ArkhalisHPStatus_AfterUpgrades ; 
            Ariamis_HPstatus = AriamisHPStatus_AfterUpgrades; 
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
            Arkhalis_HPStatus = _ScriptableObject_PlayerStats._Arkhalis_HP; 
            Ariamis_HPstatus = _ScriptableObject_PlayerStats._Ariamis_HP; 

            _PreSerializeData = new RMC_SaveFile_Information(); 
            SerialiceGameData.Serialize_GameData(_PreSerializeData);
            _PreSerializeData = null;
        }

        void Deserialice(  ){
            SerialiceGameData.Deserialice_GameDAta<RMC_SaveFile_Information>(RMC_SaveFile_Information._SafeFilePath_Static,Action_Deserialice);
        }

        void Action_Deserialice(RMC_SaveFile_Information data)
        {
            _DeserialzieData = data;
            _DeserialzieData.OnDeserializeUpdate(data);

        }

        #endregion


    }

}
