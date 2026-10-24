using UnityEngine;
using C_Reveria.TagsLibrary;
using System;
using RMC.Player_Interaction.ThePlayerGeneralManager;
using System.Collections.Generic;
[DefaultExecutionOrder (1000)]
public abstract class RMC_RenderLibrary : MonoBehaviour , RMC_Render_Contracts
{
    [Header ("AllGameobjects")]
    public List<GameObject> allGameobjects_OnUpdateRender = new(); //Cosas van aqui: Enemigos, efectos visuales, cosas que den un peso impositivo al sistema perpetuo.
    [Header ("Referencias")]
    [SerializeField] protected CircleCollider2D RenderCollider; 
    [SerializeField] protected LayerMask Ground_layer; 
    [SerializeField] protected LayerMask DamageEnviorment_layer; 
    [SerializeField] protected LayerMask Player_Layer;

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
        OnRender_Awake(allGameobjects_OnUpdateRender);
    }

    #endregion

    #region  OnRenderContracts
    protected virtual void OnEnter_RenderRange(Collision2D DetectedObject,bool Turn_On_of)
    {
        if (allGameobjects_OnUpdateRender.Contains(DetectedObject.transform.gameObject))
        {
            DetectedObject.transform.gameObject.SetActive(Turn_On_of);
        }
    }

    protected virtual void OnRender_Awake(List<GameObject> Listed_objects)
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
        allGameobjects_OnUpdateRender.Add(self);
    }

    public void RemmoveToList(GameObject self)
    {
        allGameobjects_OnUpdateRender.Remove(self);
    }
    #endregion

}
