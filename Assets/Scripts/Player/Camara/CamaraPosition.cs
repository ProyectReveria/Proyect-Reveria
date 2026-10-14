using UnityEngine;

public class CamaraPosition : MonoBehaviour
{
    [Header("Player")]
    public GameObject Player;
    [Header("Camara")]
    public Camera Player_Camera;
    [Header ("Camara OffSet")]
    public Vector3 offsert = new Vector3(0f,0f,-10f);

    private void Awake()
    {
        Player_Camera.transform.position = Vector3.zero;
    }

    private void LateUpdate()
    {
        Overload_CamaraFallowFunction();
    }

    void Overload_CamaraFallowFunction()
    {
        Player_Camera.transform.position = Player.transform.position + offsert;


    }


}
