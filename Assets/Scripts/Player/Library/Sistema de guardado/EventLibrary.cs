using UnityEngine;
using UnityEngine.Events;

namespace RMC.SaveFile.EventLibrary
{
    public class EventLibrary 
    {
        public static UnityEvent GameSaving = new UnityEvent();
        public static UnityEvent<bool> GameStartLoading = new UnityEvent<bool>();
        public static UnityEvent<bool> GameendendLoading = new UnityEvent<bool>(); 
        public static void Execute_GameSaveLoading() => GameSaving.Invoke(); 
        public static void Execute_GameendstartLoading() => GameStartLoading.Invoke(true);
        public static void Execute_GameendendLoading() => GameendendLoading.Invoke(false);
        
    }
}

