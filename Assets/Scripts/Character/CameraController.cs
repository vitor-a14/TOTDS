using UnityEngine;

//This script will go in a Camera Pivot Object, this Pivot need's to be a child of the target
//The main camera doesn't need any parent objects
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Input Settings")]
    public bool isActive = true;
    public float sensivity;
    public float maxAngle;
    public float smoothVelocity;

    private Vector2 cameraInput; //Input received from Engine
    private Vector3 focusPoint; //Target position
    private float yaw, pitch; //Input translated into transform rotation 

    private float smothedPitch, smothedYaw;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    private void Start() {
        //Receive input system from character controller script
        PlayerController.Instance.inputs.Character.Camera.performed += ctx => cameraInput = ctx.ReadValue<Vector2>();
        PlayerController.Instance.inputs.Character.Camera.canceled += ctx => cameraInput = Vector2.zero;
    }

    //Apply camera motion, this will be passed to cinemachine
    private void Update() {
        yaw += cameraInput.x * sensivity * Time.deltaTime;
        pitch -= cameraInput.y * sensivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -maxAngle, maxAngle);

        smothedPitch = Mathf.Lerp(smothedPitch, pitch, smoothVelocity * Time.deltaTime);
        smothedYaw = Mathf.Lerp(smothedYaw, yaw, smoothVelocity * Time.deltaTime);

        if(isActive) {
            transform.localEulerAngles = new Vector3(smothedPitch, smothedYaw);
        }
    }
}
