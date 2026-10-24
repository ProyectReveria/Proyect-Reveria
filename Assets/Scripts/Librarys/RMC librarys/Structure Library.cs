using UnityEngine;

namespace reveria_Stadistics_Structs
{
    public class StructureLibrary 
    {
        #region  Enums

        public enum _EnemyStatus
        {
            alive, 
            death, 
            stun,
        }
        public enum Weapond_Name
        {
            /// !<Sobre los tier>
            ///  Las armas tienen tiers, no es mas que una forma bonita de serializar de forma mas segura cada arma y sus implicaciones, tienen nombres diferentes 
            /// como forma de evitar confusiones, siempre se puede volver aqui para revisarlo
            /// !</end>
            The_Red_Sun_Nikana,  //Tier 1
            The_Dual_Nikana_Halfth_Truth_and_Self_Moonligth, //tier 2
            Existence_of_Self_blood, //tier 3
            The_Midle_moon_Tear,  //tier 1
            The_Defender_of_Magic, //tier 2
            The_book_of_the_kingdom_of_Magic,  //tier 3
        }

        public enum Enemy_Type
        {
            Range_Enemy, 
            Melee_Enemy, 
            Boss_Enemy, 
            Neutral_Enemy,
            Test_Enemy,
        }
        #endregion
        #region  Enemy 
        [System.Serializable]
        public struct Enemy_UniversalStats
        {
            /// !<Sobre la vida de los enemigos>
            ///  La vida de los enemigos esta puesta como un valor 'Float' esto se debe a que la vida en general tanto de jugador como de enemigos se trabaja en valores de:
            ///  1.0 y 0.5 
            ///  esto significa ningun arma en el juego ara daño que no sea 1 o 0.5 estrictamente; 
            /// !</End>
            [Range(1f,700f)] public float EnemyHP; 
            [Range(0f,5f)] public float Enemy_Resistance; 
            //TODO: Se aplica le mismo principio de la vida al daño
            [Range(1f,3f)] public float Enemy_Damage; 
            public Vector2 Enemy_Spawn; //? Si se ocupa 

            [Range(1f,10f)]public float Enemy_Speed; 
            public Enemy_Type Enemy_Type;
            [Range(0.25f,50f)] public float Enemy_Aceleration;
            [Range(0.25f,50f)] public float Enemy_DAceleration;

            [Range (0.10f, 0.50f)] public float Enemy_LedgeCheckoofset;
            public _EnemyStatus Enemy_Status; 
            [Range(1f,5f)] public float EnemyDetectionRange; 
            [Range (1f,5f)] public float TimeBetweenAttacks; 
        }
        #endregion

        #region PlayerWeapon 
        [System.Serializable]
        public struct Weapond_Stats
        {
            //TODO: Se aplica le mismo principio de la vida al daño
            public  Weapond_Name Player_Weapond; 
            [Range(1f,10f)] public float WeapondDamage;
            [Range(1f,5f)] public float Weapond_Resistance_Penetration; 
        }

        #endregion
    }

}
