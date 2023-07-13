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

    [Header("Camera Settings")]
    [Range(0f, 15f)] public float distance = 5f;
    [Range(0f, 1f)] public float focusCentering = 0.5f;
    [Min(0f)] public float focusRadius = 1f;
    public Transform cam; //The position and rotation is set with the rigidbody, not transform - this make a smooth movement with the player controller
    public LayerMask cameraCollisionMask; //The surfaces that the camera will avoid pass through

    private Vector2 cameraInput; //Input received from Engine
    private Vector3 focusPoint; //Target position
    private float yaw, pitch; //Input translated into transform rotation 
    [HideInInspector] public Vector3 lookPosition;
    [HideInInspector] public Quaternion lookRotation;
    [HideInInspector] public bool inCameraTransition = false; //This is used when transitioning between the bird mode and character mode

    public Vector3 CameraHalfExtends;

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
        focusPoint = transform.parent.position;
    }

    private void Update() {
        //Receive input 
        yaw += cameraInput.x * sensivity * Time.deltaTime;
        pitch -= cameraInput.y * sensivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -maxAngle, maxAngle);

        ApplyMotion();
    }

    //Calculate the camera smooth follow position to the transform parent
    private void FixedUpdate() {
        if(isActive && !PlayerController.Instance.reading) 
            CalculateMotion();
    }

    //Input calculation logic, also calculates the camera movement based on the player movement
    public void CalculateMotion() {
        //Smooth following camera logic
        Vector3 targetPoint = transform.position;
        if (focusRadius > 0f) {
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

    //Besides this being applied in the Update method, the movement is setted to the Camera Rigidbody
    //That makes the player movement and camera movement work smoothly together with rigidbody
    public void ApplyMotion() {
        //Apply rotation
        transform.localEulerAngles = new Vector3(pitch, yaw);

        lookRotation = transform.rotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;

        //Camera collision logic
        if (Physics.BoxCast(focusPoint, CameraHalfExtends, -lookDirection, out RaycastHit hit, lookRotation, distance - Camera.main.nearClipPlane, cameraCollisionMask)) {
			lookPosition = focusPoint - lookDirection * (hit.distance + Camera.main.nearClipPlane);
		} else {
            lookPosition = focusPoint - lookDirection * distance;
        }

        //Apply position
        cam.transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
}
