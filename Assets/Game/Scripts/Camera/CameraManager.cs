using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using System;
public class CameraManager : MonoBehaviour
{
    [SerializeField]
    public CameraState CameraState;

    [SerializeField]
    private CinemachineCamera _tpsCamera;
    [SerializeField]
    private CinemachineCamera _fpsCamera;
    [SerializeField]
    private InputManager _inputManager;
    public Action OnChangePerspective;
    
    private void SwitchCamera()
    {
        OnChangePerspective();
        if (CameraState == CameraState.ThirdPerson)
        {
            CameraState = CameraState.FirstPerson;
            _tpsCamera.gameObject.SetActive(false);
            _fpsCamera.gameObject.SetActive(true);
        }
        else
        {
            CameraState = CameraState.ThirdPerson;
            _tpsCamera.gameObject.SetActive(true);
            _fpsCamera.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        _inputManager.OnChangePOV += SwitchCamera;
    }
    
    private void OnDestroy()
    {
        _inputManager.OnChangePOV -= SwitchCamera;
    }

    public void SetTPSFieldOfView(float fieldOfView)
    {
        _tpsCamera.Lens.FieldOfView = fieldOfView;
    }

    public void SetFPSClampedCamera(bool isClamped, Vector3 playerRotation)
    {
        CinemachinePanTilt pov = _fpsCamera.GetComponent<CinemachinePanTilt>();
        if (isClamped)
        {
            pov.PanAxis.Wrap = false;
            pov.PanAxis.Range.x = playerRotation.y - 45;
            pov.PanAxis.Range.y = playerRotation.y + 45;
            // pov.m_HorizontalAxis.m_Wrap = false;
            // pov.m_HorizontalAxis.m_MinValue = playerRotation.y - 45;
            // pov.m_HorizontalAxis.m_MaxValue = playerRotation.y + 45;
        }
        else
        {
            pov.PanAxis.Wrap = true;
            pov.PanAxis.Range.x = -180;
            pov.PanAxis.Range.y = 180;
            // pov.m_HorizontalAxis.m_MinValue = -180;
            // pov.m_HorizontalAxis.m_MaxValue = 180;
            // pov.m_HorizontalAxis.m_Wrap = true;
        }
    }
}