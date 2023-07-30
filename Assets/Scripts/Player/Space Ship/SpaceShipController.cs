using UnityEngine;

public class SpaceShipController : PhysicsObject, Interactable
{
    public SpaceShipStateMachine StateMachine { get; private set; }
    public static SpaceShipController Instance { get; private set; }

    //Finite player states declarations
    public SpaceShipIdle IdleState { get; private set; }
    public SpaceShipOnPlanet OnPlanetState { get; private set; }
    public SpaceShipOnSpace OnSpaceState { get; private set; }

    [Header("Movement Settings")]
    public float changeModeDuration;
    public float movementSpeed;
    public float torqueSpeed;
    public float boostSpeed;
    public float boostCooldown;
    public float boostModeThreshold;
    
    [Header("Camera")]
    public Transform cameraPivot;
    public Vector3 cameraOffset;
    public Vector3 cameraRadius;
    public LayerMask cameraCollisionLayers;

    [Header("Effects")]
    public Animator coreAnimator;
    public GameObject dissolveEffect;
    public GameObject trailEffect;

    //Input
    [HideInInspector] public Inputs inputs;
    private Vector2 inputMove, inputRotate;
    private float inputAltitude;
    private float inputRoll;
    private Vector3 forwardDirection, otherDirections, torqueRotation;

    [HideInInspector] public PlayerController player;
    [HideInInspector] public SpaceShipAudio audioHandler;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        //Setup character finite states - code new states here
        StateMachine = new SpaceShipStateMachine(); //Handle state changes
        IdleState = new SpaceShipIdle(this, StateMachine);
        OnPlanetState = new SpaceShipOnPlanet(this, StateMachine);
        OnSpaceState = new SpaceShipOnSpace(this, StateMachine);

        //Inputs
        inputs = new Inputs();

        //Dependencies
        audioHandler = GetComponent<SpaceShipAudio>();
    }

    private void Start() {
        InitializePhysics();
        StateMachine.Initialize(IdleState);
        player = PlayerController.Instance;
    }

    private void Update() {
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

    public void ProcessCameraMovement() {
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
}
