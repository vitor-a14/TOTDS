using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Input Settings")]
    public bool isActive = true;
    public float sensivity;
    [SerializeField] private float maxAngle;

    [Header("Camera Settings")]
    [SerializeField, Range(0f, 15f)] private float distance = 5f;
    public LayerMask cameraCollisionMask;
    [SerializeField, Range(0f, 1f)] private float focusCentering = 0.5f;
    [SerializeField, Min(0f)] private float focusRadius = 1f;

    //Calculation variables
    private Vector2 cameraInput;
    private Vector3 focusPoint;
    private float yaw, pitch;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    private void Start() {
        //Receive input system from character controller script
        CharacterController.Instance.inputs.Gameplay.Camera.performed += ctx => cameraInput = ctx.ReadValue<Vector2>();
        CharacterController.Instance.inputs.Gameplay.Camera.canceled += ctx => cameraInput = Vector2.zero;
        focusPoint = transform.parent.position;
    }

    //Calculate the camera smooth follow position to the transform parent
    private void LateUpdate() {
        if(!isActive) return;

        //Receive input 
        yaw += cameraInput.x * sensivity * Time.deltaTime;
        pitch -= cameraInput.y * sensivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -maxAngle, maxAngle);

        //Smooth following camera logic
        Vector3 targetPoint = transform.parent.position;
        if (focusRadius > 0f)
        {
            float t = 1f;
            float distance = Vector3.Distance(targetPoint, focusPoint);

            if (distance > 0.01f && focusCentering > 0f)
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);

            if (distance > focusRadius)
                t = Mathf.Min(t, focusRadius / distance);

            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
            focusPoint = targetPoint;
    }

    private void FixedUpdate() {
        //Apply rotation
        transform.localEulerAngles = new Vector3(pitch, yaw);

        Quaternion lookRotation = transform.rotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition;

        //Camera collision logic
        if (Physics.Raycast(focusPoint, -lookDirection, out RaycastHit hit, distance, cameraCollisionMask))
            lookPosition = focusPoint - lookDirection * hit.distance + (hit.normal * 0.12f);
        else
            lookPosition = focusPoint - lookDirection * distance;

        //Apply position
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
}
