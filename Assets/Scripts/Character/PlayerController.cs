using System.Collections;
using UnityEngine;

public class PlayerController : PhysicsObject
{
    public static PlayerController Instance { get; private set; }
    [HideInInspector] public Inputs inputs;

    [Header("Movement Settings")]
    public bool canMove = true;
    public LayerMask walkableLayers;
    public Transform cam;
    public Transform characterModel;
    public float movementSpeed;
    public float inputSmoothDamp;
    public float detectWallRayLength = 0.25f; //This ray detects if a wall is in front of the character to prevent moving
    public float onAirMovementDecrease = 0;

    private bool shiftWalk = false; //only for keyboard
    private Vector3 processedDirection;
    private Vector3 surfaceNormal;
    [HideInInspector] public string floorTag;
    [HideInInspector] public Vector2 input;
    [HideInInspector] public Vector2 processedInput;
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public bool reading = false;

    [Header("Slope Movement")]
    public float slopeAngleTrigger;
    public float slopeMovementDecrease;

    [Header("Step Up")]
    public float upStepHeight;
    public float lowerRayLength, upperRayLength;

    [HideInInspector] public bool onSlope;
    [HideInInspector] public bool nearWall;

    [Header("Jump Settings")]
    public float jumpForce;
    public float jumpAditionalForce;
    public float groundDistanceCheck;
    public float jumpCooldown;
    public float landIdleDuration;
    public float jumpIdleDuration;
    public Cloth cape;

    private bool canJump = true; //also used to see if the player is on a jump
    private bool jumpButtonIsDown = false;
    private RaycastHit hit;

    [HideInInspector] public bool onGround = true; 
    //To detect if the player hit the ground and activate a callback to the animation
    public bool hitOnGround = false;

