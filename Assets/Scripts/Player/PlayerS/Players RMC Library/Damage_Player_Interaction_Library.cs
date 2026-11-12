using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RMC.Player_Interaction.Damage_Interaction
{
    public class IFrames : MonoBehaviour
     {

        public bool FromLibrary_Dot_Iframes = false; 
        public Coroutine FromLibrary_Dot_Iframes_Process;

        void OnDisable()
        {
            FromLibrary_Dot_Iframes = false;
            FromLibrary_Dot_Iframes_Process = null;        
        }

        
        void DealDamageToplayer(float damage)
        {
            float _RealDamageCalculation = Mathf.Floor(damage * 2f) /2f;

        }


        public void Iframes_Active(float Iframes_Timer )
        {

            if (FromLibrary_Dot_Iframes_Process != null)
            {
                StopCoroutine(FromLibrary_Dot_Iframes_Process);
            }
            FromLibrary_Dot_Iframes_Process = StartCoroutine(Iframes_WateTime(Iframes_Timer));
        }

        private float Iframe_OnDamageOverTimeEffect(bool IsActive, float damage,float Iframes)
        {
            float Damage_State = 1;
            while (IsActive)
            {
                Damage_State += damage;
                Damage_State = Mathf.Ceil(Damage_State);
                StartCoroutine(Iframes_WateTime(Iframes));
            }

            return Damage_State;

        }

        public IEnumerator On_AnimacionIframes(float Animation_Duration)
        {
            yield return new WaitForSeconds(Animation_Duration);
        }


        private IEnumerator Iframes_WateTime(float Iframes_Timer)
        {
            FromLibrary_Dot_Iframes = true;


            //TimeFromIframes
            yield return new WaitForSeconds(Iframes_Timer);

            FromLibrary_Dot_Iframes = false;
        }
        
    }
}
