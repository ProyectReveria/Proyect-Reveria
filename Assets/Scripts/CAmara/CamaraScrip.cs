using Unity.Cinemachine;
using UnityEngine;

public class CharacterCameraSwitch : MonoBehaviour
{
    [SerializeField] private CinemachineCamera VCam;
    [SerializeField] private Transform BloodTarget;  // Arkhalis
    [SerializeField] private Transform MagicTarget;  // Ariamis
    [SerializeField] private PlayerManager Pmanager;

    private bool _lastArkhalisActive;

    void Start()
    {
        _lastArkhalisActive = Pmanager._IsArkhalisActive;
        VCam.Follow = _lastArkhalisActive ? BloodTarget : MagicTarget;
    }

    void Update()
    {
        if (Pmanager._IsArkhalisActive == _lastArkhalisActive) return;

        _lastArkhalisActive = Pmanager._IsArkhalisActive;
        VCam.Follow = _lastArkhalisActive ? BloodTarget : MagicTarget;
    }
}