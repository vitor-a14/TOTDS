using System.Collections;
using UnityEngine;
using TMPro;
using Cinemachine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    [Header("Panel")]
    public GameObject debugPanel;
    public TMP_Text frameRateText;
    public TMP_Text posText;

    [Header("Camera")]
    public Transform debugCameraPivot;
    public Transform debugCameraMover;
    public CinemachineVirtualCamera debugCamera;
    public float movementSpeed;

    private Vector3 direction;

    [Header("Dependencies")]
    [SerializeField] private Transform birdTransform;
    [SerializeField] private Transform playerTransform;

    private bool showDebugPanel = false;
    private bool usingDebugCamera = false;

    private PlayerController player;

    private void Start() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        player = PlayerController.Instance;

        player.inputs.Debug.ShowPanel.performed += ctx => HandleDebugPanel(!showDebugPanel);
        player.inputs.Debug.DebugCamera.performed += ctx => HandleDebugCamera(!usingDebugCamera);

        HandleDebugCamera(false);
        HandleDebugPanel(false);
    }

    private void HandleDebugPanel(bool isActive) {
        showDebugPanel = isActive;
        debugPanel.SetActive(showDebugPanel);

        if(showDebugPanel)
            StartCoroutine(UpdateFrameRateText());
        else
            StopCoroutine(UpdateFrameRateText());
    }

    private void HandleDebugCamera(bool isActive) {
        usingDebugCamera = isActive;

        if(usingDebugCamera) {
            debugCameraMover.position = Camera.main.transform.position;
            debugCameraPivot.rotation = Camera.main.transform.rotation;
            debugCamera.Priority = 90;
            player.canMove = false;
            CameraController.Instance.isActive = false;
        } else {
            debugCamera.Priority = 0;
            player.canMove = true;
            CameraController.Instance.isActive = true;
        }
    }

    private void Update() {
        if(usingDebugCamera) {
            debugCameraPivot.localEulerAngles = new Vector3(CameraController.Instance.smothedPitch, CameraController.Instance.smothedYaw);
            direction = (debugCameraPivot.forward * player.input.y + debugCameraPivot.right * player.input.x) * movementSpeed;
            debugCameraMover.Translate(direction * Time.deltaTime, Space.World);
        }
    }

    IEnumerator UpdateFrameRateText() {
        while(true) {
            yield return new WaitForSeconds(0.12f);
            frameRateText.text = "FPS: " + Mathf.RoundToInt(1.0f / Time.deltaTime);

            if(BirdController.Instance.piloting) {
                posText.text = "Universal position: " + birdTransform.position;
            } else {
                posText.text = "Universal position: " + playerTransform.position;
            }
        }
    }
}
