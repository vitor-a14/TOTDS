using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public CinemachineVirtualCamera characterCamera;
    public CinemachineVirtualCamera birdCamera;
    private CinemachineVirtualCamera currentCamera;
    
    private int secundaryCameraPriority = 1;
    private int primaryCameraPriority = 5;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    private void Start() {
        ChangeCamera(characterCamera);
    }

    public void ChangeWorldPos(Vector3 newOffset, Vector3 translation) {
        characterCamera.OnTargetObjectWarped(characterCamera.Follow, translation);
        birdCamera.OnTargetObjectWarped(birdCamera.Follow, translation);
    }

    public void ChangeToCharacterCamera() {
        ChangeCamera(characterCamera);
    }

    public void ChangeToBirdCamera() {
        ChangeCamera(birdCamera);
    }

    private void ChangeCamera(CinemachineVirtualCamera newCamera) {
        if(currentCamera != null)
            currentCamera.Priority = secundaryCameraPriority;

        newCamera.Priority = primaryCameraPriority;
        currentCamera = newCamera;
    }
}
