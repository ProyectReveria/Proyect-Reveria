using UnityEngine;
using C_Reveria.TagsLibrary;
using System;
using RMC.Player_Interaction.ThePlayerGeneralManager;
using System.Collections.Generic;
[DefaultExecutionOrder (1000)]
public abstract class RMC_RenderLibrary : MonoBehaviour , RMC_Render_Contracts
{
    [Header ("AllGameobjects")]
   
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
