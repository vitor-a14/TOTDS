using System.Collections;
using UnityEngine;

public class BirdController : MonoBehaviour, Interactable
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

    [Header("Dependencies")]
    public SpaceShipAudio audioManager;
    [SerializeField] private Transform playerTransform;
    [HideInInspector] [SerializeField] public PhysicsObject physics;
    private Inputs inputs;
    private Vector2 inputMove, inputRotate;
    private float inputAltitude;
    private float inputRoll;
    private Vector3 forwardDirection, otherDirections, torqueRotation;
    public Cloth characterCape;

    [Header("Camera")]
    public Transform cameraPivot;
    public Vector3 cameraOffset;
    public Vector3 cameraRadius;
    public float cameraFollowSpeed;
    public float cameraRotationSpeed;
    public LayerMask cameraCollisionLayers;

    [Header("Animation")]
    public Transform[] motorRings;
    public float motorRotationVelocity;
    private float motorCurrentRotationVelocity;

    private PlayerController player;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        player = PlayerController.Instance;

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

        if(piloting) {
            BirdInput();

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

    private void FixedUpdate() {
        if(piloting) {
            ProcessBirdMovement();
            ProcessCameraMovement();
        }
    }

    private void ProcessCameraMovement() {
        Vector3 offsetPosition = transform.position + (transform.up * cameraOffset.y);
        Quaternion lookRotation = cameraPivot.transform.rotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 camPos;

        if (Physics.BoxCast(offsetPosition, cameraRadius, -lookDirection, out RaycastHit hit, lookRotation, cameraOffset.z - Camera.main.nearClipPlane, cameraCollisionLayers)) {
            camPos = offsetPosition - cameraPivot.forward * (hit.distance + Camera.main.nearClipPlane);
		} else {
            camPos = offsetPosition - cameraPivot.forward * cameraOffset.z;
        }

        cameraPivot.rotation = cameraPivot.rotation;
        cameraPivot.position = camPos;
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
            physics.rigid.AddRelativeTorque((torqueRotation / 2.5f) * Time.fixedDeltaTime, ForceMode.VelocityChange);
            aditionalForce = boostSpeed;
        } else {
            physics.rigid.AddRelativeTorque(torqueRotation * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        physics.rigid.AddForce(forwardDirection * aditionalForce * physics.rigid.mass * Time.fixedDeltaTime, ForceMode.Acceleration);
        physics.rigid.AddForce(otherDirections * physics.rigid.mass * Time.fixedDeltaTime, ForceMode.Acceleration);

        if(inputMove.magnitude <= boostModeThreshold && canCheckBoostModeStatus) {
            speedMode = false;
        }
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
    public void Interact() {
        EnterPilotMode();
    }

    //This will hide the player and enter the "Bird Mode" to control the vehicle
    public void EnterPilotMode() {
        if(!player.canMove) return;
        CameraController.Instance.isActive = false;
        piloting = true;
        canFreezeRigid = false;
        player.inputs.Disable();
        CameraManager.Instance.ChangeToBirdCamera();
        inputs.Enable();
        audioManager.EnterShip();
        playerTransform.SetParent(transform);
        player.gameObject.SetActive(false);
        physics.useGravitacionalForce = false;
        physics.rigid.freezeRotation = true;
    }

    //This method can only be called in the bird mode
    public void ExitPilotMode() {
        if(!piloting) return;
        CameraController.Instance.isActive = true;
        piloting = false;
        StartCoroutine(FreezeRigid());
        player.gameObject.SetActive(true);
        playerTransform.SetParent(null);
        player.inputs.Enable();
        player.AdjustModelRotation();
        player.SetRotationToGravityDirection();
        characterCape.ClearTransformMotion();
        CameraManager.Instance.ChangeToCharacterCamera();
        inputs.Disable();
        audioManager.ExitShip();
        physics.useGravitacionalForce = true;
        physics.rigid.freezeRotation = false;
    }
    
    //Input process logic
    private Vector2 ClampMagnitude(Vector2 vector, float minMagnitude, float maxMagnitude) {
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