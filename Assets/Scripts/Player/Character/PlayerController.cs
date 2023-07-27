using UnityEngine;

public class PlayerController : PhysicsObject
{
    public PlayerStateMachine StateMachine { get; private set; }
    public static PlayerController Instance { get; private set; }

    //Finite player states declarations
    public PlayerGroundedState GroundedState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }

    [Header("Movement Settings")]
    public bool canMove = true;
    public bool stopCharacterNearWalls = false;
    public LayerMask walkableLayers;
    public Transform characterModel;
    public Transform cam; //Used to calculate the direction that the player will move
    public CapsuleCollider characterCollider;
    public float inputSmoothDamp;
    public float movementSpeed;
    public float movementSpeedOnSlope;
    public float movementSpeedOnAir;

    [Tooltip("Angle threshold to consider a surface a slope (0 is a wall and 1 is a plane)")]
    public float slopeAngleTrigger;

    [Tooltip("Max step height to consider the automatic player climb")]
    public float maxStepHeight = 0.4f;

    [Header("Jump Settings")]
    public float jumpForce;
    public float groundDistanceCheck;

    [Tooltip("Duration of the recover of the player after the landing")]
    public float landIdleDuration;
    
    [Tooltip("Duration of preparation of the player before the jump")]
    public float jumpIdleDuration;

    public float smallFallDuration;
    public float normalFallDuration;

    //Input variables
    [HideInInspector] public Inputs inputs;
    [HideInInspector] public Vector2 input;
    [HideInInspector] public Vector2 processedInput; // use this to calculate animations
    [HideInInspector] public bool jumpButtonIsPressed;

    //General condition variables
    [HideInInspector] public Vector3 surfaceNormal;
    [HideInInspector] public string floorTag;
    [HideInInspector] public bool isFalling;
    [HideInInspector] public bool onGround; 
    [HideInInspector] public bool onEdge;
    [HideInInspector] public bool onSlope;
    [HideInInspector] public bool nearWall;

    private RaycastHit hit; 

    private void Awake() {
        //Setup instance to be called from other scripts
        if (Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        //Setup character finite states - code new states here
        StateMachine = new PlayerStateMachine(); //Handle state changes
        GroundedState = new PlayerGroundedState(this, StateMachine);
        JumpState = new PlayerJumpState(this, StateMachine);
        FallState = new PlayerFallState(this, StateMachine);

        //Setup input
        inputs = new Inputs();
        inputs.Enable();
        inputs.Character.Movement.performed += ctx => input = ctx.ReadValue<Vector2>();
        inputs.Character.Movement.canceled += ctx => input = Vector2.zero;
        inputs.Character.Jump.performed += ctx => jumpButtonIsPressed = true;
        inputs.Character.Jump.canceled += ctx => jumpButtonIsPressed = false;
    }

    private void Start() {
        InitializePhysics();
        StateMachine.Initialize(GroundedState);
    }

    private void Update() {
        processedInput = Vector2.Lerp(processedInput, ClampMagnitude(input, 0f, 1f), inputSmoothDamp * Time.deltaTime);
        StateMachine.CurrentState.StateUpdate();

        CheckGround();
    }

    private void FixedUpdate() {
        UpdatePhysics();
        DetectWalls();


        StateMachine.CurrentState.StateFixedUpdate();
    }

    private void DetectWalls() {
        if(stopCharacterNearWalls) {
            float detectWallRayLength = characterCollider.radius + 0.1f;
            if (Physics.Raycast(transform.position - transform.up * 0.3f, characterModel.TransformDirection(Vector3.forward), detectWallRayLength, walkableLayers)) {
                nearWall = true;
            } else {
                nearWall = false;
            }
        } else {
            nearWall = false;
        }
    }

    private void CheckGround() {
        if (Physics.SphereCast(transform.position, 0.15f, -transform.up, out hit, groundDistanceCheck, walkableLayers)) {
            surfaceNormal = hit.normal;
            floorTag = hit.collider.transform.tag;
            onGround = true;
        } else {
            onGround = false;
        }
    }

    //Custom clamp function to define the min and max magnitude
    private Vector2 ClampMagnitude(Vector2 vector, float minMagnitude, float maxMagnitude) {
        float magnitude = Mathf.Clamp(vector.magnitude, minMagnitude, maxMagnitude);
        return vector.normalized * magnitude;
    }

    //This function readjust the model rotation, used in the spaceship script
    public void AdjustModelRotation() {
        Vector3 gravityDirection = GetGravityDirection();
        characterModel.rotation = Quaternion.LookRotation(transform.forward, gravityDirection);
    }
}
