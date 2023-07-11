using System.Collections;
using UnityEngine;

public class BirdController : Interactable
{
    public static BirdController Instance { get; private set; }

    [Header("Movement Settings")]
    public bool piloting = false;
    public float changeModeDuration;
    public float movementSpeed;
    public float torqueSpeed;
    public float boostSpeed;
    public float boostCooldown;
    public float boostModeThreshold;
    private float boostTimer;
    private bool speedMode = false;
    private bool canCheckBoostModeStatus = true;

    private bool canFreezeRigid = false;
    private float freezeRigidWait = 2f;
    private float freezeVelocityThreshold = 1f;

    [Header("Camera Settings")]
    public Vector3 cameraHalfExtends;
    public Vector3 cameraOffset;
    public float cameraFollowSpeed, cameraRotationSpeed;
    [SerializeField] private Transform cam;
    private bool inCameraTransition = false;

    [Header("Dependencies")]
    public BirdAudioManager audioManager;
    [SerializeField] private Transform player;
    [HideInInspector] [SerializeField] public PhysicsObject physics;
    private Inputs inputs;
    private Vector2 inputMove, inputRotate;
    private float inputAltitude;
    private float inputRoll;
    private Vector3 forwardDirection, otherDirections, torqueRotation;

    [Header("Animation")]
    public Transform[] motorRings;
    public float motorRotationVelocity;
    private float motorCurrentRotationVelocity;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        inputs = new Inputs();
        inputs.Enable();