    //Instance and input setup
    private void Awake() {
        if (Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        inputs = new Inputs();
        inputs.Enable();

        inputs.Character.Movement.performed += ctx => input = ctx.ReadValue<Vector2>();
        inputs.Character.Movement.canceled += ctx => input = Vector2.zero;
        inputs.Character.Jump.performed += ctx => Jump();
        inputs.Character.Jump.canceled += ctx => jumpButtonIsDown = false;
        inputs.Character.ShiftWalk.performed += ctx => shiftWalk = true;
        inputs.Character.ShiftWalk.canceled += ctx => shiftWalk = false; 

        inputs.Communication.Up.performed += ctx => CommunicationHandler.Instance.AddInteraction(Interaction.UP);        
        inputs.Communication.Down.performed += ctx => CommunicationHandler.Instance.AddInteraction(Interaction.DOWN);        
        inputs.Communication.Right.performed += ctx => CommunicationHandler.Instance.AddInteraction(Interaction.RIGHT);        
        inputs.Communication.Left.performed += ctx => CommunicationHandler.Instance.AddInteraction(Interaction.LEFT);      
        inputs.Communication.Interact.performed += ctx => InteractHandler();
    }

    //Handle screen message show. If the player is already in reading mode, go to next message until it's gone
    //If player is not in reading mode, show the reading screen
    private void InteractHandler() {
        if (!canMove) return; //validate this line

        if (reading) {
            CommunicationHandler.Instance.readingTarget.ShowMessage();
        } else {
            CommunicationHandler.Instance.InteractWithCurrentTarget();
        }
    }

    private void Start() {
        InitializePhysics();
    }

    private void Update() {
        if (!canMove || reading) {
            direction = Vector3.zero;
            return;
        }
        
        Vector3 gravityDirection = GetGravityDirection();
        Vector3 forward = Vector3.Cross(-gravityDirection, cam.right).normalized;
        Vector3 right = Vector3.Cross(-gravityDirection, -cam.forward).normalized;
        float movementMultiplier = 1;

        if (onSlope)
            movementMultiplier = (1 - slopeMovementDecrease);
        else if (!onGround)
            movementMultiplier = (1 - onAirMovementDecrease);
 
        direction = (forward * processedInput.y + right * processedInput.x) * movementSpeed * movementMultiplier;

        if (shiftWalk)
            processedInput = Vector2.Lerp(processedInput, ClampMagnitude(input, 0f, 0.41f), inputSmoothDamp * Time.deltaTime);
        else
            processedInput = Vector2.Lerp(processedInput, ClampMagnitude(input, 0f, 1f), inputSmoothDamp * Time.deltaTime);

        if (direction != Vector3.zero && input != Vector2.zero) {
            Quaternion modelRotation = Quaternion.LookRotation(direction.normalized, gravityDirection);
            characterModel.rotation = Quaternion.Slerp(characterModel.rotation, modelRotation, 15f * Time.deltaTime);
        } else {
            Vector3 forwardDir = Vector3.Cross(gravityDirection, characterModel.right);
            Quaternion modelRotation = Quaternion.LookRotation(-forwardDir, gravityDirection);
            characterModel.rotation = Quaternion.Slerp(characterModel.rotation, modelRotation, 15f * Time.deltaTime);
        }
    }

    private void FixedUpdate() {
        UpdatePhysics();
        CheckGround();
        ApplyMotion();
        StepUp();
    }

    private void ApplyMotion() {
        Vector3 startPos = transform.position + transform.up * 0.1f;

        if (onGround) {
            processedDirection = Vector3.ProjectOnPlane(direction, surfaceNormal);
        } else {
            processedDirection = direction;
        }

        if (!Physics.Linecast(startPos, startPos + characterModel.TransformDirection(Vector3.forward) * detectWallRayLength, walkableLayers)) {
            rigid.position += processedDirection * Time.fixedDeltaTime;
            nearWall = false;
        } else {
            nearWall = true;
        }

        if(jumpButtonIsDown && !canJump) {
            rigid.AddForce(transform.up * jumpAditionalForce, ForceMode.Acceleration);
        }
    }

    public void AdjustModelRotation() {
        Vector3 gravityDirection = GetGravityDirection();
        characterModel.rotation = Quaternion.LookRotation(transform.forward, gravityDirection);
    }

    private void CheckGround() {
        if (Physics.SphereCast(transform.position, 0.15f, -transform.up, out hit, groundDistanceCheck, walkableLayers)) {
            onSlope = Vector3.Dot(transform.up, surfaceNormal) < slopeAngleTrigger ? true : false;
            surfaceNormal = hit.normal;
            floorTag = hit.collider.transform.tag;
            onGround = true;
            if(!hitOnGround) {
                StartCoroutine(HandleLandCoroutine());
            }
        } else {
            onGround = false;
            hitOnGround = false;
        }
    }

    // GO BACK HERE TO IMPROVE //
    private void StepUp() {
        if(!onGround || !canMove || reading) return;

        RaycastHit lowerHit;
        bool lowerRayHit;
        bool upperRayHit;

        //lower ray
        Debug.DrawRay(characterModel.position + transform.up * 0.1f, characterModel.forward * 0.2f, Color.cyan);
        if(Physics.Raycast(characterModel.position + transform.up * 0.1f, characterModel.TransformDirection(Vector3.forward), out lowerHit, lowerRayLength, walkableLayers)) {
            lowerRayHit = true;
        } else {
            lowerRayHit = false;
        }

        //upper ray
        Debug.DrawRay(characterModel.position + transform.up * 0.5f, characterModel.forward * 0.4f, Color.cyan);
        if(Physics.Raycast(characterModel.position + transform.up * 0.5f, characterModel.TransformDirection(Vector3.forward), upperRayLength, walkableLayers)) {
            upperRayHit = true;
        } else {
            upperRayHit = false;
        }

        float stepness = Vector3.Dot(lowerHit.normal, transform.up);

        //steep detected
        if(lowerRayHit && !upperRayHit && direction.sqrMagnitude > 0.4f && stepness < 0.6f) {
            Vector3 origin = characterModel.position + transform.up * 0.5f + characterModel.transform.TransformDirection(Vector3.forward) * upperRayLength;
            Vector3 pos = rigid.position + transform.up * upStepHeight;
            rigid.position = pos;
        }
    }
    
    //Called when the jump button is pressed
    private void Jump() {
        jumpButtonIsDown = true;

        if (onGround && canJump && canMove && !reading) {
            StartCoroutine(HandleJumpCoroutine());
        }
    }

    //Time window to know if the player is pressing the jumping button and apply a higher jump force
    private IEnumerator HandleJumpCoroutine() {
        canMove = false;
        canJump = false;
        CharacterAnimation.Instance.PlayJumpAnim();

        yield return new WaitForSeconds(jumpIdleDuration);

        rigid.AddForce(surfaceNormal * jumpForce, ForceMode.VelocityChange);
        canMove = true;
    }  

    private IEnumerator HandleLandCoroutine() {
        canMove = false;
        hitOnGround = true;
        CharacterAnimation.Instance.PlayLandAnim();
        processedInput = Vector2.zero;
        yield return new WaitForSeconds(landIdleDuration);
        canJump = true;
        canMove = true;
    }

    //A custom clamp magnitude with min and max. The built in unity ClampMagnitude only has the max parameter
    private Vector2 ClampMagnitude(Vector2 vector, float minMagnitude, float maxMagnitude) {
        float magnitude = Mathf.Clamp(vector.magnitude, minMagnitude, maxMagnitude);
        return vector.normalized * magnitude;
    }
}
