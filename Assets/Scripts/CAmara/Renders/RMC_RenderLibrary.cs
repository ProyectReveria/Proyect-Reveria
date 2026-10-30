using UnityEngine;
using C_Reveria.TagsLibrary;
using System;
using RMC.Player_Interaction.ThePlayerGeneralManager;
using System.Collections.Generic;
[DefaultExecutionOrder (1000)]
public abstract class RMC_RenderLibrary : MonoBehaviour , RMC_Render_Contracts
{
   
    [Header ("Referencias")]
    [SerializeField] protected CircleCollider2D RenderCollider; 
    [SerializeField] protected PlayerManager Pmanager;

    //Referencial Data from the script
    #region  OnUpdate_FromUnity

    protected void OnCollisionEnter2D(Collision2D Rendered_objects)
    {
        OnEnter_RenderRange(Rendered_objects,true); 
    }

    protected void OnCollisionExit2D(Collision2D Rendered_objects)
    {
        OnEnter_RenderRange(Rendered_objects,false); 
    }

    protected void Awake()
    {
        OnRender_Awake(Pmanager.Render_Objects);
    }

    #endregion

    #region  OnRenderContracts
    protected virtual void OnEnter_RenderRange(Collision2D DetectedObject,bool Turn_On_of)
    {
        if (Pmanager.Render_Objects.Contains(DetectedObject.transform.gameObject))
        {
            DetectedObject.transform.gameObject.SetActive(Turn_On_of);
        }
    }

    public virtual void OnRender_Awake(List<GameObject> Listed_objects)
    {
        foreach (GameObject gameObject in Listed_objects)
        {
            gameObject.SetActive(false);
        }
    }

    #endregion

    #region AddToList
    //! cada efecto visual, enemigo, cosas que alteren el rendimiento en update que van a estar fuera o dentro del rango del collider se tienen que activar y desactivar desde la lista en pmanager
    //! por favor usar estos dos metodos YA HECHOS para agregarlos a la lista, si no lo hacen are que aiden dibuje manos por 3 horas. muchas gracias.
    //! si son de progra los hago programar una base de datos sin SQL en C++ avisados estan y un sistema de kubernets en js 2
    //! si son de narrativa los are escribir 700 lineas mal escritas sobe la constutcion mexicana y corregirlas con un lapisero de tinta ultravioleta para uqe no se vean.
    public void AddToList(GameObject self)
    {
        Pmanager.Render_Objects.Add(self);
    }

    public void RemmoveToList(GameObject self)
    {
        Pmanager.Render_Objects.Remove(self);
    }
    #endregion

}
