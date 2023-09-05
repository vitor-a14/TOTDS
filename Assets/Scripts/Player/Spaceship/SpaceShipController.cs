using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SpaceshipController : PhysicsObject, Interactable
{
    public SpaceshipStateMachine StateMachine { get; private set; }
    public static SpaceshipController Instance { get; private set; }

    //Finite player states declarations
    public SpaceshipIdle IdleState { get; private set; }
    public SpaceshipOnPlanet OnPlanetState { get; private set; }
    public SpaceshipOnSpace OnSpaceState { get; private set; }

    [Header("Movement Settings")]
    public float maxSpeed;
    public float torque;
    public float acceleration;

    public float boostMultiplier;
    public float boostTorqueFriction;

    [Header("Avoid Collisions")]
    public float avoidanceForce;
    public float avoidanceDistance;
    
    [Header("Camera")]
    public CinemachineVirtualCamera cam;
    public Transform cameraPivot;
    public Vector3 cameraOffset;
    public Vector3 cameraRadius;
    public LayerMask cameraCollisionLayers;
    public float cameraVelocity;

    [Header("Effects")]
    public Animator coreAnimator;
    public GameObject dissolveEffect;
    public GameObject trailEffect;
    public GameObject dustEffect;
    public Transform[] motorRings;

    //Input
    [HideInInspector] public Inputs inputs;
    [HideInInspector] public Vector2 movementInput;
    [HideInInspector] public Vector2 rotationInput;
    [HideInInspector] public float inputAltitude;
    [HideInInspector] public float inputRoll;

    [HideInInspector] public PlayerController player;
    [HideInInspector] public SpaceShipAudio audioHandler;
    [HideInInspector] public Vector3 playerTeleportPoint;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        //Setup character finite states - code new states here
        StateMachine = new SpaceshipStateMachine(); //Handle state changes
        IdleState = new SpaceshipIdle(this, StateMachine);
        OnPlanetState = new SpaceshipOnPlanet(this, StateMachine);
        OnSpaceState = new SpaceshipOnSpace(this, StateMachine);

        //Inputs
        inputs = new Inputs();
        
        inputs.Bird.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputs.Bird.Move.canceled += ctx => movementInput = Vector2.zero;

        inputs.Bird.Rotate.performed += ctx => rotationInput = ctx.ReadValue<Vector2>();
        inputs.Bird.Rotate.canceled += ctx => rotationInput = Vector2.zero;

        inputs.Bird.Altitude.performed += ctx => inputAltitude = ctx.ReadValue<float>();
        inputs.Bird.Altitude.canceled += ctx => inputAltitude = 0;

        inputs.Bird.Roll.performed += ctx => inputRoll = ctx.ReadValue<float>();
        inputs.Bird.Roll.canceled += ctx => inputRoll = 0;

        inputs.Communication.Interact.performed += ctx => Interact();
        inputs.Bird.Boost.performed += ctx => Boost();

        //Dependencies
        audioHandler = GetComponent<SpaceShipAudio>();
    }

    private void Start() {
        InitializePhysics();
        player = PlayerController.Instance;
        StateMachine.Initialize(IdleState);
    }

    private void Update() {
        movementInput = player.ClampMagnitude(movementInput, -1.0f, 1.0f);
        rotationInput = player.ClampMagnitude(rotationInput, 0.0f, 1.0f);
        ProcessRingsAnimation();

        StateMachine.CurrentState.StateUpdate();
    }

    private void FixedUpdate() {
        UpdatePhysics();
        StateMachine.CurrentState.StateFixedUpdate();
    }

    //From interaction system
    public void Interact() {
        StateMachine.CurrentState.Interact();
    }

    public void Boost() {
        StateMachine.CurrentState.Boost();
    }

    public void ProcessCameraMovement(bool useLerp) {
        Vector3 offsetPosition = transform.position + (transform.up * cameraOffset.y);
        Quaternion lookRotation = cameraPivot.transform.rotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 camPos;

        if (Physics.BoxCast(offsetPosition, cameraRadius, -lookDirection, out RaycastHit hit, lookRotation, cameraOffset.z - Camera.main.nearClipPlane, cameraCollisionLayers)) {
            camPos = offsetPosition - cameraPivot.forward * (hit.distance + Camera.main.nearClipPlane);
		} else {
            camPos = offsetPosition - cameraPivot.forward * cameraOffset.z;
        }

        if(useLerp) {
            cam.transform.position = Vector3.Lerp(cam.transform.position, camPos, cameraVelocity * Time.fixedDeltaTime);
            cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, cameraPivot.rotation, cameraVelocity * Time.fixedDeltaTime);
        } else {
            cam.transform.position = camPos;
            cam.transform.rotation = cameraPivot.rotation;
        }
    }

    public Vector3 FindPlayerTeleportPoint() {
        Vector3 frontPoint = transform.position + transform.forward * 10f;
        Vector3 backPoint = transform.position - transform.forward * 3f;
        Vector3 dir = transform.TransformDirection(-Vector3.up);
        float rayDistance = 5f;

        if(Physics.Raycast(frontPoint, dir, out RaycastHit hit1, rayDistance, player.walkableLayers)) {
            return hit1.point + hit1.normal * 0.2f;
        }

        if(Physics.Raycast(backPoint, dir, out RaycastHit hit2, rayDistance, player.walkableLayers)) {
            return hit2.point + hit1.normal * 0.2f;
        }

        return Vector3.zero;
    }

    public void AvoidCollisions() {
        Vector3 rightWing = transform.position + transform.right * 5f - transform.forward * 1;
        Vector3 leftWing = transform.position - transform.right * 5f - transform.forward * 1;
        Vector3 front = transform.position + transform.forward * 5f - transform.right * 5f;
        Vector3 back = transform.position + transform.forward * 5f + transform.right * 5f;

        //Sides
        if(Physics.Raycast(front, -GetGravityDirection(), out RaycastHit frontHit, avoidanceDistance, player.walkableLayers)) {
            float anchorForce = Mathf.Abs(1 / Vector3.Distance(frontHit.point, transform.position));
            rigid.AddForceAtPosition(GetGravityDirection() * anchorForce * avoidanceForce, front, ForceMode.Acceleration);
        }

        if(Physics.Raycast(back, -GetGravityDirection(), out RaycastHit backHit, avoidanceDistance, player.walkableLayers)) {
            float anchorForce = Mathf.Abs(1 / Vector3.Distance(backHit.point, transform.position));
            rigid.AddForceAtPosition(GetGravityDirection() * anchorForce * avoidanceForce, back, ForceMode.Acceleration);
        }

        if(Physics.Raycast(rightWing, -GetGravityDirection(), out RaycastHit rightHit, avoidanceDistance, player.walkableLayers)) {
            float anchorForce = Mathf.Abs(1 / Vector3.Distance(rightHit.point, transform.position));
            rigid.AddForceAtPosition(GetGravityDirection() * anchorForce * avoidanceForce, rightWing, ForceMode.Acceleration);
        }

        if(Physics.Raycast(leftWing, -GetGravityDirection(), out RaycastHit leftHit, avoidanceDistance, player.walkableLayers)) {
            float anchorForce = Mathf.Abs(1 / Vector3.Distance(leftHit.point, transform.position));
            rigid.AddForceAtPosition(GetGravityDirection() * anchorForce * avoidanceForce, leftWing, ForceMode.Acceleration);
        }
    }

    private void ProcessRingsAnimation() {
        int count = 0;
        foreach(Transform ring in motorRings) {
            if(count % 2 == 0) {
                ring.Rotate(Vector3.forward * 15f * Time.deltaTime);
            } else {
                ring.Rotate(Vector3.forward * -15f * Time.deltaTime);
            }
            count++;
        }
    }
}
