using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class DownScript : MonoBehaviour
{
    private RaycastHit2D BoxCastHit; 

    private string PlayerTag = "Player";
    bool candrop = false;

    void Update()
    {
        OnCollisionDesactivate();
    }

    private void OnCollisionEnter2D(Collision2D DropOption)
    {
        if (DropOption.gameObject.CompareTag(PlayerTag))
        {
            candrop = true;
        }
    }

    private void OnCollisionExit2D(Collision2D DropOption)
    {
        if (DropOption.gameObject.CompareTag(PlayerTag))
        {
            candrop = false;
        }
    }

    void OnCollisionDesactivate()
    {
        
        if (candrop && Input_Manager._OnDropDownPlatformEvent)
        {
            candrop = false;
            StartCoroutine(DesactivateCollider());
        }
    }

    private IEnumerator DesactivateCollider()
    {
        Collider2D thisObjectcollider = this.gameObject.GetComponent<TilemapCollider2D>();
        thisObjectcollider.enabled = false; 
        yield return new WaitForSeconds(0.50f);
        thisObjectcollider.enabled = true;
    }
}
