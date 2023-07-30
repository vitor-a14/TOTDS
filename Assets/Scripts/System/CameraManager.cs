using UnityEngine;
using Cinemachine;
using System.Collections;

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

    public void PanOutCameraEffect(float duration) {
        StartCoroutine(HandleCameraPanOut(duration));
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

    private IEnumerator HandleCameraPanOut(float duration) {
        var componentBase = characterCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        float startingDistance = (componentBase as Cinemachine3rdPersonFollow).CameraDistance;
        float elapsedTime = 0f;
        float waitTime = 3f;

        while (elapsedTime < waitTime) {
            (componentBase as Cinemachine3rdPersonFollow).CameraDistance = Mathf.Lerp((componentBase as Cinemachine3rdPersonFollow).CameraDistance, 10, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }  

        yield return new WaitForSeconds(duration);

        elapsedTime = 0f;
        while (elapsedTime < waitTime) {
            (componentBase as Cinemachine3rdPersonFollow).CameraDistance = Mathf.Lerp((componentBase as Cinemachine3rdPersonFollow).CameraDistance, startingDistance, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }  

        (componentBase as Cinemachine3rdPersonFollow).CameraDistance = startingDistance;
    }
}