        inputs.Bird.Exit.performed += ctx => ExitPilotMode();
        inputs.Bird.Move.performed += ctx => inputMove = ctx.ReadValue<Vector2>();
        inputs.Bird.Move.canceled += ctx => inputMove = Vector2.zero;
        inputs.Bird.Altitude.performed += ctx => inputAltitude = ctx.ReadValue<float>();
        inputs.Bird.Altitude.canceled += ctx => inputAltitude = 0;
        inputs.Bird.Rotate.performed += ctx => inputRotate = ctx.ReadValue<Vector2>();
        inputs.Bird.Rotate.canceled += ctx => inputRotate = Vector2.zero;
        inputs.Bird.Roll.performed += ctx => inputRoll = ctx.ReadValue<float>();
        inputs.Bird.Roll.canceled += ctx => inputRoll = 0;
        inputs.Bird.Boost.performed += ctx => Boost();
    }

    private void Start() {
        StartCoroutine(FreezeRigid()); 
    }

    private void Update() {
        boostTimer += Time.deltaTime;
        boostTimer = Mathf.Clamp(boostTimer, 0, boostCooldown);

        if(piloting) 
            BirdInput();
    }

    private void FixedUpdate() {
        if(piloting) {
            ProcessBirdMovement();
            ProcessCameraMovement();
            
            motorCurrentRotationVelocity = motorRotationVelocity * (1 + inputMove.magnitude * 2);
            if(physics.rigid.isKinematic) { 
                canFreezeRigid = false;
                physics.rigid.isKinematic = false;
            }
        } else {
            motorCurrentRotationVelocity = motorRotationVelocity;

            //Freezes the bird physics after sometime still
            if(physics.rigid.velocity.magnitude <= freezeVelocityThreshold && !physics.rigid.isKinematic && canFreezeRigid) 
                physics.rigid.isKinematic = true;
        }

        ProcessAnimation();        
    }

    private void ProcessAnimation() {
        int count = 0;
        foreach(Transform ring in motorRings) {
            if(count % 2 == 0) {
                ring.Rotate(Vector3.up * motorCurrentRotationVelocity * Time.deltaTime);
            } else {
                ring.Rotate(Vector3.up * -motorCurrentRotationVelocity * Time.deltaTime);
            }
            count++;
        }
    }

    private void BirdInput() {
        inputMove = ClampMagnitude(inputMove, 0.4f, 1.0f);
        inputRotate = ClampMagnitude(inputRotate, 0.4f, 1.0f);
        
        forwardDirection = (transform.forward * inputMove.y) * movementSpeed;
        otherDirections = (transform.right * inputMove.x + transform.up * inputAltitude) * movementSpeed;
        torqueRotation = (Vector3.up * inputRotate.x + Vector3.forward * -inputRoll + Vector3.right * -inputRotate.y) * torqueSpeed;
    }

    private void ProcessBirdMovement() {
        float aditionalForce = 1;
        if(speedMode) {
            aditionalForce = boostSpeed;
        }

        physics.rigid.AddForce(forwardDirection * aditionalForce * physics.rigid.mass * Time.deltaTime, ForceMode.Acceleration);
        physics.rigid.AddForce(otherDirections * physics.rigid.mass * Time.deltaTime, ForceMode.Acceleration);

        if(speedMode)
            physics.rigid.AddRelativeTorque((torqueRotation / 2.5f) * Time.deltaTime, ForceMode.VelocityChange);
        else
            physics.rigid.AddRelativeTorque(torqueRotation * Time.deltaTime, ForceMode.VelocityChange);

        if(inputMove.magnitude <= boostModeThreshold && canCheckBoostModeStatus) {
            speedMode = false;
        }
    }

    private void ProcessCameraMovement() {
        if(inCameraTransition) return;

        Vector3 offsetPosition = transform.position + (transform.up * cameraOffset.y);
        Quaternion lookRotation = cam.transform.rotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 camPos;

        if (Physics.BoxCast(offsetPosition, cameraHalfExtends, -lookDirection, out RaycastHit hit, lookRotation, cameraOffset.z - Camera.main.nearClipPlane, CameraController.Instance.cameraCollisionMask)) {
            camPos = offsetPosition - cam.forward * (hit.distance + Camera.main.nearClipPlane);
		} else {
            camPos = offsetPosition - cam.forward * cameraOffset.z;
        }

        cam.rotation = Quaternion.Slerp(cam.rotation, transform.rotation, cameraRotationSpeed * Time.fixedDeltaTime);
        cam.position = Vector3.Lerp(cam.position, camPos, cameraFollowSpeed * Time.fixedDeltaTime);
    }

    private void Boost() {
        if(!piloting) return;
        
        if(boostTimer >= boostCooldown && !speedMode && inputMove.magnitude >= boostModeThreshold) {
            StartCoroutine(CheckBoostModeCooldown());
            audioManager.Boost();
            speedMode = true;
            boostTimer = 0;
        }
    }

    //The code below is for manage the in and out of the bird control state

    //The player will call this method in normal character mode
    public override void Interact() {
        EnterPilotMode();
    }

    //This will hide the player and enter the "Bird Mode" to control the vehicle
    public void EnterPilotMode() {
        if(!PlayerController.Instance.canMove && PlayerController.Instance.reading) return;
        StartCoroutine(EnterPilotModeCoroutine());
        canFreezeRigid = false;
        CameraController.Instance.isActive = false;
        PlayerController.Instance.inputs.Disable();
        inputs.Enable();
        audioManager.EnterShip();
        player.SetParent(transform);
        player.gameObject.SetActive(false);
        physics.userGravitacionalForce = false;
        physics.rigid.freezeRotation = true;
    }

    //This method can only be called in the bird mode
    public void ExitPilotMode() {
        if(!piloting) return;
        StartCoroutine(ExitPilotModeCoroutine());
        StartCoroutine(FreezeRigid());
        CameraController.Instance.isActive = true;
        player.gameObject.SetActive(true);
        player.SetParent(null);
        PlayerController.Instance.inputs.Enable();
        PlayerController.Instance.AdjustModelRotation();
        inputs.Disable();
        audioManager.ExitShip();
        physics.userGravitacionalForce = true;
        physics.rigid.freezeRotation = false;
    }

    private IEnumerator EnterPilotModeCoroutine() {
        yield return new WaitForEndOfFrame();

        piloting = true;
        inCameraTransition = true;
        float elapsedTime = 0;
        float smoothElapsedTime = 0;
        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;

        while (elapsedTime < changeModeDuration)
        {
            Vector3 offsetPosition = transform.position + (transform.up * cameraOffset.y);
            Vector3 camPos = offsetPosition - cam.forward * cameraOffset.z;
            cam.rotation = Quaternion.Slerp(startRot, transform.rotation, smoothElapsedTime);
            cam.position = Vector3.Slerp(startPos, camPos, smoothElapsedTime);
            elapsedTime += Time.fixedDeltaTime;
            smoothElapsedTime = elapsedTime / changeModeDuration;
            smoothElapsedTime = smoothElapsedTime * smoothElapsedTime * (3f - 2f * smoothElapsedTime);
            yield return null;
        }  

        inCameraTransition = false;
    }

    private IEnumerator ExitPilotModeCoroutine() {
        yield return new WaitForEndOfFrame();
        piloting = false;
        CameraController.Instance.inCameraTransition = true;

        float elapsedTime = 0;
        float smoothElapsedTime = 0;
        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;

        while (elapsedTime < changeModeDuration)
        {
            Vector3 offsetPosition = transform.position + (transform.up * cameraOffset.y);
            Vector3 camPos = offsetPosition - cam.forward * cameraOffset.z;
            cam.rotation = Quaternion.Slerp(startRot, CameraController.Instance.lookRotation, smoothElapsedTime);
            cam.position = Vector3.Slerp(startPos, CameraController.Instance.lookPosition, smoothElapsedTime);
            elapsedTime += Time.fixedDeltaTime;
            smoothElapsedTime = elapsedTime / changeModeDuration;
            smoothElapsedTime = smoothElapsedTime * smoothElapsedTime * (3f - 2f * smoothElapsedTime);
            yield return null;
        } 

        CameraController.Instance.inCameraTransition = false; 
    }
    
    //Input process logic
    private Vector2 ClampMagnitude(Vector2 vector, float minMagnitude, float maxMagnitude)
    {
        float magnitude = Mathf.Clamp(vector.magnitude, minMagnitude, maxMagnitude);
        return vector.normalized * magnitude;
    }

        private IEnumerator CheckBoostModeCooldown() {
        canCheckBoostModeStatus = false;
        yield return new WaitForSeconds(0.5f);
        canCheckBoostModeStatus = true;
    }

    // Allows the physics rigidbody to freeze after it stops
    private IEnumerator FreezeRigid() {
        yield return new WaitForSeconds(freezeRigidWait);
        canFreezeRigid = true;
    }
}
